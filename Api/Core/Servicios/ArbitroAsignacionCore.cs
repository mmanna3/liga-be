using System.Globalization;
using System.Text.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Entidades.EntidadesConValoresPredefinidos;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Servicios;

public class ArbitroAsignacionCore : IArbitroAsignacionCore
{
    private static readonly CultureInfo CulturaEsAr = CultureInfo.GetCultureInfo("es-AR");

    private readonly AppDbContext _context;
    private readonly IArbitroJornadaRepo _arbitroJornadaRepo;
    private readonly IFaseCategoriaRepo _faseCategoriaRepo;
    private readonly IBDVirtual _bdVirtual;
    private readonly IRelojZonaHorariaArgentina _relojArgentina;

    public ArbitroAsignacionCore(
        AppDbContext context,
        IArbitroJornadaRepo arbitroJornadaRepo,
        IFaseCategoriaRepo faseCategoriaRepo,
        IBDVirtual bdVirtual,
        IRelojZonaHorariaArgentina relojArgentina)
    {
        _context = context;
        _arbitroJornadaRepo = arbitroJornadaRepo;
        _faseCategoriaRepo = faseCategoriaRepo;
        _bdVirtual = bdVirtual;
        _relojArgentina = relojArgentina;
    }

    public async Task<AsignacionArbitrosPorAgrupadorDTO> ObtenerAsignacionPorAgrupador(int agrupadorId, int anio)
    {
        var agrupadorExiste = await _context.TorneoAgrupadores.AnyAsync(t => t.Id == agrupadorId);
        if (!agrupadorExiste)
            throw new ExcepcionControlada("El agrupador de torneo no existe.");

        var hoy = DateOnly.FromDateTime(_relojArgentina.AhoraLocal.Date);

        var torneos = await _context.Torneos
            .AsNoTracking()
            .Where(t => t.TorneoAgrupadorId == agrupadorId && t.Anio == anio)
            .OrderBy(t => t.Nombre)
            .ToListAsync();

        var torneoIds = torneos.Select(t => t.Id).ToList();

        var fases = await _context.Fases
            .AsNoTracking()
            .Where(f => torneoIds.Contains(f.TorneoId))
            .OrderBy(f => f.Numero)
            .ToListAsync();

        var faseIds = fases.Select(f => f.Id).ToList();

        var categoriasPorFaseId = (await _faseCategoriaRepo.ListarPorFaseIds(faseIds))
            .GroupBy(c => c.FaseId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(c => new FaseCategoriaDTO
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    AnioDesde = c.AnioDesde,
                    AnioHasta = c.AnioHasta,
                    Orden = c.Orden,
                    FaseId = c.FaseId
                }).ToList());

        var zonas = await _context.Zonas
            .AsNoTracking()
            .Where(z => faseIds.Contains(z.FaseId))
            .OrderBy(z => z.Orden)
            .ToListAsync();

        var zonaIds = zonas.Select(z => z.Id).ToList();

        var fechasFuturas = await _context.Fechas
            .AsNoTracking()
            .Where(f => zonaIds.Contains(f.ZonaId) && f.Dia != null && f.Dia > hoy)
            .ToListAsync();

        var instanciasPorId = await _context.InstanciaEliminacionDirecta
            .AsNoTracking()
            .ToDictionaryAsync(i => i.Id, i => i.Nombre);

        var proximaFechaPorZona = fechasFuturas
            .GroupBy(f => f.ZonaId)
            .ToDictionary(
                g => g.Key,
                g => SeleccionarProximaFecha(g.ToList(), instanciasPorId));

        var proximaFechaIds = proximaFechaPorZona.Values
            .Where(f => f != null)
            .Select(f => f!.Id)
            .ToList();

        var jornadasNormales = await _context.Jornadas
            .OfType<JornadaNormal>()
            .AsNoTracking()
            .Include(j => j.LocalEquipo).ThenInclude(e => e.Club)
            .Include(j => j.VisitanteEquipo)
            .Where(j => proximaFechaIds.Contains(j.FechaId))
            .OrderBy(j => j.Id)
            .ToListAsync();

