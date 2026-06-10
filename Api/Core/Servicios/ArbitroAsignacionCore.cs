using System.Globalization;
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
    private readonly IBDVirtual _bdVirtual;
    private readonly IRelojZonaHorariaArgentina _relojArgentina;

    public ArbitroAsignacionCore(
        AppDbContext context,
        IArbitroJornadaRepo arbitroJornadaRepo,
        IBDVirtual bdVirtual,
        IRelojZonaHorariaArgentina relojArgentina)
    {
        _context = context;
        _arbitroJornadaRepo = arbitroJornadaRepo;
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

        var arbitrosElegibles = await _context.Arbitros
            .AsNoTracking()
            .Include(a => a.ArbitroTorneoAgrupadores)
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
                                    ? asigs.Select(a => new ArbitroAsignadoDTO
                                    {
                                        Id = a.ArbitroId,
                                        Nombre = a.Arbitro.Nombre,
                                        Apellido = a.Arbitro.Apellido,
                                        TelefonoCelular = a.Arbitro.TelefonoCelular,
                                        Orden = a.Orden,
                                        WhatsappEnviado = a.WhatsappEnviado
                                    }).ToList()
                                    : [];

                                var jornadaDto = new JornadaAsignacionDTO
                                {
                                    Id = jornada.Id,
                                    Dia = dia,
                                    DiaSemana = diaSemana,
                                    TorneoNombre = torneo.Nombre,
                                    FaseNombre = faseNombre,
                                    ZonaNombre = zona.Nombre,
                                    Local = jornada.LocalEquipo.Nombre,
                                    Visitante = jornada.VisitanteEquipo.Nombre,
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
                        Orden = x.Orden
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
                JornadasAsignadasEnProximasFechas = jornadasDelArbitro
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

    public async Task MarcarWhatsappEnviado(int jornadaId, int arbitroId)
    {
        var marcado = await _arbitroJornadaRepo.MarcarWhatsappEnviado(jornadaId, arbitroId);
        if (!marcado)
            throw new ExcepcionControlada("No existe una asignación de ese árbitro a la jornada indicada.");

        await _bdVirtual.GuardarCambios();
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
