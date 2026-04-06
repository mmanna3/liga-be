using Api.Core.DTOs.AppCarnetDigital;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Logica;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class AppCarnetDigitalCore : IAppCarnetDigitalCore
{
    private readonly IDelegadoRepo _delegadoRepo;
    private readonly IMapper _mapper;
    private readonly IEquipoRepo _equipoRepo;
    private readonly IImagenJugadorRepo _imagenJugadorRepo;
    private readonly IImagenDelegadoRepo _imagenDelegadoRepo;
    private readonly ITorneoAgrupadorRepo _torneoAgrupadorRepo;
    private readonly IFechaRepo _fechaRepo;
    private readonly AppPaths _paths;

    public AppCarnetDigitalCore(IDelegadoRepo delegadoRepo, IEquipoRepo equipoRepo, IMapper mapper,
        IImagenJugadorRepo imagenJugadorRepo, IImagenDelegadoRepo imagenDelegadoRepo,
        ITorneoAgrupadorRepo torneoAgrupadorRepo, IFechaRepo fechaRepo, AppPaths paths)
    {
        _delegadoRepo = delegadoRepo;
        _mapper = mapper;
        _imagenJugadorRepo = imagenJugadorRepo;
        _imagenDelegadoRepo = imagenDelegadoRepo;
        _equipoRepo = equipoRepo;
        _torneoAgrupadorRepo = torneoAgrupadorRepo;
        _fechaRepo = fechaRepo;
        _paths = paths;
    }

    public async Task<EquiposDelDelegadoDTO> ObtenerEquiposPorUsuarioDeDelegado(string usuario)
    {
        var delegado = await _delegadoRepo.ObtenerPorUsuario(usuario);
        var clubsConEquipos = new List<ClubConEquiposDTO>();

        foreach (var delegadoClub in delegado.DelegadoClubs ?? [])
        {
            var club = delegadoClub.Club;
            if (club == null)
                continue;

            var equipos = (club.Equipos ?? [])
                .Select(equipo => _mapper.Map<EquipoBaseDTO>(equipo))
                .ToList();

            clubsConEquipos.Add(new ClubConEquiposDTO
            {
                Nombre = club.Nombre,
                Equipos = equipos
            });
        }

        return new EquiposDelDelegadoDTO
        {
            ClubsConEquipos = clubsConEquipos
        };
    }

    public async Task<ICollection<CarnetDigitalDTO>?> Carnets(int equipoId)
    {
        var equipo = await _equipoRepo.ObtenerPorId(equipoId);
        if (equipo == null)
            return null;

        var lista = new List<CarnetDigitalDTO>();
        var club = equipo.Club ?? throw new InvalidOperationException("El equipo debe tener un club asociado.");

        // 1. Delegados primero (arriba de todo). Si son también jugadores, aparecerán otra vez más abajo.
        var delegados = await _delegadoRepo.ListarActivosDelClub(club.Id);
        foreach (var delegado in delegados)
        {
            var carnet = _mapper.Map<CarnetDigitalDTO>(delegado);
            carnet.FotoCarnet = ImagenUtility.AgregarMimeType(_imagenDelegadoRepo.GetFotoCarnetEnBase64(delegado.DNI));
            carnet.Equipo = club.Nombre;
            carnet.Torneo = "";
            carnet.Estado = (int)EstadoDelegadoEnum.Activo;
            carnet.EsDelegado = true;
            lista.Add(carnet);
        }

        // 2. Jugadores (en su categoría correspondiente). Los que también son delegados ya aparecieron arriba.
        foreach (var jugador in equipo.Jugadores.Where(x => x.EstadoJugadorId != (int)EstadoJugadorEnum.FichajeRechazado && x.EstadoJugadorId != (int)EstadoJugadorEnum.FichajePendienteDeAprobacion && x.EstadoJugadorId != (int)EstadoJugadorEnum.AprobadoPendienteDePago))
        {
            var carnet = _mapper.Map<CarnetDigitalDTO>(jugador);
            carnet.FotoCarnet = ImagenUtility.AgregarMimeType(_imagenJugadorRepo.GetFotoCarnetEnBase64(carnet.DNI));
            carnet.Equipo = equipo.Nombre;  // Porque le cuesta por referencia circular para hacerlo con Automapper
            carnet.EsDelegado = false;
            lista.Add(carnet);
        }

        return lista;
    }

    public async Task<ICollection<CarnetDigitalPendienteDTO>?> JugadoresPendientes(int equipoId)
    {
        var equipo = await _equipoRepo.ObtenerPorId(equipoId);
        if (equipo == null)
            return null;

        var lista = new List<CarnetDigitalPendienteDTO>();
        foreach (var jugador in equipo.Jugadores.Where(x => x.EstadoJugadorId is (int)EstadoJugadorEnum.FichajeRechazado or (int)EstadoJugadorEnum.FichajePendienteDeAprobacion or (int)EstadoJugadorEnum.AprobadoPendienteDePago))
        {
            var carnet = _mapper.Map<CarnetDigitalPendienteDTO>(jugador);
            carnet.FotoCarnet = ImagenUtility.AgregarMimeType(_imagenJugadorRepo.GetFotoCarnetEnBase64(carnet.DNI));
            carnet.Equipo = equipo.Nombre;  // Porque le cuesta por referencia circular para hacerlo con Automapper
            lista.Add(carnet);
        }

        return lista;
    }

    public Task<ICollection<CarnetDigitalDTO>?> CarnetsPorCodigoAlfanumerico(string codigoAlfanumerico)
    {
        var id = GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos(codigoAlfanumerico);
        return Carnets(id);
    }

    public Task<IReadOnlyList<InformacionInicialAgrupadorDTO>> InformacionInicialDeTorneosAsync(
        CancellationToken cancellationToken = default) =>
        _torneoAgrupadorRepo.ListarInformacionInicialParaAppAsync(cancellationToken);

    public async Task<IReadOnlyList<ClubesDTO>> ClubesPorZonaAsync(int zonaId,
        CancellationToken cancellationToken = default)
    {
        var equipos = await _equipoRepo.ListarPorZonaIdAsync(zonaId, cancellationToken);
        var baseEscudo = _paths.ImagenesEscudosRelative.TrimEnd('/');
        return equipos
            .OrderBy(e => e.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .Select(e =>
        {
            var club = e.Club;
            return new ClubesDTO
            {
                Equipo = e.Nombre,
                Escudo = $"{baseEscudo}/{club.Id}.jpg",
                Localidad = club.Localidad ?? string.Empty,
                Direccion = club.Direccion ?? string.Empty,
                EsTechado = club.EsTechado switch
                {
                    true => "Sí",
                    false => "No",
                    null => string.Empty
                }
            };
        }).ToList();
    }

    public async Task<FixtureDTO> FixtureTodosContraTodosAsync(int zonaId,
        CancellationToken cancellationToken = default)
    {
        var fechas = await _fechaRepo.ListarTodosContraTodosPorZonaParaAppAsync(zonaId, cancellationToken);
        var dto = new FixtureDTO();
        foreach (var fecha in fechas)
        {
            dto.Fechas.Add(new FixtureFechaDTO
            {
                Titulo = $"Fecha {fecha.Numero}",
                Dia = fecha.Dia?.ToString("dd-MM") ?? string.Empty,
                Partidos = fecha.Jornadas
                    .OrderBy(j => j.Id)
                    .Select(MapJornadaAFixturePartido)
                    .ToList()
            });
        }

        return dto;
    }

    private string EscudoRelativo(int clubId) =>
        $"{_paths.ImagenesEscudosRelative.TrimEnd('/')}/{clubId}.jpg";

    private FixturePartidoDTO MapJornadaAFixturePartido(Jornada j) => j switch
    {
        JornadaNormal n => new FixturePartidoDTO
        {
            Local = n.LocalEquipo.Nombre,
            Visitante = n.VisitanteEquipo.Nombre,
            LocalEscudo = EscudoRelativo(n.LocalEquipo.ClubId),
            VisitanteEscudo = EscudoRelativo(n.VisitanteEquipo.ClubId)
        },
        JornadaLibre l => new FixturePartidoDTO
        {
            Local = l.EquipoLocal.Nombre,
            Visitante = "LIBRE",
            LocalEscudo = string.Empty,
            VisitanteEscudo = string.Empty
        },
        JornadaInterzonal i when i.LocalOVisitanteId == (int)LocalVisitanteEnum.Local => new FixturePartidoDTO
        {
            Local = i.Equipo.Nombre,
            Visitante = "INTERZONAL",
            LocalEscudo = EscudoRelativo(i.Equipo.ClubId),
            VisitanteEscudo = string.Empty
        },
        JornadaInterzonal i => new FixturePartidoDTO
        {
            Local = "INTERZONAL",
            Visitante = i.Equipo.Nombre,
            LocalEscudo = string.Empty,
            VisitanteEscudo = EscudoRelativo(i.Equipo.ClubId)
        },
        JornadaSinEquipos => new FixturePartidoDTO(),
        _ => throw new InvalidOperationException($"Tipo de jornada no soportado: {j.GetType().Name}")
    };

    public async Task<JornadasDTO> JornadasTodosContraTodosAsync(int zonaId,
        CancellationToken cancellationToken = default)
    {
        var fechas = await _fechaRepo.ListarTodosContraTodosPorZonaParaAppConPartidosAsync(zonaId, cancellationToken);
        var dto = new JornadasDTO();
        if (fechas.Count == 0)
            return dto;

        var categoriasOrdenadas = fechas[0].Zona.Fase.Torneo.Categorias.OrderBy(c => c.Id).ToList();

        foreach (var fecha in fechas)
        {
            dto.Fechas.Add(new FechasParaJornadasDTO
            {
                Titulo = $"Fecha {fecha.Numero}",
                Dia = fecha.Dia?.ToString("dd-MM") ?? string.Empty,
                Jornadas = fecha.Jornadas.OrderBy(x => x.Id)
                    .Select(j => MapJornadaAJornadasPorFecha(j, categoriasOrdenadas))
                    .ToList()
            });
        }

        return dto;
    }

    public async Task<PosicionesDTO> PosicionesTodosContraTodosAsync(int zonaId,
        CancellationToken cancellationToken = default)
    {
        var categorias =
            await _fechaRepo.ListarCategoriasTorneoPorZonaTodosContraTodosAsync(zonaId, cancellationToken);
        var fechas = await _fechaRepo.ListarTodosContraTodosPorZonaParaAppConPartidosAsync(zonaId, cancellationToken);
        var equipos = (await _equipoRepo.ListarPorZonaIdAsync(zonaId, cancellationToken))
            .OrderBy(e => e.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var dto = new PosicionesDTO();
        var baseEscudo = _paths.ImagenesEscudosRelative.TrimEnd('/');

        foreach (var cat in categorias)
        {
            var bloque = new CategoriasConPosicionesDTO
            {
                Categoria = cat.Nombre,
                Renglones = []
            };

            var filas = new List<(Equipo Equipo, EstadisticasPosicionEquipo Stats, int Puntos)>();
            foreach (var equipo in equipos)
            {
                var ac = new EstadisticasPosicionEquipo();
                var puntos = 0;
                foreach (var fecha in fechas)
                {
                    foreach (var j in fecha.Jornadas)
                    {
                        var partido = j.Partidos?.FirstOrDefault(p => p.CategoriaId == cat.Id);
                        if (partido == null)
                            continue;
                        if (!PosicionesTodosContraTodosLogica.PartidoTieneResultadosCargados(partido))
                            continue;
                        if (!PosicionesTodosContraTodosLogica.IntentarObtenerMiResultadoYRival(partido, j, equipo.Id,
                                out var mi, out var rival))
                            continue;
                        PosicionesTodosContraTodosLogica.AcumularPartido(ref ac, mi, rival);
                        PosicionesTodosContraTodosLogica.AcumularPuntos(ref puntos, mi, rival);
                    }
                }

                filas.Add((equipo, ac, puntos));
            }

            var ordenadas = filas
                .OrderByDescending(f => f.Puntos)
                .ThenBy(f => f.Equipo.Nombre, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            var numeroPosicion = 1;
            foreach (var f in ordenadas)
            {
                var diff = f.Stats.GolesAFavor - f.Stats.GolesEnContra;
                bloque.Renglones.Add(new PosicionDelEquipoDTO
                {
                    Posicion = numeroPosicion++.ToString(),
                    Puntos = f.Puntos.ToString(),
                    Equipo = f.Equipo.Nombre,
                    Escudo = $"{baseEscudo}/{f.Equipo.ClubId}.jpg",
                    PartidosJugados = f.Stats.PartidosJugados.ToString(),
                    PartidosGanados = f.Stats.PartidosGanados.ToString(),
                    PartidosEmpatados = f.Stats.PartidosEmpatados.ToString(),
                    PartidosPerdidos = f.Stats.PartidosPerdidos.ToString(),
                    GolesAFavor = f.Stats.GolesAFavor.ToString(),
                    GolesEnContra = f.Stats.GolesEnContra.ToString(),
                    GolesDiferencia = diff.ToString(),
                    PartidosNoPresento = f.Stats.PartidosNoPresento.ToString(),
                    PartidosGanoPuntos = f.Stats.PartidosGanoPuntos.ToString(),
                    PartidosPerdioPuntos = f.Stats.PartidosPerdioPuntos.ToString()
                });
            }

            dto.Posiciones.Add(bloque);
        }

        return dto;
    }

    private static string FormatearResultadoPartido(Partido? p)
    {
        if (p == null)
            return string.Empty;

        var rl = p.ResultadoLocal.Trim();
        var rv = p.ResultadoVisitante.Trim();
        if (string.IsNullOrEmpty(rl) && string.IsNullOrEmpty(rv))
            return string.Empty;

        var s = $"{rl} - {rv}";
        var pl = p.PenalesLocal?.Trim();
        var pv = p.PenalesVisitante?.Trim();
        if (!string.IsNullOrEmpty(pl) || !string.IsNullOrEmpty(pv))
            s += $" ({pl ?? ""} - {pv ?? ""})";

        return s;
    }

    private JornadasPorFechaDTO MapJornadaAJornadasPorFecha(Jornada j,
        IReadOnlyList<TorneoCategoria> categoriasOrdenadas)
    {
        var partidosPorCat = (j.Partidos ?? [])
            .GroupBy(p => p.CategoriaId)
            .ToDictionary(g => g.Key, g => g.First());

        var categorias = categoriasOrdenadas.Select(cat =>
        {
            partidosPorCat.TryGetValue(cat.Id, out var p);
            return new ResultadoCategoriaDTO
            {
                Categoria = cat.Nombre,
                Resultado = FormatearResultadoPartido(p)
            };
        }).ToList();

        return j switch
        {
            JornadaNormal n => new JornadasPorFechaDTO
            {
                Local = new JornadaPorEquipoDTO
                {
                    Escudo = EscudoRelativo(n.LocalEquipo.ClubId),
                    Equipo = n.LocalEquipo.Nombre,
                    Categorias = categorias
                },
                Visitante = new JornadaPorEquipoDTO
                {
                    Escudo = EscudoRelativo(n.VisitanteEquipo.ClubId),
                    Equipo = n.VisitanteEquipo.Nombre,
                    Categorias = categorias
                }
            },
            JornadaLibre l => new JornadasPorFechaDTO
            {
                Local = new JornadaPorEquipoDTO
                {
                    Escudo = EscudoRelativo(l.EquipoLocal.ClubId),
                    Equipo = l.EquipoLocal.Nombre,
                    Categorias = categorias
                },
                Visitante = new JornadaPorEquipoDTO
                {
                    Escudo = string.Empty,
                    Equipo = "LIBRE",
                    Categorias = categorias
                }
            },
            JornadaInterzonal i when i.LocalOVisitanteId == (int)LocalVisitanteEnum.Local => new JornadasPorFechaDTO
            {
                Local = new JornadaPorEquipoDTO
                {
                    Escudo = EscudoRelativo(i.Equipo.ClubId),
                    Equipo = i.Equipo.Nombre,
                    Categorias = categorias
                },
                Visitante = new JornadaPorEquipoDTO
                {
                    Escudo = string.Empty,
                    Equipo = "INTERZONAL",
                    Categorias = categorias
                }
            },
            JornadaInterzonal i => new JornadasPorFechaDTO
            {
                Local = new JornadaPorEquipoDTO
                {
                    Escudo = string.Empty,
                    Equipo = "INTERZONAL",
                    Categorias = categorias
                },
                Visitante = new JornadaPorEquipoDTO
                {
                    Escudo = EscudoRelativo(i.Equipo.ClubId),
                    Equipo = i.Equipo.Nombre,
                    Categorias = categorias
                }
            },
            JornadaSinEquipos => new JornadasPorFechaDTO
            {
                Local = new JornadaPorEquipoDTO { Categorias = categorias },
                Visitante = new JornadaPorEquipoDTO { Categorias = categorias }
            },
            _ => throw new InvalidOperationException($"Tipo de jornada no soportado: {j.GetType().Name}")
        };
    }
}