using System.Globalization;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Entidades.EntidadesConValoresPredefinidos;
using Api.Core.Enums;
using Api.Core.Logica;
using Api.Core.Otros;
using AutoMapper;
using System.Linq;
using Api.Core.DTOs.AppCarnetDigital;
using Api.Core.Servicios;
using ClubDTOAdmin = Api.Core.DTOs.ClubDTO;

namespace Api._Config;

public class MapperConfig : Profile
{
    public MapperConfig()
    {
        CreateMap<Club, ClubDTOAdmin>()
            .ForMember(dest => dest.Delegados, opt => opt.MapFrom(src => src.DelegadoClubs.Select(dc => dc.Delegado).ToList()))
            .ForMember(dest => dest.Direccion, opt => opt.MapFrom(src => src.Direccion))
            .ForMember(dest => dest.CanchaTipo,
                opt => opt.MapFrom(src => CanchaTipoEtiqueta.Formatear(src.CanchaTipo)))
            .ForMember(dest => dest.Localidad, opt => opt.MapFrom(src => src.Localidad))
            .PreserveReferences().ReverseMap()
            .ForMember(dest => dest.CanchaTipo, opt => opt.Ignore())
            .ForMember(dest => dest.DelegadoClubs, opt => opt.Ignore());
        CreateMap<Torneo, TorneoDTO>()
            .ForMember(dest => dest.TorneoAgrupadorNombre, opt => opt.MapFrom(src => src.TorneoAgrupador != null ? src.TorneoAgrupador.Nombre : string.Empty))
            .ForMember(dest => dest.FaseAperturaNombre, opt => opt.MapFrom(src => src.FaseApertura != null ? src.FaseApertura.Nombre : null))
            .ForMember(dest => dest.FaseClausuraNombre, opt => opt.MapFrom(src => src.FaseClausura != null ? src.FaseClausura.Nombre : null))
            .ForMember(dest => dest.SePuedeEditar, opt => opt.MapFrom<TorneoSePuedeEditarResolver>())
            .ForMember(dest => dest.Fases, opt => opt.MapFrom(src => src.Fases != null ? src.Fases : new List<Fase>()))
            .ForMember(dest => dest.GruposDeFases, opt => opt.MapFrom(src => src.GruposDeFases != null ? src.GruposDeFases : new List<GrupoDeFases>()))
            .ForMember(dest => dest.Categorias, opt => opt.MapFrom(src => src.Categorias != null
                ? src.Categorias.OrderBy(c => c.Orden).ThenBy(c => c.Id).ToList()
                : new List<TorneoCategoria>()))
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.TorneoAgrupador, opt => opt.Ignore())
            .ForMember(dest => dest.Fases, opt => opt.Ignore())
            .ForMember(dest => dest.GruposDeFases, opt => opt.Ignore())
            .ForMember(dest => dest.Categorias, opt => opt.Ignore())
            .ForMember(dest => dest.FaseApertura, opt => opt.Ignore())
            .ForMember(dest => dest.FaseClausura, opt => opt.Ignore())
            .ForMember(dest => dest.FaseAperturaId, opt => opt.Ignore())
            .ForMember(dest => dest.FaseClausuraId, opt => opt.Ignore())
            .ForSourceMember(src => src.SePuedeEditar, opt => opt.DoNotValidate());