        var jornadasPorFechaId = jornadasNormales
            .GroupBy(j => j.FechaId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var jornadaIds = jornadasNormales.Select(j => j.Id).ToList();
        var asignaciones = await _arbitroJornadaRepo.ListarPorJornadaIds(jornadaIds);
        var asignacionesPorJornadaId = asignaciones
            .GroupBy(a => a.JornadaId)
            .ToDictionary(g => g.Key, g => g.OrderBy(a => a.Orden).ToList());

        var fechasPasadas = await _context.Fechas
            .AsNoTracking()
            .Where(f => zonaIds.Contains(f.ZonaId) && f.Dia != null && f.Dia <= hoy)
            .ToListAsync();

        var ultimasDosFechasPorZona = fechasPasadas
            .GroupBy(f => f.ZonaId)
            .ToDictionary(
                g => g.Key,
                g => OrdenarFechasHistoricas(g.ToList()).Take(2).ToList());

        var ultimasFechaIds = ultimasDosFechasPorZona.Values
            .SelectMany(fechas => fechas)
            .Select(f => f.Id)
            .Distinct()
            .ToList();

        var metadataPorFechaId = new Dictionary<int, (int ZonaId, int? Numero, string? InstanciaNombre)>();
        foreach (var (zonaId, fechasDeZona) in ultimasDosFechasPorZona)
        {
            foreach (var fecha in fechasDeZona)
            {
                var numero = (fecha as FechaTodosContraTodos)?.Numero;
                string? instanciaNombre = null;
                if (fecha is FechaEliminacionDirecta fed)
                    instanciasPorId.TryGetValue(fed.InstanciaId, out instanciaNombre);
                metadataPorFechaId[fecha.Id] = (zonaId, numero, instanciaNombre);
            }
        }

        var jornadasRecientes = ultimasFechaIds.Count == 0
            ? []
            : await _context.Jornadas
                .OfType<JornadaNormal>()
                .AsNoTracking()
                .Include(j => j.LocalEquipo)
                .Include(j => j.VisitanteEquipo)
                .Where(j => ultimasFechaIds.Contains(j.FechaId))
                .ToListAsync();

        var jornadaRecienteIds = jornadasRecientes.Select(j => j.Id).ToList();
        var asignacionesRecientes = jornadaRecienteIds.Count == 0
            ? []
            : await _arbitroJornadaRepo.ListarPorJornadaIds(jornadaRecienteIds);

        var recientesPorArbitroId = new Dictionary<int, List<JornadaAsignadaRecienteDTO>>();
        foreach (var asignacionReciente in asignacionesRecientes)
        {
            var jornadaReciente = jornadasRecientes.First(j => j.Id == asignacionReciente.JornadaId);
            if (!metadataPorFechaId.TryGetValue(jornadaReciente.FechaId, out var meta))
                continue;

            var dto = new JornadaAsignadaRecienteDTO
            {
                ZonaId = meta.ZonaId,
                JornadaId = jornadaReciente.Id,
                FechaNumero = meta.Numero,
                InstanciaNombre = meta.InstanciaNombre,
                LocalEquipoId = jornadaReciente.LocalEquipoId,
                VisitanteEquipoId = jornadaReciente.VisitanteEquipoId,
                Local = jornadaReciente.LocalEquipo.Nombre,
                Visitante = jornadaReciente.VisitanteEquipo.Nombre
            };

            if (!recientesPorArbitroId.TryGetValue(asignacionReciente.ArbitroId, out var lista))
            {
                lista = [];
                recientesPorArbitroId[asignacionReciente.ArbitroId] = lista;
            }

            lista.Add(dto);
        }

        var arbitrosElegibles = await _context.Arbitros
            .AsNoTracking()
            .Include(a => a.ArbitroTorneoAgrupadores)
            .Include(a => a.ArbitroEquiposProhibidos)
            .Where(a => a.ArbitroTorneoAgrupadores.Any(x => x.TorneoAgrupadorId == agrupadorId))
            .OrderBy(a => a.Apellido).ThenBy(a => a.Nombre)
            .ToListAsync();

        var torneosDto = new List<TorneoAsignacionDTO>();
        var resumenesPorJornadaId = new Dictionary<int, JornadaAsignadaResumenDTO>();

        foreach (var torneo in torneos)
        {
            var fasesDelTorneo = fases.Where(f => f.TorneoId == torneo.Id).ToList();
            var fasesDto = new List<FaseAsignacionDTO>();

            foreach (var fase in fasesDelTorneo)
            {
                var zonasDeFase = zonas.Where(z => z.FaseId == fase.Id).ToList();
                var zonasDto = new List<ZonaAsignacionDTO>();

                foreach (var zona in zonasDeFase)
                {
                    ProximaFechaAsignacionDTO? proximaFechaDto = null;

                    if (proximaFechaPorZona.TryGetValue(zona.Id, out var proximaFecha) && proximaFecha?.Dia != null)
                    {
                        var dia = proximaFecha.Dia.Value;
                        var diaSemana = FormatearDiaSemana(dia);
                        var numero = (proximaFecha as FechaTodosContraTodos)?.Numero;
                        string? instanciaNombre = null;
                        if (proximaFecha is FechaEliminacionDirecta fed)
                            instanciasPorId.TryGetValue(fed.InstanciaId, out instanciaNombre);

                        var jornadasDto = new List<JornadaAsignacionDTO>();
                        var faseNombre = string.IsNullOrWhiteSpace(fase.Nombre)
                            ? $"Fase {fase.Numero}"
                            : fase.Nombre;

                        if (jornadasPorFechaId.TryGetValue(proximaFecha.Id, out var jornadasDeFecha))
                        {
                            foreach (var jornada in jornadasDeFecha)
                            {
                                var arbitrosAsignados = asignacionesPorJornadaId.TryGetValue(jornada.Id, out var asigs)
                                    ? asigs.Select(MapArbitroAsignado).ToList()
                                    : [];

                                var jornadaDto = new JornadaAsignacionDTO
                                {
                                    Id = jornada.Id,
                                    Dia = dia,
                                    DiaSemana = diaSemana,
                                    TorneoNombre = torneo.Nombre,
                                    FaseNombre = faseNombre,
                                    ZonaNombre = zona.Nombre,
                                    ZonaId = zona.Id,
                                    Local = jornada.LocalEquipo.Nombre,
                                    Visitante = jornada.VisitanteEquipo.Nombre,
                                    LocalEquipoId = jornada.LocalEquipoId,
                                    VisitanteEquipoId = jornada.VisitanteEquipoId,
                                    NombreClubLocal = jornada.LocalEquipo.Club.Nombre,
                                    DireccionLocal = jornada.LocalEquipo.Club.Direccion,
                                    LocalidadLocal = jornada.LocalEquipo.Club.Localidad,
                                    ArbitrosAsignados = arbitrosAsignados
                                };
                                jornadasDto.Add(jornadaDto);

                                resumenesPorJornadaId[jornada.Id] = new JornadaAsignadaResumenDTO
                                {
                                    JornadaId = jornada.Id,
                                    Dia = dia,
                                    DiaSemana = diaSemana,
                                    TorneoNombre = torneo.Nombre,
                                    FaseNombre = faseNombre,
                                    ZonaNombre = zona.Nombre,
                                    Local = jornada.LocalEquipo.Nombre,
                                    Visitante = jornada.VisitanteEquipo.Nombre,
                                    LocalidadLocal = jornada.LocalEquipo.Club.Localidad,
                                    FechaNumero = numero,
                                    InstanciaNombre = instanciaNombre,
                                    Orden = 0
                                };
                            }
                        }

                        proximaFechaDto = new ProximaFechaAsignacionDTO
                        {
                            FechaId = proximaFecha.Id,
                            Dia = dia,
                            DiaSemana = diaSemana,
                            Numero = numero,
                            InstanciaNombre = instanciaNombre,
                            Jornadas = jornadasDto
                        };
                    }

                    if (proximaFechaDto != null)
                    {
                        zonasDto.Add(new ZonaAsignacionDTO
                        {
                            Id = zona.Id,
                            Nombre = zona.Nombre,
                            ProximaFecha = proximaFechaDto
                        });
                    }
                }

                if (zonasDto.Count > 0)
                {
                    fasesDto.Add(new FaseAsignacionDTO
                    {
                        Id = fase.Id,
                        Nombre = string.IsNullOrWhiteSpace(fase.Nombre) ? $"Fase {fase.Numero}" : fase.Nombre,
                        Categorias = categoriasPorFaseId.TryGetValue(fase.Id, out var categorias)
                            ? categorias
                            : [],
                        Zonas = zonasDto
                    });
                }
            }

            if (fasesDto.Count > 0)
            {
                torneosDto.Add(new TorneoAsignacionDTO
                {
                    Id = torneo.Id,
                    Nombre = torneo.Nombre,
                    HorarioDeJuego = torneo.HorarioDeJuego,
                    Fases = fasesDto
                });
            }
        }

        var arbitrosElegiblesDto = arbitrosElegibles.Select(a =>
        {
            var jornadasDelArbitro = asignaciones
                .Where(x => x.ArbitroId == a.Id && resumenesPorJornadaId.ContainsKey(x.JornadaId))
                .Select(x =>
                {
                    var baseResumen = resumenesPorJornadaId[x.JornadaId];
                    return new JornadaAsignadaResumenDTO
                    {
                        JornadaId = baseResumen.JornadaId,
                        Dia = baseResumen.Dia,
                        DiaSemana = baseResumen.DiaSemana,
                        TorneoNombre = baseResumen.TorneoNombre,
                        FaseNombre = baseResumen.FaseNombre,
                        ZonaNombre = baseResumen.ZonaNombre,
                        Local = baseResumen.Local,
                        Visitante = baseResumen.Visitante,
                        LocalidadLocal = baseResumen.LocalidadLocal,
                        FechaNumero = baseResumen.FechaNumero,
                        InstanciaNombre = baseResumen.InstanciaNombre,
                        Orden = x.Orden,
                        Whatsapp = MapWhatsappAsignacion(x)
                    };
                })
                .OrderBy(j => j.Dia).ThenBy(j => j.TorneoNombre)
                .ToList();

            return new ArbitroElegibleAsignacionDTO
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Apellido = a.Apellido,
                TelefonoCelular = a.TelefonoCelular,
                JornadasAsignadasEnProximasFechas = jornadasDelArbitro,
                EquiposProhibidosIds = a.ArbitroEquiposProhibidos.Select(x => x.EquipoId).ToList(),
                JornadasEnUltimasFechas = recientesPorArbitroId.TryGetValue(a.Id, out var recientes)
                    ? recientes
                    : []
            };
        }).ToList();

