using System.Globalization;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Entidades.EntidadesConValoresPredefinidos;
using Api.Core.Enums;
using Api.Core.Logica;
using AutoMapper;
using System.Linq;
using Api.Core.DTOs.AppCarnetDigital;

namespace Api._Config;

public class MapperConfig : Profile
{
    public MapperConfig()
    {
        CreateMap<Club, ClubDTO>()
            .ForMember(dest => dest.Delegados, opt => opt.MapFrom(src => src.DelegadoClubs.Select(dc => dc.Delegado).ToList()))
            .ForMember(dest => dest.Direccion, opt => opt.MapFrom(src => src.Direccion))
            .ForMember(dest => dest.EsTechado, opt => opt.MapFrom(src => src.EsTechado))
            .ForMember(dest => dest.Localidad, opt => opt.MapFrom(src => src.Localidad))
            .PreserveReferences().ReverseMap()
            .ForMember(dest => dest.DelegadoClubs, opt => opt.Ignore());
        CreateMap<Torneo, TorneoDTO>()
            .ForMember(dest => dest.TorneoAgrupadorNombre, opt => opt.MapFrom(src => src.TorneoAgrupador != null ? src.TorneoAgrupador.Nombre : string.Empty))
            .ForMember(dest => dest.SePuedeEditar, opt => opt.MapFrom<TorneoSePuedeEditarResolver>())
            .ForMember(dest => dest.Fases, opt => opt.MapFrom(src => src.Fases != null ? src.Fases : new List<TorneoFase>()))
            .ForMember(dest => dest.Categorias, opt => opt.MapFrom(src => src.Categorias != null ? src.Categorias : new List<TorneoCategoria>()))
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.TorneoAgrupador, opt => opt.Ignore())
            .ForMember(dest => dest.Fases, opt => opt.Ignore())
            .ForMember(dest => dest.Categorias, opt => opt.Ignore())
            .ForSourceMember(src => src.SePuedeEditar, opt => opt.DoNotValidate());