        CreateMap<TorneoCategoria, TorneoCategoriaDTO>()
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.Torneo, opt => opt.Ignore());

        CreateMap<FaseCategoria, FaseCategoriaDTO>()
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.Fase, opt => opt.Ignore());

        CreateMap<GrupoDeFases, GrupoDeFasesDTO>()
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.Torneo, opt => opt.Ignore())
            .ForMember(dest => dest.GrupoDeFasesPadre, opt => opt.Ignore())
            .ForMember(dest => dest.SubGrupos, opt => opt.Ignore())
            .ForMember(dest => dest.Fases, opt => opt.Ignore());

        CreateMap<LeyendaTablaPosiciones, LeyendaTablaPosicionesDTO>()
            .ForMember(dest => dest.Equipo, opt => opt.MapFrom(src => src.Equipo != null ? src.Equipo.Nombre : null))
            .ReverseMap()
            .ForMember(dest => dest.Zona, opt => opt.Ignore())
            .ForMember(dest => dest.Categoria, opt => opt.Ignore())
            .ForMember(dest => dest.Equipo, opt => opt.Ignore());
        CreateMap<Zona, ZonaDeFaseDTO>()
            .Include<ZonaTodosContraTodos, ZonaDeFaseDTO>()
            .Include<ZonaEliminacionDirecta, ZonaDeFaseDTO>();
        CreateMap<ZonaTodosContraTodos, ZonaDeFaseDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.Orden, opt => opt.MapFrom(src => src.Orden))
            .ForMember(dest => dest.CantidadDeEquipos, opt => opt.MapFrom<CantidadEquiposDeZonaResolver>())
            .ForMember(dest => dest.CategoriaId, opt => opt.MapFrom(_ => (int?)null))
            .ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(_ => (string?)null));
        CreateMap<ZonaEliminacionDirecta, ZonaDeFaseDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.Orden, opt => opt.MapFrom(src => src.Orden))
            .ForMember(dest => dest.CantidadDeEquipos, opt => opt.MapFrom<CantidadEquiposDeZonaResolver>())
            .ForMember(dest => dest.CategoriaId, opt => opt.MapFrom(src => src.CategoriaId))
            .ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Nombre : null));

        CreateMap<Zona, ZonaResumenDTO>()
            .Include<ZonaTodosContraTodos, ZonaResumenDTO>()
            .Include<ZonaEliminacionDirecta, ZonaResumenDTO>();
        CreateMap<ZonaTodosContraTodos, ZonaResumenDTO>()
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => src.Fase != null && src.Fase.Torneo != null ? $"{src.Fase.Torneo.Nombre} {src.Fase.Torneo.Anio}" : string.Empty))
            .ForMember(dest => dest.Anio, opt => opt.MapFrom(src => src.Fase != null && src.Fase.Torneo != null ? (int?)src.Fase.Torneo.Anio : null))
            .ForMember(dest => dest.Agrupador, opt => opt.MapFrom(src => src.Fase != null && src.Fase.Torneo != null && src.Fase.Torneo.TorneoAgrupador != null ? src.Fase.Torneo.TorneoAgrupador.Nombre : string.Empty))
            .ForMember(dest => dest.AgrupadorId, opt => opt.MapFrom(src => src.Fase != null && src.Fase.Torneo != null ? (int?)src.Fase.Torneo.TorneoAgrupadorId : null));
        CreateMap<ZonaEliminacionDirecta, ZonaResumenDTO>()
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => src.Fase != null && src.Fase.Torneo != null ? $"{src.Fase.Torneo.Nombre} {src.Fase.Torneo.Anio}" : string.Empty))
            .ForMember(dest => dest.Anio, opt => opt.MapFrom(src => src.Fase != null && src.Fase.Torneo != null ? (int?)src.Fase.Torneo.Anio : null))
            .ForMember(dest => dest.Agrupador, opt => opt.MapFrom(src => src.Fase != null && src.Fase.Torneo != null && src.Fase.Torneo.TorneoAgrupador != null ? src.Fase.Torneo.TorneoAgrupador.Nombre : string.Empty))
            .ForMember(dest => dest.AgrupadorId, opt => opt.MapFrom(src => src.Fase != null && src.Fase.Torneo != null ? (int?)src.Fase.Torneo.TorneoAgrupadorId : null));

        CreateMap<Fase, FaseDTO>()
            .Include<FaseTodosContraTodos, FaseDTO>()
            .Include<FaseEliminacionDirecta, FaseDTO>();
        CreateMap<FaseTodosContraTodos, FaseDTO>()
            .ForMember(dest => dest.TipoDeFase, opt => opt.MapFrom(_ => TipoDeFaseEnum.TodosContraTodos))
            .ForMember(dest => dest.TipoDeFaseNombre, opt => opt.MapFrom(_ => "Todos contra todos"))
            .ForMember(dest => dest.EstadoFaseNombre, opt => opt.MapFrom(src => src.EstadoFase != null ? src.EstadoFase.Estado : string.Empty))
            .ForMember(dest => dest.SePuedeEditar, opt => opt.MapFrom(src =>
                src.Zonas == null || !src.Zonas.Any()))
            .ForMember(dest => dest.GrupoDeFasesId, opt => opt.MapFrom(src => src.GrupoDeFasesId))
            .ForMember(dest => dest.Zonas, opt => opt.MapFrom(src => src.Zonas != null ? src.Zonas.Cast<Zona>().ToList() : new List<Zona>()))
            .ForMember(dest => dest.Categorias, opt => opt.MapFrom(src => src.Categorias != null
                ? src.Categorias.OrderBy(c => c.Orden).ThenBy(c => c.Id).ToList()
                : new List<FaseCategoria>()))
            .PreserveReferences();
        CreateMap<FaseEliminacionDirecta, FaseDTO>()
            .ForMember(dest => dest.TipoDeFase, opt => opt.MapFrom(_ => TipoDeFaseEnum.EliminacionDirecta))
            .ForMember(dest => dest.TipoDeFaseNombre, opt => opt.MapFrom(_ => "Eliminación directa"))
            .ForMember(dest => dest.EstadoFaseNombre, opt => opt.MapFrom(src => src.EstadoFase != null ? src.EstadoFase.Estado : string.Empty))
            .ForMember(dest => dest.SePuedeEditar, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.GrupoDeFasesId, opt => opt.MapFrom(src => src.GrupoDeFasesId))
            .ForMember(dest => dest.Zonas, opt => opt.MapFrom(src => src.Zonas != null ? src.Zonas.Cast<Zona>().ToList() : new List<Zona>()))
            .ForMember(dest => dest.Categorias, opt => opt.MapFrom(src => src.Categorias != null
                ? src.Categorias.OrderBy(c => c.Orden).ThenBy(c => c.Id).ToList()
                : new List<FaseCategoria>()))
            .PreserveReferences();
        CreateMap<Equipo, EquipoDeLaZonaDTO>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.Club, opt => opt.MapFrom(src => src.Club != null ? src.Club.Nombre : string.Empty))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => GeneradorDeHash.GenerarAlfanumerico7Digitos(src.Id)));

        CreateMap<Zona, ZonaDTO>()
            .Include<ZonaTodosContraTodos, ZonaDTO>()
            .Include<ZonaEliminacionDirecta, ZonaDTO>();
        CreateMap<ZonaTodosContraTodos, ZonaDTO>()
            .ForMember(dest => dest.CategoriaId, opt => opt.MapFrom(_ => (int?)null))
            .ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(_ => (string?)null))
            .ForMember(dest => dest.Equipos, opt => opt.MapFrom<EquiposDeZonaResolver>())
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.Fase, opt => opt.Ignore())
            .ForMember(dest => dest.EquiposZona, opt => opt.Ignore())
            .ForMember(dest => dest.Fechas, opt => opt.Ignore())
            .ForSourceMember(src => src.Equipos, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.CategoriaNombre, opt => opt.DoNotValidate());
        CreateMap<ZonaEliminacionDirecta, ZonaDTO>()
            .ForMember(dest => dest.CategoriaId, opt => opt.MapFrom(src => src.CategoriaId))
            .ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Nombre : null))
            .ForMember(dest => dest.Equipos, opt => opt.MapFrom<EquiposDeZonaResolver>())
            .PreserveReferences();
        CreateMap<Fecha, FechaDTO>()
            .Include<FechaTodosContraTodos, FechaTodosContraTodosDTO>()
            .Include<FechaEliminacionDirecta, FechaEliminacionDirectaDTO>();
        CreateMap<FechaTodosContraTodos, FechaTodosContraTodosDTO>()
            .ForMember(dest => dest.Jornadas, opt => opt.MapFrom(src =>
                src.Jornadas != null ? src.Jornadas.OrderBy(j => j.Id).ToList() : new List<Jornada>()))
            .PreserveReferences();
        CreateMap<FechaEliminacionDirecta, FechaEliminacionDirectaDTO>()
            .ForMember(dest => dest.InstanciaNombre, opt => opt.MapFrom(src => src.Instancia != null ? src.Instancia.Nombre : null))
            .ForMember(dest => dest.Jornadas, opt => opt.MapFrom(src =>
                src.Jornadas != null ? src.Jornadas.OrderBy(j => j.Id).ToList() : new List<Jornada>()))
            .PreserveReferences();

        CreateMap<Partido, PartidoDTO>()
            .ForMember(dest => dest.Categoria,
                opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Nombre : string.Empty))
            .ForMember(dest => dest.Orden,
                opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Orden : 0));
        CreateMap<Jornada, JornadaDTO>()
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom<JornadaTipoResolver>())
            .ForMember(dest => dest.LocalId, opt => opt.MapFrom<JornadaLocalIdResolver>())
            .ForMember(dest => dest.VisitanteId, opt => opt.MapFrom<JornadaVisitanteIdResolver>())
            .ForMember(dest => dest.Local, opt => opt.MapFrom<JornadaLocalNombreResolver>())
            .ForMember(dest => dest.Visitante, opt => opt.MapFrom<JornadaVisitanteNombreResolver>())
            .ForMember(dest => dest.EquipoId, opt => opt.MapFrom<JornadaEquipoResolver>())
            .ForMember(dest => dest.Equipo, opt => opt.MapFrom<JornadaEquipoNombreResolver>())
            .ForMember(dest => dest.Numero, opt => opt.MapFrom<JornadaNumeroResolver>())
            .ForMember(dest => dest.LocalOVisitante, opt => opt.MapFrom<JornadaLocalOVisitanteResolver>())
            .ForMember(dest => dest.Partidos,
                opt => opt.MapFrom(src => src.Partidos != null ? src.Partidos.ToList() : new List<Partido>()));
        CreateMap<DniExpulsadoDeLaLiga, DniExpulsadoDeLaLigaDTO>().ReverseMap();
        CreateMap<ArbitroTorneoAgrupador, ArbitroTorneoAgrupadorDTO>()
            .ForMember(dest => dest.TorneoAgrupadorNombre, opt => opt.MapFrom(src => src.TorneoAgrupador.Nombre));
        CreateMap<ArbitroEquipoProhibido, ArbitroEquipoProhibidoDTO>()
            .ConvertUsing(src => ArbitroEquipoProhibidoMapper.Map(src));
        CreateMap<Arbitro, ArbitroDTO>()
            .ForMember(dest => dest.TorneoAgrupadorIds, opt => opt.MapFrom(src => src.ArbitroTorneoAgrupadores.Select(x => x.TorneoAgrupadorId).ToList()))
            .ForMember(dest => dest.TorneoAgrupadores, opt => opt.MapFrom(src => src.ArbitroTorneoAgrupadores))
            .ForMember(dest => dest.EquipoProhibidoIds, opt => opt.MapFrom(src => src.ArbitroEquiposProhibidos.Select(x => x.EquipoId).ToList()))
            .ForMember(dest => dest.EquiposProhibidos, opt => opt.MapFrom(src => src.ArbitroEquiposProhibidos))
            .ReverseMap()
            .ForMember(dest => dest.ArbitroTorneoAgrupadores, opt => opt.Ignore())
            .ForMember(dest => dest.ArbitroEquiposProhibidos, opt => opt.Ignore());
        CreateMap<SponsorWebPublica, SponsorWebPublicaDTO>()
            .ForMember(dest => dest.Imagen, opt => opt.Ignore())
            .ReverseMap()
            .ForMember(dest => dest.Orden, opt => opt.Ignore());
        CreateMap<Configuracion, ConfiguracionDTO>().ReverseMap();

        CreateMap<TorneoAgrupador, TorneoAgrupadorDTO>()
            .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color != null ? src.Color.Nombre : nameof(ColorEnum.Negro)))
            .ForMember(dest => dest.CantidadDeTorneos, opt => opt.MapFrom(src => src.Torneos != null ? src.Torneos.Count : 0))
            .ForMember(dest => dest.Torneos, opt => opt.MapFrom(src => src.Torneos))
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.Color, opt => opt.Ignore())
            .ForMember(dest => dest.ColorId, opt => opt.Ignore())
            .ForMember(dest => dest.Torneos, opt => opt.Ignore());

        CreateMap<Equipo, EquipoDTO>()
            .ForMember(dest => dest.ClubNombre, x => x.MapFrom(src => src.Club.Nombre))
            .ForMember(dest => dest.Zonas, x => x.MapFrom(src => src.Zonas != null ? src.Zonas.Select(ez => ez.Zona).ToList() : new List<Zona>()))
            .ForMember(dest => dest.CodigoAlfanumerico,
                x => x.MapFrom(src => GeneradorDeHash.GenerarAlfanumerico7Digitos(src.Id)))
            .PreserveReferences();

        CreateMap<EquipoDTO, Equipo>()
            .ForMember(dest => dest.Club, opt => opt.Ignore())
            .ForMember(dest => dest.Zonas, opt => opt.Ignore())
            .PreserveReferences();

        CreateMap<JugadorEquipo, JugadorDelEquipoDTO>()
            .ForMember(dest => dest.JugadorEquipoId, x => x.MapFrom(src => src.Id))
            .ForMember(dest => dest.Nombre, x => x.MapFrom(src => src.Jugador.Nombre))
            .ForMember(dest => dest.Apellido, x => x.MapFrom(src => src.Jugador.Apellido))
            .ForMember(dest => dest.Id, x => x.MapFrom(src => src.Jugador.Id))
            .ForMember(dest => dest.DNI, x => x.MapFrom(src => src.Jugador.DNI))
            .ForMember(dest => dest.AnioNacimiento, x => x.MapFrom(src => src.Jugador.FechaNacimiento.Year))
            .ForMember(dest => dest.Estado, x => x.MapFrom(src => src.EstadoJugador.Id))
            .ForMember(dest => dest.Motivo, x => x.MapFrom(src => src.Motivo))
            .ForMember(dest => dest.TarjetasAmarillas, x => x.MapFrom(src => src.TarjetasAmarillas))
            .ForMember(dest => dest.TarjetasRojas, x => x.MapFrom(src => src.TarjetasRojas))
            .PreserveReferences().ReverseMap();

        CreateMap<JugadorEquipo, EquipoDelJugadorDTO>()
            .ForMember(dest => dest.EquipoId, x => x.MapFrom(src => src.EquipoId))
            .ForMember(dest => dest.Nombre, x => x.MapFrom(src => src.Equipo.Nombre))
            .ForMember(dest => dest.Club, x => x.MapFrom(src => src.Equipo.Club.Nombre))
            .ForMember(dest => dest.Torneo, x => x.MapFrom(src => TorneoNombreDesdePrimeraZona(src.Equipo)))
            .ForMember(dest => dest.Estado, x => x.MapFrom(src => src.EstadoJugador.Id))
            .ForMember(dest => dest.FechaPagoDeFichaje, opt => opt.MapFrom<FechaPagoResolver>())
            .ForMember(dest => dest.Motivo, x => x.MapFrom(src => src.Motivo))
            .ForMember(dest => dest.TarjetasAmarillas, x => x.MapFrom(src => src.TarjetasAmarillas))
            .ForMember(dest => dest.TarjetasRojas, x => x.MapFrom(src => src.TarjetasRojas))
            .PreserveReferences().ReverseMap();

        CreateMap<DelegadoClub, DelegadoClubDTO>()
            .ForMember(dest => dest.ClubNombre, x => x.MapFrom(src => src.Club.Nombre))
            .ForMember(dest => dest.EquiposDelClub, x => x.MapFrom(src => src.Club.Equipos.Select(e => e.Nombre).ToList()))
            .PreserveReferences();

        CreateMap<Usuario, UsuarioDTO>()
            .ForMember(dest => dest.DelegadoId, x => x.MapFrom(src => src.DelegadoId))
            .PreserveReferences();

        CreateMap<Usuario, UsuarioAdminDTO>()
            .ForMember(dest => dest.RolNombre, opt => opt.MapFrom(src => src.Rol.Nombre))
            .ForMember(dest => dest.BlanqueoPendiente, opt => opt.MapFrom(src => src.Password == null))
            .ForMember(dest => dest.AccesosModulo, opt => opt.MapFrom(src => src.AccesosModulo))
            .ReverseMap()
            .ForMember(dest => dest.Password, opt => opt.Ignore())
            .ForMember(dest => dest.Rol, opt => opt.Ignore())
            .ForMember(dest => dest.DelegadoId, opt => opt.Ignore())
            .ForMember(dest => dest.Delegado, opt => opt.Ignore())
            .ForMember(dest => dest.AccesosModulo, opt => opt.Ignore());

        CreateMap<UsuarioAccesoModulo, UsuarioAccesoModuloDTO>().ReverseMap();

        CreateMap<Rol, RolDTO>();

        CreateMap<Delegado, DelegadoDTO>()
            .ForMember(dest => dest.Usuario, x => x.MapFrom(src => src.Usuario))
            .ForMember(dest => dest.BlanqueoPendiente, x => x.MapFrom(src => src.Usuario != null && src.Usuario.Password == null))
            .ForMember(dest => dest.ClubIds, x => x.MapFrom(src => src.DelegadoClubs.Select(dc => dc.ClubId).ToList()))
            .ForMember(dest => dest.DelegadoClubs, x => x.MapFrom(src => src.DelegadoClubs))
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.DelegadoClubs, opt => opt.Ignore());


        CreateMap<Jugador, JugadorDTO>()
            .ForMember(dest => dest.Equipos, x => x.MapFrom(src => src.JugadorEquipos))
            .PreserveReferences().ReverseMap();

        CreateMap<Jugador, JugadorBaseDTO>();

        CreateMap<JugadorEquipo, JugadorEquipoDTO>().PreserveReferences().ReverseMap();
        CreateMap<EstadoJugador, EstadoJugadorDTO>().PreserveReferences().ReverseMap();
        CreateMap<EstadoDelegado, EstadoDelegadoDTO>().PreserveReferences();

        CreateMap<JugadorEquipo, EquipoDTO>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Equipo.Nombre))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Equipo.Id));

        CreateMap<Equipo, EquipoBaseDTO>()
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => TorneoNombreDesdePrimeraZona(src)))
            .ForMember(dest => dest.CodigoAlfanumerico, opt => opt.MapFrom(src => GeneradorDeHash.GenerarAlfanumerico7Digitos(src.Id)));

        CreateMap<FixtureAlgoritmo, FixtureAlgoritmoDTO>()
            .ForMember(d => d.FixtureAlgoritmoId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Fechas, opt => opt.MapFrom(s =>
                (s.Fechas ?? new List<FixtureAlgoritmoFecha>())
                    .OrderBy(f => f.Fecha)
                    .ThenBy(f => f.Orden)
                    .ThenBy(f => f.Id)
                    .ToList()));
        CreateMap<FixtureAlgoritmoDTO, FixtureAlgoritmo>()
            .ForMember(d => d.Fechas, opt => opt.Ignore());
        CreateMap<FixtureAlgoritmoFecha, FixtureAlgoritmoFechaDTO>();

        CreateMap<Delegado, CarnetDigitalDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.DNI, opt => opt.MapFrom(src => src.DNI))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.Apellido, opt => opt.MapFrom(src => src.Apellido))
            .ForMember(dest => dest.FechaNacimiento, opt => opt.MapFrom(src => src.FechaNacimiento))
            .ForMember(dest => dest.TarjetasAmarillas, opt => opt.MapFrom(_ => 0))
            .ForMember(dest => dest.TarjetasRojas, opt => opt.MapFrom(_ => 0));

        CreateMap<JugadorEquipo, CarnetDigitalDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Jugador.Id))
            .ForMember(dest => dest.DNI, opt => opt.MapFrom(src => src.Jugador.DNI))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Jugador.Nombre))
            .ForMember(dest => dest.Apellido, opt => opt.MapFrom(src => src.Jugador.Apellido))
            .ForMember(dest => dest.FechaNacimiento, opt => opt.MapFrom(src => src.Jugador.FechaNacimiento))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.EstadoJugadorId))
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => TorneoNombreDesdePrimeraZona(src.Equipo)))
            .ForMember(dest => dest.Color, opt => opt.MapFrom(src => ColorAgrupadorDesdePrimeraZona(src.Equipo)))
            .ForMember(dest => dest.TarjetasAmarillas, opt => opt.MapFrom(src => src.TarjetasAmarillas))
            .ForMember(dest => dest.TarjetasRojas, opt => opt.MapFrom(src => src.TarjetasRojas));

        CreateMap<JugadorEquipo, CarnetDigitalPendienteDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Jugador.Id))
            .ForMember(dest => dest.DNI, opt => opt.MapFrom(src => src.Jugador.DNI))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Jugador.Nombre))
            .ForMember(dest => dest.Apellido, opt => opt.MapFrom(src => src.Jugador.Apellido))
            .ForMember(dest => dest.FechaNacimiento, opt => opt.MapFrom(src => src.Jugador.FechaNacimiento))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.EstadoJugadorId))
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => TorneoNombreDesdePrimeraZona(src.Equipo)))
            .ForMember(dest => dest.Color, opt => opt.MapFrom(src => ColorAgrupadorDesdePrimeraZona(src.Equipo)))
            .ForMember(dest => dest.Motivo, opt => opt.MapFrom(src => src.Motivo))
            .ForMember(dest => dest.TarjetasAmarillas, opt => opt.MapFrom(src => src.TarjetasAmarillas))
            .ForMember(dest => dest.TarjetasRojas, opt => opt.MapFrom(src => src.TarjetasRojas));

        // CreateMap<string, DateTime>().ConvertUsing(s => 
        //     DateTime.ParseExact(s, "dd-MM-yyyy", CultureInfo.InvariantCulture)
        // );
    }

    private static string TorneoNombreDesdePrimeraZona(Equipo? equipo)
    {
        if (equipo?.Zonas == null || !equipo.Zonas.Any())
            return "";
        return TorneoNombreDesdeZona(equipo.Zonas.First().Zona);
    }

    private static string ColorAgrupadorDesdePrimeraZona(Equipo? equipo)
    {
        if (equipo?.Zonas == null || !equipo.Zonas.Any())
            return "";
        return ColorAgrupadorDesdeZona(equipo.Zonas.First().Zona);
    }

    private static string ColorAgrupadorDesdeZona(Zona? zona)
    {
        var torneo = TorneoDesdeZona(zona);
        return torneo?.TorneoAgrupador?.Color?.Nombre ?? "";
    }

    private static Torneo? TorneoDesdeZona(Zona? zona)
    {
        if (zona == null) return null;
        return zona switch
        {
            ZonaTodosContraTodos z => z.Fase?.Torneo,
            ZonaEliminacionDirecta z => z.Fase?.Torneo,
            _ => null
        };
    }

    private static string TorneoNombreDesdeZona(Zona? zona)
    {
        return TorneoDesdeZona(zona)?.Nombre ?? "";
    }
}

