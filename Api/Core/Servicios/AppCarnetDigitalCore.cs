using Api.Core.DTOs.AppCarnetDigital;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Logica;
using Api.Core.Otros;
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
    private readonly ITorneoRepo _torneoRepo;
    private readonly ILeyendaTablaPosicionesRepo _leyendaTablaPosicionesRepo;
    private readonly AppPaths _paths;
    private readonly IImagenEscudoRepo _imagenEscudoRepo;

    public AppCarnetDigitalCore(IDelegadoRepo delegadoRepo, IEquipoRepo equipoRepo, IMapper mapper,
        IImagenJugadorRepo imagenJugadorRepo, IImagenDelegadoRepo imagenDelegadoRepo,
        ITorneoAgrupadorRepo torneoAgrupadorRepo, IFechaRepo fechaRepo, ITorneoRepo torneoRepo,
        ILeyendaTablaPosicionesRepo leyendaTablaPosicionesRepo, AppPaths paths,
        IImagenEscudoRepo imagenEscudoRepo)
    {
        _delegadoRepo = delegadoRepo;
        _mapper = mapper;
        _imagenJugadorRepo = imagenJugadorRepo;
        _imagenDelegadoRepo = imagenDelegadoRepo;
        _equipoRepo = equipoRepo;
        _torneoAgrupadorRepo = torneoAgrupadorRepo;
        _fechaRepo = fechaRepo;
        _torneoRepo = torneoRepo;
        _leyendaTablaPosicionesRepo = leyendaTablaPosicionesRepo;
        _paths = paths;
        _imagenEscudoRepo = imagenEscudoRepo;
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

    public async Task<PlanillaDeJuegoDTO> PlanillasDeJuegoAsync(string codigoAlfanumerico,
        CancellationToken cancellationToken = default)
    {
        var equipoId = GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos(codigoAlfanumerico);

        var anio = DateTime.Today.Year;
        var torneoIds = await _equipoRepo.ListarTorneoIdsDelEquipoEnAnioAsync(equipoId, anio, cancellationToken);

        if (torneoIds.Count == 0)
            throw new ExcepcionControlada("El equipo no está inscripto en ningún torneo este año.");

        var torneoId = torneoIds.OrderBy(id => id).First();
        var torneo = await _torneoRepo.ObtenerPorIdConCategoriasAsync(torneoId, cancellationToken);
        if (torneo == null)
            throw new ExcepcionControlada("No existe el torneo.");

        var equipo = await _equipoRepo.ObtenerPorId(equipoId);
        if (equipo == null)
            throw new ExcepcionControlada("No existe el equipo.");

        var categoriasOrdenadas = torneo.Categorias.OrderBy(c => c.Id).ToList();
        var estadosPermitidos = new HashSet<int>
        {
            (int)EstadoJugadorEnum.Activo,
            (int)EstadoJugadorEnum.Suspendido,
            (int)EstadoJugadorEnum.Inhabilitado
        };

        var jugadoresPorCategoria = categoriasOrdenadas.ToDictionary(c => c.Id, _ => new List<JugadorDatosPlanillaDTO>());

        foreach (var je in equipo.Jugadores)
        {
            if (!estadosPermitidos.Contains(je.EstadoJugadorId))
                continue;

            var jugador = je.Jugador;
            if (jugador == null)
                continue;

            var anioNac = jugador.FechaNacimiento.Year;
            var nombreCompleto = $"{jugador.Nombre} {jugador.Apellido}".Trim();
            var estadoTexto = ((EstadoJugadorEnum)je.EstadoJugadorId).ToString();

            foreach (var cat in categoriasOrdenadas.Where(c => anioNac >= c.AnioDesde && anioNac <= c.AnioHasta))
            {
                jugadoresPorCategoria[cat.Id].Add(new JugadorDatosPlanillaDTO
                {
                    DNI = jugador.DNI,
                    Nombre = nombreCompleto,
                    Estado = estadoTexto
                });
            }
        }

        var planillas = new List<JugadoresPorCategoriaDTO>();
        foreach (var c in categoriasOrdenadas)
        {
            var lista = jugadoresPorCategoria[c.Id];
            lista.Sort((a, b) => string.Compare(a.Nombre, b.Nombre, StringComparison.CurrentCultureIgnoreCase));
            planillas.Add(new JugadoresPorCategoriaDTO
            {
                Categoria = c.Nombre,
                Jugadores = lista
            });
        }

        return new PlanillaDeJuegoDTO
        {
            Torneo = torneo.Nombre,
            Equipo = equipo.Nombre,
            Planillas = planillas
        };
    }

    public Task<IReadOnlyList<InformacionInicialAgrupadorDTO>> InformacionInicialDeTorneosAsync(
        CancellationToken cancellationToken = default) =>
        _torneoAgrupadorRepo.ListarInformacionInicialParaAppAsync(cancellationToken);

    public async Task<IReadOnlyList<ClubesDTO>> ClubesPorZonaAsync(int zonaId,
        CancellationToken cancellationToken = default)
    {
        var equipos = await _equipoRepo.ListarPorZonaIdAsync(zonaId, cancellationToken);
        return equipos
            .OrderBy(e => e.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .Select(e =>
        {
            var club = e.Club;
            return new ClubesDTO
            {
                Equipo = e.Nombre,
                Escudo = _imagenEscudoRepo.GetRutaRelativaEscudo(club.Id),
                Localidad = club.Localidad ?? string.Empty,
                Direccion = club.Direccion ?? string.Empty,
                EsTechado = club.EsTechado switch
                {
                    true => "Sí",
                    false => "No",
                    null => string.Empty
                },
                TipoCancha = club.CanchaTipo?.Tipo ?? nameof(CanchaTipoEnum.Consultar)
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

    private string EscudoRelativo(int clubId) => _imagenEscudoRepo.GetRutaRelativaEscudo(clubId);

    private string EscudoPorDefectoRelativo() => _paths.EscudoDefaultRelative;

    private FixturePartidoDTO MapJornadaAFixturePartido(Jornada j) => j switch
    {
        JornadaNormal n => new FixturePartidoDTO
        {
            Local = n.LocalEquipo.Nombre,
            Visitante = n.VisitanteEquipo.Nombre,
            LocalEscudo = EscudoRelativo(n.LocalEquipo.ClubId),
            VisitanteEscudo = EscudoRelativo(n.VisitanteEquipo.ClubId)
        },
        JornadaLibre l when l.LocalOVisitanteId == (int)LocalVisitanteEnum.Local => new FixturePartidoDTO
        {
            Local = l.Equipo.Nombre,
            Visitante = "LIBRE",
            LocalEscudo = EscudoRelativo(l.Equipo.ClubId),
            VisitanteEscudo = EscudoPorDefectoRelativo()
        },
        JornadaLibre l => new FixturePartidoDTO
        {
            Local = "LIBRE",
            Visitante = l.Equipo.Nombre,
            LocalEscudo = EscudoPorDefectoRelativo(),
            VisitanteEscudo = EscudoRelativo(l.Equipo.ClubId)
        },
        JornadaInterzonal i when i.LocalOVisitanteId == (int)LocalVisitanteEnum.Local => new FixturePartidoDTO
        {
            Local = i.Equipo.Nombre,
            Visitante = InterzonalAppEtiqueta.Equipo(i.Numero),
            LocalEscudo = EscudoRelativo(i.Equipo.ClubId),
            VisitanteEscudo = EscudoPorDefectoRelativo()
        },
        JornadaInterzonal i => new FixturePartidoDTO
        {
            Local = InterzonalAppEtiqueta.Equipo(i.Numero),
            Visitante = i.Equipo.Nombre,
            LocalEscudo = EscudoPorDefectoRelativo(),
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
        var datos = await CalcularDatosPosicionesZonaAsync(zonaId, cancellationToken);
        string EscudoDeClub(int clubId) => _imagenEscudoRepo.GetRutaRelativaEscudo(clubId);
        var bloques = new List<CategoriasConPosicionesDTO>();

        foreach (var (cat, filas) in datos.FilasPorCategoria)
        {
            var leyendaCat =
                PosicionesLeyendasTablaHelper.ConcatenarTextos(datos.LeyendasZona.Where(l => l.CategoriaId == cat.Id));
            bloques.Add(ConstruirBloquePosiciones(cat.Nombre, leyendaCat, filas, EscudoDeClub));
        }

        var leyendaGeneral =
            PosicionesLeyendasTablaHelper.ConcatenarTextos(datos.LeyendasZona.Where(l => l.CategoriaId == null));
        bloques.Add(ConstruirBloqueGeneralAcumulado(leyendaGeneral, datos.FilasPorCategoria, datos.SoloGeneralPorEquipo,
            EscudoDeClub));

        return new PosicionesDTO { VerGoles = datos.VerGoles, Posiciones = bloques };
    }

    public async Task<PosicionesDTO> PosicionesAnualAsync(int zonaId,
        CancellationToken cancellationToken = default)
    {
        var par = await _fechaRepo.ObtenerIdsZonasAnualPorZonaReferenciaAsync(zonaId, cancellationToken);
        if (par == null)
            throw new ExcepcionControlada(
                "No se puede calcular la tabla anual: el torneo debe tener fases de apertura y clausura y la zona debe existir en ambas.");

        var datosA = await CalcularDatosPosicionesZonaAsync(par.Value.ZonaAperturaId, cancellationToken);
        var datosC = await CalcularDatosPosicionesZonaAsync(par.Value.ZonaClausuraId, cancellationToken);

        var filasFusion = FusionarFilasDosZonas(datosA.FilasPorCategoria, datosC.FilasPorCategoria);
        var soloGeneralFusion = FusionarDiccionariosSuma(datosA.SoloGeneralPorEquipo, datosC.SoloGeneralPorEquipo);

        string EscudoDeClub(int clubId) => _imagenEscudoRepo.GetRutaRelativaEscudo(clubId);
        var bloques = new List<CategoriasConPosicionesDTO>();

        foreach (var (cat, filas) in filasFusion)
        {
            var leyendaCat = PosicionesLeyendasTablaHelper.ConcatenarTextos(
                datosA.LeyendasZona.Where(l => l.CategoriaId == cat.Id)
                    .Concat(datosC.LeyendasZona.Where(l => l.CategoriaId == cat.Id)));
            bloques.Add(ConstruirBloquePosiciones(cat.Nombre, leyendaCat, filas, EscudoDeClub));
        }

        var leyendaGeneral = PosicionesLeyendasTablaHelper.ConcatenarTextos(
            datosA.LeyendasZona.Where(l => l.CategoriaId == null)
                .Concat(datosC.LeyendasZona.Where(l => l.CategoriaId == null)));
        bloques.Add(ConstruirBloqueGeneralAcumulado(leyendaGeneral, filasFusion, soloGeneralFusion, EscudoDeClub));

        return new PosicionesDTO { VerGoles = datosA.VerGoles, Posiciones = bloques };
    }

    private async Task<DatosPosicionesZonaCalculados> CalcularDatosPosicionesZonaAsync(int zonaId,
        CancellationToken cancellationToken)
    {
        var categorias =
            await _fechaRepo.ListarCategoriasTorneoPorZonaTodosContraTodosAsync(zonaId, cancellationToken);
        var fechas = await _fechaRepo.ListarTodosContraTodosPorZonaParaAppConPartidosAsync(zonaId, cancellationToken);
        var equipos = (await _equipoRepo.ListarPorZonaIdAsync(zonaId, cancellationToken))
            .OrderBy(e => e.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var verGoles = fechas.Count > 0
            ? fechas[0].Zona.Fase.Torneo.SeVenLosGolesEnTablaDePosiciones
            : categorias.FirstOrDefault()?.Torneo?.SeVenLosGolesEnTablaDePosiciones ?? true;

        var leyendasZona = (await _leyendaTablaPosicionesRepo.ListarPorPadre(zonaId)).OrderBy(l => l.Id).ToList();

        var quitaPorCatYEquipo = new Dictionary<(int CategoriaId, int EquipoId), int>();
        var soloGeneralPorEquipo = new Dictionary<int, int>();
        foreach (var l in leyendasZona)
        {
            if (l.QuitaDePuntos <= 0 || l.EquipoId is not { } eqId)
                continue;
            if (l.CategoriaId is { } cid)
            {
                var k = (cid, eqId);
                quitaPorCatYEquipo[k] = quitaPorCatYEquipo.GetValueOrDefault(k) + l.QuitaDePuntos;
            }
            else
                soloGeneralPorEquipo[eqId] = soloGeneralPorEquipo.GetValueOrDefault(eqId) + l.QuitaDePuntos;
        }

        var filasPorCategoria = new List<(TorneoCategoria Cat, List<(Equipo Equipo, EstadisticasPosicionEquipo Stats, int Puntos)>)>();

        foreach (var cat in categorias)
        {
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

                var quitaCat = quitaPorCatYEquipo.GetValueOrDefault((cat.Id, equipo.Id));
                filas.Add((equipo, ac, puntos - quitaCat));
            }

            filasPorCategoria.Add((cat, filas));
        }

        return new DatosPosicionesZonaCalculados(filasPorCategoria, leyendasZona, soloGeneralPorEquipo, verGoles);
    }

    private static List<(TorneoCategoria Cat, List<(Equipo Equipo, EstadisticasPosicionEquipo Stats, int Puntos)>)>
        FusionarFilasDosZonas(
            List<(TorneoCategoria Cat, List<(Equipo Equipo, EstadisticasPosicionEquipo Stats, int Puntos)>)> a,
            List<(TorneoCategoria Cat, List<(Equipo Equipo, EstadisticasPosicionEquipo Stats, int Puntos)>)> b)
    {
        if (a.Count != b.Count)
            throw new InvalidOperationException("Categorías distintas al fusionar posiciones anuales.");

        var result = new List<(TorneoCategoria Cat, List<(Equipo Equipo, EstadisticasPosicionEquipo Stats, int Puntos)>)>();
        for (var i = 0; i < a.Count; i++)
        {
            var (catA, filasA) = a[i];
            var (catB, filasB) = b[i];
            if (catA.Id != catB.Id)
                throw new InvalidOperationException("Categorías distintas al fusionar posiciones anuales.");

            var porEquipo = new Dictionary<int, (Equipo Equipo, EstadisticasPosicionEquipo Stats, int Puntos)>();
            foreach (var row in filasA)
                porEquipo[row.Equipo.Id] = row;
            foreach (var row in filasB)
            {
                if (porEquipo.TryGetValue(row.Equipo.Id, out var prev))
                {
                    porEquipo[row.Equipo.Id] = (
                        row.Equipo,
                        EstadisticasPosicionEquipo.Sumar(prev.Stats, row.Stats),
                        prev.Puntos + row.Puntos);
                }
                else
                    porEquipo[row.Equipo.Id] = row;
            }

            result.Add((catA, porEquipo.Values.ToList()));
        }

        return result;
    }

    private static Dictionary<int, int> FusionarDiccionariosSuma(Dictionary<int, int> a, Dictionary<int, int> b)
    {
        var r = new Dictionary<int, int>(a);
        foreach (var kv in b)
            r[kv.Key] = r.GetValueOrDefault(kv.Key) + kv.Value;
        return r;
    }

    private sealed record DatosPosicionesZonaCalculados(
        List<(TorneoCategoria Cat, List<(Equipo Equipo, EstadisticasPosicionEquipo Stats, int Puntos)>)> FilasPorCategoria,
        List<LeyendaTablaPosiciones> LeyendasZona,
        Dictionary<int, int> SoloGeneralPorEquipo,
        bool VerGoles);

    private static CategoriasConPosicionesDTO ConstruirBloqueGeneralAcumulado(
        string? leyenda,
        List<(TorneoCategoria Cat, List<(Equipo Equipo, EstadisticasPosicionEquipo Stats, int Puntos)>)> filasPorCategoria,
        Dictionary<int, int> soloGeneralPorEquipo,
        Func<int, string> escudoRelativoPorClubId)
    {
        var acumulado = new Dictionary<int, (Equipo Equipo, EstadisticasPosicionEquipo Stats, int Puntos)>();

        foreach (var (_, filas) in filasPorCategoria)
        {
            foreach (var (equipo, stats, puntos) in filas)
            {
                if (acumulado.TryGetValue(equipo.Id, out var prev))
                    acumulado[equipo.Id] = (
                        equipo,
                        EstadisticasPosicionEquipo.Sumar(prev.Stats, stats),
                        prev.Puntos + puntos);
                else
                    acumulado[equipo.Id] = (equipo, stats, puntos);
            }
        }

        var filasAgg = acumulado.Values
            .Select(x =>
            {
                var solo = soloGeneralPorEquipo.GetValueOrDefault(x.Equipo.Id);
                return (x.Equipo, x.Stats, x.Puntos - solo);
            })
            .ToList();

        return ConstruirBloquePosiciones("General", leyenda, filasAgg, escudoRelativoPorClubId);
    }

    private static CategoriasConPosicionesDTO ConstruirBloquePosiciones(
        string nombreCategoria,
        string? leyenda,
        List<(Equipo Equipo, EstadisticasPosicionEquipo Stats, int Puntos)> filas,
        Func<int, string> escudoRelativoPorClubId)
    {
        var bloque = new CategoriasConPosicionesDTO
        {
            Categoria = nombreCategoria,
            Leyenda = leyenda,
            Renglones = []
        };

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
                Escudo = escudoRelativoPorClubId(f.Equipo.ClubId),
                PartidosJugados = f.Stats.PartidosJugados.ToString(),
                PartidosGanados = f.Stats.PartidosGanados.ToString(),
                PartidosEmpatados = f.Stats.PartidosEmpatados.ToString(),
                PartidosPerdidos = f.Stats.PartidosPerdidos.ToString(),
                GolesAFavor = f.Stats.GolesAFavor.ToString(),
                GolesEnContra = f.Stats.GolesEnContra.ToString(),
                GolesDiferencia = diff.ToString(),
                PartidosNoPresento = f.Stats.PartidosNoPresento.ToString()
            });
        }

        return bloque;
    }

    public async Task<EliminacionDirectaDTO> EliminacionDirectaAsync(int zonaId,
        CancellationToken cancellationToken = default)
    {
        var fechas = await _fechaRepo.ListarEliminacionDirectaPorZonaParaAppAsync(zonaId, cancellationToken);
        var dto = new EliminacionDirectaDTO();
        if (fechas.Count == 0)
            return dto;

        foreach (var fecha in fechas)
        {
            var categoriaId = fecha.Zona.CategoriaId;
            dto.Instancias.Add(new InstanciasDTO
            {
                Titulo = fecha.Instancia.Nombre,
                Dia = fecha.Dia?.ToString("dd-MM") ?? string.Empty,
                Partidos = fecha.Jornadas
                    .OrderBy(j => j.Id)
                    .Select(j => MapJornadaAPartidoEliminacionDirecta(j, categoriaId))
                    .ToList()
            });
        }

        return dto;
    }

    private PartidoEliminacionDirectaDTO MapJornadaAPartidoEliminacionDirecta(Jornada j, int categoriaId)
    {
        var p = j.Partidos?.FirstOrDefault(x => x.CategoriaId == categoriaId);
        return j switch
        {
            JornadaNormal n => new PartidoEliminacionDirectaDTO
            {
                Local = n.LocalEquipo.Nombre,
                Visitante = n.VisitanteEquipo.Nombre,
                EscudoLocal = EscudoRelativo(n.LocalEquipo.ClubId),
                EscudoVisitante = EscudoRelativo(n.VisitanteEquipo.ClubId),
                ResultadoLocal = TrimResultado(p?.ResultadoLocal),
                ResultadoVisitante = TrimResultado(p?.ResultadoVisitante),
                PenalesLocal = TrimResultado(p?.PenalesLocal),
                PenalesVisitante = TrimResultado(p?.PenalesVisitante)
            },
            JornadaLibre l when l.LocalOVisitanteId == (int)LocalVisitanteEnum.Local => new PartidoEliminacionDirectaDTO
            {
                Local = l.Equipo.Nombre,
                Visitante = "LIBRE",
                EscudoLocal = EscudoRelativo(l.Equipo.ClubId),
                EscudoVisitante = EscudoPorDefectoRelativo(),
                ResultadoLocal = TrimResultado(p?.ResultadoLocal),
                ResultadoVisitante = TrimResultado(p?.ResultadoVisitante),
                PenalesLocal = TrimResultado(p?.PenalesLocal),
                PenalesVisitante = TrimResultado(p?.PenalesVisitante)
            },
            JornadaLibre l => new PartidoEliminacionDirectaDTO
            {
                Local = "LIBRE",
                Visitante = l.Equipo.Nombre,
                EscudoLocal = EscudoPorDefectoRelativo(),
                EscudoVisitante = EscudoRelativo(l.Equipo.ClubId),
                ResultadoLocal = TrimResultado(p?.ResultadoLocal),
                ResultadoVisitante = TrimResultado(p?.ResultadoVisitante),
                PenalesLocal = TrimResultado(p?.PenalesLocal),
                PenalesVisitante = TrimResultado(p?.PenalesVisitante)
            },
            JornadaInterzonal i when i.LocalOVisitanteId == (int)LocalVisitanteEnum.Local => new PartidoEliminacionDirectaDTO
            {
                Local = i.Equipo.Nombre,
                Visitante = InterzonalAppEtiqueta.Equipo(i.Numero),
                EscudoLocal = EscudoRelativo(i.Equipo.ClubId),
                EscudoVisitante = EscudoPorDefectoRelativo(),
                ResultadoLocal = TrimResultado(p?.ResultadoLocal),
                ResultadoVisitante = TrimResultado(p?.ResultadoVisitante),
                PenalesLocal = TrimResultado(p?.PenalesLocal),
                PenalesVisitante = TrimResultado(p?.PenalesVisitante)
            },
            JornadaInterzonal i => new PartidoEliminacionDirectaDTO
            {
                Local = InterzonalAppEtiqueta.Equipo(i.Numero),
                Visitante = i.Equipo.Nombre,
                EscudoLocal = EscudoPorDefectoRelativo(),
                EscudoVisitante = EscudoRelativo(i.Equipo.ClubId),
                ResultadoLocal = TrimResultado(p?.ResultadoLocal),
                ResultadoVisitante = TrimResultado(p?.ResultadoVisitante),
                PenalesLocal = TrimResultado(p?.PenalesLocal),
                PenalesVisitante = TrimResultado(p?.PenalesVisitante)
            },
            JornadaSinEquipos => new PartidoEliminacionDirectaDTO
            {
                ResultadoLocal = TrimResultado(p?.ResultadoLocal),
                ResultadoVisitante = TrimResultado(p?.ResultadoVisitante),
                PenalesLocal = TrimResultado(p?.PenalesLocal),
                PenalesVisitante = TrimResultado(p?.PenalesVisitante)
            },
            _ => throw new InvalidOperationException($"Tipo de jornada no soportado: {j.GetType().Name}")
        };
    }

    private static string TrimResultado(string? s) => string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim();

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

    private static (int puntosTotales, int partidosJugados) CalcularTotalesEquipoEnJornada(
        Jornada jornada,
        int equipoId)
    {
        var puntosTotales = 0;
        var partidosJugados = 0;

        foreach (var partido in jornada.Partidos ?? [])
        {
            if (!PosicionesTodosContraTodosLogica.PartidoTieneResultadosCargados(partido))
                continue;

            if (!PosicionesTodosContraTodosLogica.IntentarObtenerMiResultadoYRival(partido, jornada, equipoId, out var mi,
                    out var rival))
                continue;

            PosicionesTodosContraTodosLogica.AcumularPuntos(ref puntosTotales, mi, rival);

            var miNormalizado = mi.Trim();
            if (miNormalizado is not "S" and not "P" and not "NP")
                partidosJugados++;
        }

        return (puntosTotales, partidosJugados);
    }

    private JornadaPorEquipoDTO CrearJornadaPorEquipo(
        Jornada jornada,
        int equipoId,
        string escudo,
        string equipo,
        ICollection<ResultadoCategoriaDTO> categorias)
    {
        var (puntosTotales, partidosJugados) = CalcularTotalesEquipoEnJornada(jornada, equipoId);
        return new JornadaPorEquipoDTO
        {
            Escudo = escudo,
            Equipo = equipo,
            Categorias = categorias,
            PuntosTotales = puntosTotales,
            PartidosJugados = partidosJugados
        };
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
                Local = CrearJornadaPorEquipo(j, n.LocalEquipoId, EscudoRelativo(n.LocalEquipo.ClubId), n.LocalEquipo.Nombre,
                    categorias),
                Visitante = CrearJornadaPorEquipo(j, n.VisitanteEquipoId, EscudoRelativo(n.VisitanteEquipo.ClubId),
                    n.VisitanteEquipo.Nombre, categorias)
            },
            JornadaLibre l when l.LocalOVisitanteId == (int)LocalVisitanteEnum.Local => new JornadasPorFechaDTO
            {
                Local = CrearJornadaPorEquipo(j, l.EquipoId, EscudoRelativo(l.Equipo.ClubId), l.Equipo.Nombre,
                    categorias),
                Visitante = new JornadaPorEquipoDTO
                {
                    Escudo = EscudoPorDefectoRelativo(),
                    Equipo = "LIBRE",
                    Categorias = categorias
                }
            },
            JornadaLibre l => new JornadasPorFechaDTO
            {
                Local = new JornadaPorEquipoDTO
                {
                    Escudo = EscudoPorDefectoRelativo(),
                    Equipo = "LIBRE",
                    Categorias = categorias
                },
                Visitante = CrearJornadaPorEquipo(j, l.EquipoId, EscudoRelativo(l.Equipo.ClubId), l.Equipo.Nombre,
                    categorias)
            },
            JornadaInterzonal i when i.LocalOVisitanteId == (int)LocalVisitanteEnum.Local => new JornadasPorFechaDTO
            {
                Local = CrearJornadaPorEquipo(j, i.EquipoId, EscudoRelativo(i.Equipo.ClubId), i.Equipo.Nombre, categorias),
                Visitante = new JornadaPorEquipoDTO
                {
                    Escudo = EscudoPorDefectoRelativo(),
                    Equipo = InterzonalAppEtiqueta.Equipo(i.Numero),
                    Categorias = categorias
                }
            },
            JornadaInterzonal i => new JornadasPorFechaDTO
            {
                Local = new JornadaPorEquipoDTO
                {
                    Escudo = EscudoPorDefectoRelativo(),
                    Equipo = InterzonalAppEtiqueta.Equipo(i.Numero),
                    Categorias = categorias
                },
                Visitante = CrearJornadaPorEquipo(j, i.EquipoId, EscudoRelativo(i.Equipo.ClubId), i.Equipo.Nombre, categorias)
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