        CreateMap<TorneoCategoria, TorneoCategoriaDTO>()
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.Torneo, opt => opt.Ignore());
        CreateMap<TorneoZona, ZonaDeFaseDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.CantidadDeEquipos, opt => opt.MapFrom<CantidadEquiposDeZonaResolver>());

        CreateMap<TorneoZona, ZonaDTO>()
            .Include<ZonaTodosContraTodos, ZonaDTO>()
            .Include<ZonaEliminacionDirecta, ZonaDTO>();
        CreateMap<ZonaTodosContraTodos, ZonaDTO>()
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => src.Fase != null && src.Fase.Torneo != null ? $"{src.Fase.Torneo.Nombre} {src.Fase.Torneo.Anio}" : string.Empty))
            .ForMember(dest => dest.Agrupador, opt => opt.MapFrom(src => src.Fase != null && src.Fase.Torneo != null && src.Fase.Torneo.TorneoAgrupador != null ? src.Fase.Torneo.TorneoAgrupador.Nombre : string.Empty))
            .ForMember(dest => dest.AgrupadorId, opt => opt.MapFrom(src => src.Fase != null && src.Fase.Torneo != null ? (int?)src.Fase.Torneo.TorneoAgrupadorId : null));
        CreateMap<ZonaEliminacionDirecta, ZonaDTO>()
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => src.Fase != null && src.Fase.Torneo != null ? $"{src.Fase.Torneo.Nombre} {src.Fase.Torneo.Anio}" : string.Empty))
            .ForMember(dest => dest.Agrupador, opt => opt.MapFrom(src => src.Fase != null && src.Fase.Torneo != null && src.Fase.Torneo.TorneoAgrupador != null ? src.Fase.Torneo.TorneoAgrupador.Nombre : string.Empty))
            .ForMember(dest => dest.AgrupadorId, opt => opt.MapFrom(src => src.Fase != null && src.Fase.Torneo != null ? (int?)src.Fase.Torneo.TorneoAgrupadorId : null));

        CreateMap<TorneoFase, TorneoFaseDTO>()
            .Include<FaseTodosContraTodos, TorneoFaseDTO>()
            .Include<FaseEliminacionDirecta, TorneoFaseDTO>();
        CreateMap<FaseTodosContraTodos, TorneoFaseDTO>()
            .ForMember(dest => dest.TipoDeFase, opt => opt.MapFrom(_ => TipoDeFaseEnum.TodosContraTodos))
            .ForMember(dest => dest.TipoDeFaseNombre, opt => opt.MapFrom(_ => "Todos contra todos"))
            .ForMember(dest => dest.InstanciaEliminacionDirectaNombre, opt => opt.MapFrom(_ => (string?)null))
            .ForMember(dest => dest.EstadoFaseNombre, opt => opt.MapFrom(src => src.EstadoFase != null ? src.EstadoFase.Estado : string.Empty))
            .ForMember(dest => dest.SePuedeEditar, opt => opt.MapFrom(src =>
                src.Zonas == null || !src.Zonas.Any()))
            .ForMember(dest => dest.Zonas, opt => opt.MapFrom(src => src.Zonas != null ? src.Zonas.Cast<TorneoZona>().ToList() : new List<TorneoZona>()))
            .PreserveReferences();
        CreateMap<FaseEliminacionDirecta, TorneoFaseDTO>()
            .ForMember(dest => dest.TipoDeFase, opt => opt.MapFrom(_ => TipoDeFaseEnum.EliminacionDirecta))
            .ForMember(dest => dest.TipoDeFaseNombre, opt => opt.MapFrom(_ => "Eliminación directa"))
            .ForMember(dest => dest.InstanciaEliminacionDirectaNombre, opt => opt.MapFrom(src => src.InstanciaEliminacionDirecta != null ? src.InstanciaEliminacionDirecta.Nombre : null))
            .ForMember(dest => dest.EstadoFaseNombre, opt => opt.MapFrom(src => src.EstadoFase != null ? src.EstadoFase.Estado : string.Empty))
            .ForMember(dest => dest.SePuedeEditar, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.Zonas, opt => opt.MapFrom(src => src.Zonas != null ? src.Zonas.Cast<TorneoZona>().ToList() : new List<TorneoZona>()))
            .PreserveReferences();
        CreateMap<Equipo, EquipoDeLaZonaDTO>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.Club, opt => opt.MapFrom(src => src.Club != null ? src.Club.Nombre : string.Empty))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => GeneradorDeHash.GenerarAlfanumerico7Digitos(src.Id)));

        CreateMap<TorneoZona, TorneoZonaDTO>()
            .Include<ZonaTodosContraTodos, TorneoZonaDTO>()
            .Include<ZonaEliminacionDirecta, TorneoZonaDTO>();
        CreateMap<ZonaTodosContraTodos, TorneoZonaDTO>()
            .ForMember(dest => dest.Equipos, opt => opt.MapFrom<EquiposDeZonaResolver>())
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.Fase, opt => opt.Ignore())
            .ForMember(dest => dest.EquiposZona, opt => opt.Ignore())
            .ForMember(dest => dest.Fechas, opt => opt.Ignore())
            .ForSourceMember(src => src.Equipos, opt => opt.DoNotValidate());
        CreateMap<ZonaEliminacionDirecta, TorneoZonaDTO>()
            .ForMember(dest => dest.Equipos, opt => opt.MapFrom<EquiposDeZonaResolver>())
            .PreserveReferences();
        CreateMap<TorneoFecha, TorneoFechaDTO>()
            .Include<FechaTodosContraTodos, TorneoFechaDTO>()
            .Include<FechaEliminacionDirecta, TorneoFechaDTO>();
        CreateMap<FechaTodosContraTodos, TorneoFechaDTO>()
            .ForMember(dest => dest.InstanciaEliminacionDirectaNombre, opt => opt.MapFrom(_ => (string?)null))
            .ForMember(dest => dest.Jornadas, opt => opt.MapFrom(src => src.Jornadas != null ? src.Jornadas : new List<Jornada>()))
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.Zona, opt => opt.Ignore())
            .ForMember(dest => dest.Jornadas, opt => opt.Ignore())
            .ForSourceMember(src => src.InstanciaEliminacionDirectaNombre, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.Jornadas, opt => opt.DoNotValidate());
        CreateMap<FechaEliminacionDirecta, TorneoFechaDTO>()
            .ForMember(dest => dest.Numero, opt => opt.MapFrom(_ => 0))
            .ForMember(dest => dest.InstanciaEliminacionDirectaNombre, opt => opt.MapFrom(src => src.InstanciaEliminacionDirecta != null ? src.InstanciaEliminacionDirecta.Nombre : null))
            .ForMember(dest => dest.Jornadas, opt => opt.MapFrom(src => src.Jornadas != null ? src.Jornadas : new List<Jornada>()))
            .PreserveReferences();

        CreateMap<Jornada, JornadaDTO>()
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom<JornadaTipoResolver>())
            .ForMember(dest => dest.LocalId, opt => opt.MapFrom<JornadaLocalIdResolver>())
            .ForMember(dest => dest.VisitanteId, opt => opt.MapFrom<JornadaVisitanteIdResolver>())
            .ForMember(dest => dest.Local, opt => opt.MapFrom<JornadaLocalNombreResolver>())
            .ForMember(dest => dest.Visitante, opt => opt.MapFrom<JornadaVisitanteNombreResolver>())
            .ForMember(dest => dest.EquipoId, opt => opt.MapFrom<JornadaEquipoResolver>())
            .ForMember(dest => dest.Equipo, opt => opt.MapFrom<JornadaEquipoNombreResolver>())
            .ForMember(dest => dest.LocalOVisitante, opt => opt.MapFrom<JornadaLocalOVisitanteResolver>());
        CreateMap<TorneoAgrupador, TorneoAgrupadorDTO>()
            .ForMember(dest => dest.CantidadDeTorneos, opt => opt.MapFrom(src => src.Torneos != null ? src.Torneos.Count : 0))
            .ForMember(dest => dest.Torneos, opt => opt.MapFrom(src => src.Torneos))
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.Torneos, opt => opt.Ignore());

        CreateMap<Equipo, EquipoDTO>()
            .ForMember(dest => dest.ClubNombre, x => x.MapFrom(src => src.Club.Nombre))
            .ForMember(dest => dest.Zonas, x => x.MapFrom(src => src.Zonas != null ? src.Zonas.Select(ez => ez.Zona).ToList() : new List<TorneoZona>()))
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
            .ForMember(dest => dest.Estado, x => x.MapFrom(src => src.EstadoJugador.Id))
            .ForMember(dest => dest.Motivo, x => x.MapFrom(src => src.Motivo))
            .PreserveReferences().ReverseMap();

        CreateMap<JugadorEquipo, EquipoDelJugadorDTO>()
            .ForMember(dest => dest.EquipoId, x => x.MapFrom(src => src.EquipoId))
            .ForMember(dest => dest.Nombre, x => x.MapFrom(src => src.Equipo.Nombre))
            .ForMember(dest => dest.Club, x => x.MapFrom(src => src.Equipo.Club.Nombre))
            .ForMember(dest => dest.Torneo, x => x.MapFrom(src => TorneoNombreDesdePrimeraZona(src.Equipo)))
            .ForMember(dest => dest.Estado, x => x.MapFrom(src => src.EstadoJugador.Id))
            .ForMember(dest => dest.FechaPagoDeFichaje, opt => opt.MapFrom<FechaPagoResolver>())
            .ForMember(dest => dest.Motivo, x => x.MapFrom(src => src.Motivo))
            .PreserveReferences().ReverseMap();

        CreateMap<DelegadoClub, DelegadoClubDTO>()
            .ForMember(dest => dest.ClubNombre, x => x.MapFrom(src => src.Club.Nombre))
            .ForMember(dest => dest.EquiposDelClub, x => x.MapFrom(src => src.Club.Equipos.Select(e => e.Nombre).ToList()))
            .PreserveReferences();

        CreateMap<Usuario, UsuarioDTO>()
            .ForMember(dest => dest.DelegadoId, x => x.MapFrom(src => src.DelegadoId))
            .PreserveReferences();

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
            .ForMember(d => d.Fechas, opt => opt.MapFrom(s => s.Fechas ?? new List<FixtureAlgoritmoFecha>()));
        CreateMap<FixtureAlgoritmoDTO, FixtureAlgoritmo>()
            .ForMember(d => d.Fechas, opt => opt.Ignore());
        CreateMap<FixtureAlgoritmoFecha, FixtureAlgoritmoFechaDTO>();

        CreateMap<Delegado, CarnetDigitalDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.DNI, opt => opt.MapFrom(src => src.DNI))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.Apellido, opt => opt.MapFrom(src => src.Apellido))
            .ForMember(dest => dest.FechaNacimiento, opt => opt.MapFrom(src => src.FechaNacimiento));

        CreateMap<JugadorEquipo, CarnetDigitalDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Jugador.Id))
            .ForMember(dest => dest.DNI, opt => opt.MapFrom(src => src.Jugador.DNI))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Jugador.Nombre))
            .ForMember(dest => dest.Apellido, opt => opt.MapFrom(src => src.Jugador.Apellido))
            .ForMember(dest => dest.FechaNacimiento, opt => opt.MapFrom(src => src.Jugador.FechaNacimiento))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.EstadoJugadorId))
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => TorneoNombreDesdePrimeraZona(src.Equipo)));

        CreateMap<JugadorEquipo, CarnetDigitalPendienteDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Jugador.Id))
            .ForMember(dest => dest.DNI, opt => opt.MapFrom(src => src.Jugador.DNI))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Jugador.Nombre))
            .ForMember(dest => dest.Apellido, opt => opt.MapFrom(src => src.Jugador.Apellido))
            .ForMember(dest => dest.FechaNacimiento, opt => opt.MapFrom(src => src.Jugador.FechaNacimiento))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.EstadoJugadorId))
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => TorneoNombreDesdePrimeraZona(src.Equipo)))
            .ForMember(dest => dest.Motivo, opt => opt.MapFrom(src => src.Motivo));

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

    private static string TorneoNombreDesdeZona(TorneoZona? zona)
    {
        if (zona == null) return "";
        return zona switch
        {
            ZonaTodosContraTodos z => z.Fase?.Torneo?.Nombre ?? "",
            ZonaEliminacionDirecta z => z.Fase?.Torneo?.Nombre ?? "",
            _ => ""
        };
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

public class EquiposDeZonaResolver : IValueResolver<TorneoZona, TorneoZonaDTO, List<EquipoDeLaZonaDTO>?>
{
    public List<EquipoDeLaZonaDTO>? Resolve(TorneoZona source, TorneoZonaDTO destination, List<EquipoDeLaZonaDTO>? destMember, ResolutionContext context)
    {
        var equipos = source.EquiposZona != null ? source.EquiposZona.Select(e => e.Equipo).Where(e => e != null).ToList()! : new List<Equipo>();
        return context.Mapper.Map<List<EquipoDeLaZonaDTO>>(equipos);
    }
}

public class CantidadEquiposDeZonaResolver : IValueResolver<TorneoZona, ZonaDeFaseDTO, int>
{
    public int Resolve(TorneoZona source, ZonaDeFaseDTO destination, int destMember, ResolutionContext context)
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

public class JornadaEquipoResolver : IValueResolver<Jornada, JornadaDTO, int?>
{
    public int? Resolve(Jornada source, JornadaDTO destination, int? destMember, ResolutionContext context)
        => source is JornadaLibre l ? l.EquipoId : source is JornadaInterzonal i ? i.EquipoId : null;
}

public class JornadaEquipoNombreResolver : IValueResolver<Jornada, JornadaDTO, string?>
{
    public string? Resolve(Jornada source, JornadaDTO destination, string? destMember, ResolutionContext context)
        => source is JornadaLibre l ? l.Equipo?.Nombre : source is JornadaInterzonal i ? i.Equipo?.Nombre : null;
}

public class JornadaLocalOVisitanteResolver : IValueResolver<Jornada, JornadaDTO, LocalVisitanteEnum?>
{
    public LocalVisitanteEnum? Resolve(Jornada source, JornadaDTO destination, LocalVisitanteEnum? destMember, ResolutionContext context)
        => source is JornadaInterzonal i ? (LocalVisitanteEnum)i.LocalOVisitanteId : null;
}