public class FechaPagoResolver : IValueResolver<JugadorEquipo, EquipoDelJugadorDTO, DateTime?>
{
    public DateTime? Resolve(JugadorEquipo source, EquipoDelJugadorDTO destination, DateTime? destMember, ResolutionContext context)
    {
        if (source.HistorialDePagos == null)
            return null;

        return source.HistorialDePagos.Fecha;
    }
}

public class EquiposDeZonaResolver : IValueResolver<Zona, ZonaDTO, List<EquipoDeLaZonaDTO>?>
{
    public List<EquipoDeLaZonaDTO>? Resolve(Zona source, ZonaDTO destination, List<EquipoDeLaZonaDTO>? destMember, ResolutionContext context)
    {
        var equipos = source.EquiposZona != null ? source.EquiposZona.Select(e => e.Equipo).Where(e => e != null).ToList()! : new List<Equipo>();
        return context.Mapper.Map<List<EquipoDeLaZonaDTO>>(equipos);
    }
}

public class CantidadEquiposDeZonaResolver : IValueResolver<Zona, ZonaDeFaseDTO, int>
{
    public int Resolve(Zona source, ZonaDeFaseDTO destination, int destMember, ResolutionContext context)
    {
        return source.EquiposZona != null ? source.EquiposZona.Count : 0;
    }
}