        var arbitrosConJornadasDto = arbitrosElegiblesDto
            .Select(a => new ArbitroConJornadasAsignacionDTO
            {
                ArbitroId = a.Id,
                Nombre = a.Nombre,
                Apellido = a.Apellido,
                JornadasProximaFecha = a.JornadasAsignadasEnProximasFechas
            })
            .ToList();

        return new AsignacionArbitrosPorAgrupadorDTO
        {
            ArbitrosElegibles = arbitrosElegiblesDto,
            Torneos = torneosDto,
            ArbitrosConJornadas = arbitrosConJornadasDto
        };
    }

    public async Task<AsignacionHistoricaArbitrosPorAgrupadorDTO> ObtenerAsignacionHistoricaPorAgrupador(
        int agrupadorId,
        int anio)
    {
        var agrupadorExiste = await _context.TorneoAgrupadores.AnyAsync(t => t.Id == agrupadorId);
        if (!agrupadorExiste)
            throw new ExcepcionControlada("El agrupador de torneo no existe.");

        var hoy = DateOnly.FromDateTime(_relojArgentina.AhoraLocal.Date);

        var torneos = await _context.Torneos
            .AsNoTracking()
            .Where(t => t.TorneoAgrupadorId == agrupadorId && t.Anio == anio)
            .OrderBy(t => t.Nombre)
            .ToListAsync();

        var torneoIds = torneos.Select(t => t.Id).ToList();

        var fases = await _context.Fases
            .AsNoTracking()
            .Where(f => torneoIds.Contains(f.TorneoId))
            .OrderBy(f => f.Numero)
            .ToListAsync();

        var faseIds = fases.Select(f => f.Id).ToList();

        var categoriasPorFaseId = (await _faseCategoriaRepo.ListarPorFaseIds(faseIds))
            .GroupBy(c => c.FaseId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(c => new FaseCategoriaDTO
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    AnioDesde = c.AnioDesde,
                    AnioHasta = c.AnioHasta,
                    Orden = c.Orden,
                    FaseId = c.FaseId
                }).ToList());

        var zonas = await _context.Zonas
            .AsNoTracking()
            .Where(z => faseIds.Contains(z.FaseId))
            .OrderBy(z => z.Orden)
            .ToListAsync();

        var zonaIds = zonas.Select(z => z.Id).ToList();

        var fechasPasadas = await _context.Fechas
            .AsNoTracking()
            .Where(f => zonaIds.Contains(f.ZonaId) && f.Dia != null && f.Dia <= hoy)
            .ToListAsync();

        var instanciasPorId = await _context.InstanciaEliminacionDirecta
            .AsNoTracking()
            .ToDictionaryAsync(i => i.Id, i => i.Nombre);

        var fechasPorZona = fechasPasadas
            .GroupBy(f => f.ZonaId)
            .ToDictionary(g => g.Key, g => OrdenarFechasHistoricas(g.ToList()));

        var fechaIds = fechasPasadas.Select(f => f.Id).ToList();

        var jornadasNormales = await _context.Jornadas
            .OfType<JornadaNormal>()
            .AsNoTracking()
            .Include(j => j.LocalEquipo).ThenInclude(e => e.Club)
            .Include(j => j.VisitanteEquipo)
            .Where(j => fechaIds.Contains(j.FechaId))
            .OrderBy(j => j.Id)
            .ToListAsync();

        var jornadaIds = jornadasNormales.Select(j => j.Id).ToList();
        var asignaciones = await _arbitroJornadaRepo.ListarPorJornadaIds(jornadaIds);
        var asignacionesPorJornadaId = asignaciones
            .GroupBy(a => a.JornadaId)
            .ToDictionary(g => g.Key, g => g.OrderBy(a => a.Orden).ToList());

        var arbitrosElegibles = await _context.Arbitros
            .AsNoTracking()
            .Include(a => a.ArbitroTorneoAgrupadores)
            .Include(a => a.ArbitroEquiposProhibidos)
            .Where(a => a.ArbitroTorneoAgrupadores.Any(x => x.TorneoAgrupadorId == agrupadorId))
            .OrderBy(a => a.Apellido).ThenBy(a => a.Nombre)
            .ToListAsync();

        var torneosDto = new List<TorneoAsignacionHistoricaDTO>();
        var resumenesPorJornadaId = new Dictionary<int, JornadaAsignadaResumenDTO>();

        foreach (var torneo in torneos)
        {
            var fasesDelTorneo = fases.Where(f => f.TorneoId == torneo.Id).ToList();
            var fasesDto = new List<FaseAsignacionHistoricaDTO>();

            foreach (var fase in fasesDelTorneo)
            {
                var zonasDeFase = zonas.Where(z => z.FaseId == fase.Id).ToList();
                var zonasDto = new List<ZonaAsignacionHistoricaDTO>();
                var faseNombre = string.IsNullOrWhiteSpace(fase.Nombre)
                    ? $"Fase {fase.Numero}"
                    : fase.Nombre;

                foreach (var zona in zonasDeFase)
                {
                    if (!fechasPorZona.TryGetValue(zona.Id, out var fechasDeZona))
                        continue;

                    var fechasHistoricasDto = new List<FechaHistoricaAsignacionDTO>();

                    foreach (var fecha in fechasDeZona)
                    {
                        if (fecha.Dia == null)
                            continue;

                        var dia = fecha.Dia.Value;
                        var diaSemana = FormatearDiaSemana(dia);
                        var numero = (fecha as FechaTodosContraTodos)?.Numero;
                        string? instanciaNombre = null;
                        if (fecha is FechaEliminacionDirecta fed)
                            instanciasPorId.TryGetValue(fed.InstanciaId, out instanciaNombre);

                        var jornadasDto = new List<JornadaAsignacionDTO>();
                        var jornadasDeFecha = jornadasNormales
                            .Where(j => j.FechaId == fecha.Id)
                            .ToList();

                        foreach (var jornada in jornadasDeFecha)
                        {
                            var arbitrosAsignados = asignacionesPorJornadaId.TryGetValue(jornada.Id, out var asigs)
                                ? asigs.Select(MapArbitroAsignado).ToList()
                                : [];

                            var jornadaDto = new JornadaAsignacionDTO
                            {
                                Id = jornada.Id,
                                Dia = dia,
                                DiaSemana = diaSemana,
                                TorneoNombre = torneo.Nombre,
                                FaseNombre = faseNombre,
                                ZonaNombre = zona.Nombre,
                                ZonaId = zona.Id,
                                Local = jornada.LocalEquipo.Nombre,
                                Visitante = jornada.VisitanteEquipo.Nombre,
                                LocalEquipoId = jornada.LocalEquipoId,
                                VisitanteEquipoId = jornada.VisitanteEquipoId,
                                NombreClubLocal = jornada.LocalEquipo.Club.Nombre,
                                DireccionLocal = jornada.LocalEquipo.Club.Direccion,
                                LocalidadLocal = jornada.LocalEquipo.Club.Localidad,
                                ArbitrosAsignados = arbitrosAsignados
                            };
                            jornadasDto.Add(jornadaDto);

                            resumenesPorJornadaId[jornada.Id] = new JornadaAsignadaResumenDTO
                            {
                                JornadaId = jornada.Id,
                                Dia = dia,
                                DiaSemana = diaSemana,
                                TorneoNombre = torneo.Nombre,
                                FaseNombre = faseNombre,
                                ZonaNombre = zona.Nombre,
                                Local = jornada.LocalEquipo.Nombre,
                                Visitante = jornada.VisitanteEquipo.Nombre,
                                LocalidadLocal = jornada.LocalEquipo.Club.Localidad,
                                FechaNumero = numero,
                                InstanciaNombre = instanciaNombre,
                                Orden = 0
                            };
                        }

                        if (jornadasDto.Count > 0)
                        {
                            fechasHistoricasDto.Add(new FechaHistoricaAsignacionDTO
                            {
                                FechaId = fecha.Id,
                                Dia = dia,
                                DiaSemana = diaSemana,
                                Numero = numero,
                                InstanciaNombre = instanciaNombre,
                                Jornadas = jornadasDto
                            });
                        }
                    }

                    if (fechasHistoricasDto.Count > 0)
                    {
                        zonasDto.Add(new ZonaAsignacionHistoricaDTO
                        {
                            Id = zona.Id,
                            Nombre = zona.Nombre,
                            FechasHistoricas = fechasHistoricasDto
                        });
                    }
                }

                if (zonasDto.Count > 0)
                {
                    fasesDto.Add(new FaseAsignacionHistoricaDTO
                    {
                        Id = fase.Id,
                        Nombre = faseNombre,
                        Categorias = categoriasPorFaseId.TryGetValue(fase.Id, out var categorias)
                            ? categorias
                            : [],
                        Zonas = zonasDto
                    });
                }
            }

            if (fasesDto.Count > 0)
            {
                torneosDto.Add(new TorneoAsignacionHistoricaDTO
                {
                    Id = torneo.Id,
                    Nombre = torneo.Nombre,
                    HorarioDeJuego = torneo.HorarioDeJuego,
                    Fases = fasesDto
                });
            }
        }

        var arbitrosConJornadasDto = arbitrosElegibles
            .Select(a =>
            {
                var jornadasHistoricas = asignaciones
                    .Where(x => x.ArbitroId == a.Id && resumenesPorJornadaId.ContainsKey(x.JornadaId))
                    .Select(x =>
                    {
                        var baseResumen = resumenesPorJornadaId[x.JornadaId];
                        return new JornadaAsignadaResumenDTO
                        {
                            JornadaId = baseResumen.JornadaId,
                            Dia = baseResumen.Dia,
                            DiaSemana = baseResumen.DiaSemana,
                            TorneoNombre = baseResumen.TorneoNombre,
                            FaseNombre = baseResumen.FaseNombre,
                            ZonaNombre = baseResumen.ZonaNombre,
                            Local = baseResumen.Local,
                            Visitante = baseResumen.Visitante,
                            LocalidadLocal = baseResumen.LocalidadLocal,
                            FechaNumero = baseResumen.FechaNumero,
                            InstanciaNombre = baseResumen.InstanciaNombre,
                            Orden = x.Orden,
                            Whatsapp = MapWhatsappAsignacion(x)
                        };
                    })
                    .OrderByDescending(j => j.Dia)
                    .ThenBy(j => j.TorneoNombre)
                    .ToList();

                return new ArbitroConJornadasHistoricasDTO
                {
                    ArbitroId = a.Id,
                    Nombre = a.Nombre,
                    Apellido = a.Apellido,
                    JornadasHistoricas = jornadasHistoricas
                };
            })
            .Where(a => a.JornadasHistoricas.Count > 0)
            .ToList();

        var arbitrosElegiblesDto = arbitrosElegibles
            .Select(a => new ArbitroElegibleAsignacionDTO
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Apellido = a.Apellido,
                TelefonoCelular = a.TelefonoCelular,
                JornadasAsignadasEnProximasFechas = []
            })
            .ToList();

        return new AsignacionHistoricaArbitrosPorAgrupadorDTO
        {
            ArbitrosElegibles = arbitrosElegiblesDto,
            Torneos = torneosDto,
            ArbitrosConJornadas = arbitrosConJornadasDto
        };
    }

    public async Task AsignarArbitrosAJornada(int jornadaId, AsignarArbitrosJornadaDTO dto)
    {
        var arbitroIds = (dto.ArbitroIds ?? []).Distinct().ToList();
        if (arbitroIds.Count > 2)
            throw new ExcepcionControlada("Una jornada puede tener como máximo 2 árbitros asignados.");

        var esJornadaNormal = await _context.Jornadas
            .OfType<JornadaNormal>()
            .AnyAsync(j => j.Id == jornadaId);

        if (!esJornadaNormal)
            throw new ExcepcionControlada("La jornada no existe o no es una jornada normal.");

        var torneoAgrupadorId = await (
            from j in _context.Jornadas
            where j.Id == jornadaId
            join f in _context.Fechas on j.FechaId equals f.Id
            join z in _context.Zonas on f.ZonaId equals z.Id
            join fa in _context.Fases on z.FaseId equals fa.Id
            join t in _context.Torneos on fa.TorneoId equals t.Id
            select (int?)t.TorneoAgrupadorId
        ).FirstOrDefaultAsync();

        if (torneoAgrupadorId == null)
            throw new ExcepcionControlada("No se pudo resolver el torneo de la jornada.");

        if (arbitroIds.Count > 0)
        {
            var arbitrosValidos = await _context.ArbitroTorneoAgrupador
                .Where(a => arbitroIds.Contains(a.ArbitroId) && a.TorneoAgrupadorId == torneoAgrupadorId)
                .Select(a => a.ArbitroId)
                .ToListAsync();

            var invalidos = arbitroIds.Except(arbitrosValidos).ToList();
            if (invalidos.Count > 0)
                throw new ExcepcionControlada("Uno o más árbitros no están habilitados para el agrupador de este torneo.");
        }

        var asignaciones = arbitroIds
            .Select((id, index) => (ArbitroId: id, Orden: index + 1))
            .ToList();

        await _arbitroJornadaRepo.ReemplazarAsignaciones(jornadaId, asignaciones);
        await _bdVirtual.GuardarCambios();
    }

    public async Task MarcarWhatsappEnviado(
        int jornadaId,
        int arbitroId,
        MarcarWhatsappEnviadoArbitroJornadaDTO dto)
    {
        var marcado = await _arbitroJornadaRepo.MarcarWhatsappEnviado(
            jornadaId,
            arbitroId,
            dto,
            _relojArgentina.AhoraLocal);
        if (!marcado)
            throw new ExcepcionControlada("No existe una asignación de ese árbitro a la jornada indicada.");

        await _bdVirtual.GuardarCambios();
    }

    private static ArbitroAsignadoDTO MapArbitroAsignado(ArbitroJornada asignacion) =>
        new()
        {
            Id = asignacion.ArbitroId,
            Nombre = asignacion.Arbitro.Nombre,
            Apellido = asignacion.Arbitro.Apellido,
            TelefonoCelular = asignacion.Arbitro.TelefonoCelular,
            Orden = asignacion.Orden,
            WhatsappEnviado = asignacion.WhatsappEnviado,
            Whatsapp = MapWhatsappAsignacion(asignacion)
        };

    private static WhatsappAsignacionDTO? MapWhatsappAsignacion(ArbitroJornada asignacion)
    {
        if (!asignacion.WhatsappEnviado
            && string.IsNullOrWhiteSpace(asignacion.WhatsappHorarioInicio)
            && string.IsNullOrWhiteSpace(asignacion.WhatsappObservaciones)
            && string.IsNullOrWhiteSpace(asignacion.WhatsappCategoriasJson))
            return null;

        return new WhatsappAsignacionDTO
        {
            Enviado = asignacion.WhatsappEnviado,
            HorarioInicio = asignacion.WhatsappHorarioInicio,
            Observaciones = asignacion.WhatsappObservaciones,
            CategoriasNombres = DeserializarCategoriasNombres(asignacion.WhatsappCategoriasJson),
            EnviadoEn = asignacion.WhatsappEnviadoEn
        };
    }

    private static List<string> DeserializarCategoriasNombres(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            var categorias = JsonSerializer.Deserialize<List<WhatsappCategoriaSnapshotDTO>>(json);
            return categorias?
                .Select(c => c.Nombre)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList() ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static List<Fecha> OrdenarFechasHistoricas(List<Fecha> fechas)
    {
        return fechas
            .OrderByDescending(f => f.Dia)
            .ThenByDescending(f => f switch
            {
                FechaTodosContraTodos tct => tct.Numero,
                FechaEliminacionDirecta fed => fed.InstanciaId,
                _ => 0
            })
            .ThenByDescending(f => f.Id)
            .ToList();
    }

    private static Fecha? SeleccionarProximaFecha(
        List<Fecha> fechas,
        IReadOnlyDictionary<int, string> instanciasPorId)
    {
        return fechas
            .OrderBy(f => f.Dia)
            .ThenBy(f => f switch
            {
                FechaTodosContraTodos tct => tct.Numero,
                FechaEliminacionDirecta fed => -fed.InstanciaId,
                _ => 0
            })
            .ThenBy(f => f.Id)
            .FirstOrDefault();
    }

    private static string FormatearDiaSemana(DateOnly dia)
    {
        var nombre = CulturaEsAr.DateTimeFormat.GetDayName(dia.DayOfWeek);
        if (string.IsNullOrEmpty(nombre))
            return nombre;
        return char.ToUpper(nombre[0], CulturaEsAr) + nombre[1..];
    }
}