public class JornadaTipoResolver : IValueResolver<Jornada, JornadaDTO, string>
{
    public string Resolve(Jornada source, JornadaDTO destination, string destMember, ResolutionContext context)
    {
        return source switch
        {
            JornadaNormal => "Normal",
            JornadaLibre => "Libre",
            JornadaInterzonal => "Interzonal",
            JornadaSinEquipos => "SinEquipos",
            _ => "Normal"
        };
    }
}

public class JornadaLocalIdResolver : IValueResolver<Jornada, JornadaDTO, int?>
{
    public int? Resolve(Jornada source, JornadaDTO destination, int? destMember, ResolutionContext context)
        => source is JornadaNormal n ? n.LocalEquipoId : null;
}

public class JornadaVisitanteIdResolver : IValueResolver<Jornada, JornadaDTO, int?>
{
    public int? Resolve(Jornada source, JornadaDTO destination, int? destMember, ResolutionContext context)
        => source is JornadaNormal n ? n.VisitanteEquipoId : null;
}

public class JornadaLocalNombreResolver : IValueResolver<Jornada, JornadaDTO, string?>
{
    public string? Resolve(Jornada source, JornadaDTO destination, string? destMember, ResolutionContext context)
        => source is JornadaNormal n ? n.LocalEquipo?.Nombre : null;
}

public class JornadaVisitanteNombreResolver : IValueResolver<Jornada, JornadaDTO, string?>
{
    public string? Resolve(Jornada source, JornadaDTO destination, string? destMember, ResolutionContext context)
        => source is JornadaNormal n ? n.VisitanteEquipo?.Nombre : null;
}

public class TorneoSePuedeEditarResolver : IValueResolver<Torneo, TorneoDTO, bool>
{
    public bool Resolve(Torneo source, TorneoDTO destination, bool destMember, ResolutionContext context)
    {
        if (source.Fases == null || !source.Fases.Any())
            return true;
        return source.Fases.All(f =>
            f is not FaseTodosContraTodos t || t.Zonas == null || !t.Zonas.Any());
    }
}

public class JornadaNumeroResolver : IValueResolver<Jornada, JornadaDTO, int?>
{
    public int? Resolve(Jornada source, JornadaDTO destination, int? destMember, ResolutionContext context)
        => source is JornadaInterzonal i ? i.Numero : null;
}

public class JornadaEquipoResolver : IValueResolver<Jornada, JornadaDTO, int?>
{
    public int? Resolve(Jornada source, JornadaDTO destination, int? destMember, ResolutionContext context)
        => source switch
        {
            JornadaLibre l => l.EquipoId,
            JornadaInterzonal i => i.EquipoId,
            _ => null
        };
}

public class JornadaEquipoNombreResolver : IValueResolver<Jornada, JornadaDTO, string?>
{
    public string? Resolve(Jornada source, JornadaDTO destination, string? destMember, ResolutionContext context)
        => source switch
        {
            JornadaLibre l => l.Equipo?.Nombre,
            JornadaInterzonal i => i.Equipo?.Nombre,
            _ => null
        };
}

public class JornadaLocalOVisitanteResolver : IValueResolver<Jornada, JornadaDTO, LocalVisitanteEnum?>
{
    public LocalVisitanteEnum? Resolve(Jornada source, JornadaDTO destination, LocalVisitanteEnum? destMember, ResolutionContext context)
        => source switch
        {
            JornadaLibre l => (LocalVisitanteEnum)l.LocalOVisitanteId,
            JornadaInterzonal i => (LocalVisitanteEnum)i.LocalOVisitanteId,
            _ => null
        };
}