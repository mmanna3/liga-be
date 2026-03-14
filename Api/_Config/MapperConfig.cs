using System.Globalization;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Entidades.EntidadesConValoresPredefinidos;
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
            .ForMember(dest => dest.SePuedeEditar, opt => opt.MapFrom(src =>
                src.Fases == null || !src.Fases.Any() || src.Fases.All(f =>
                    f.Zonas == null || !f.Zonas.Any())))
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
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.CantidadDeEquipos, opt => opt.MapFrom<CantidadEquiposDeZonaResolver>());

        CreateMap<TorneoZona, ZonaDTO>()
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => src.TorneoFase != null && src.TorneoFase.Torneo != null ? $"{src.TorneoFase.Torneo.Nombre} {src.TorneoFase.Torneo.Anio}" : string.Empty))
            .ForMember(dest => dest.Agrupador, opt => opt.MapFrom(src => src.TorneoFase != null && src.TorneoFase.Torneo != null && src.TorneoFase.Torneo.TorneoAgrupador != null ? src.TorneoFase.Torneo.TorneoAgrupador.Nombre : string.Empty))
            .ForMember(dest => dest.AgrupadorId, opt => opt.MapFrom(src => src.TorneoFase != null && src.TorneoFase.Torneo != null ? (int?)src.TorneoFase.Torneo.TorneoAgrupadorId : null))
            .ForMember(dest => dest.EsExcluyente, opt => opt.MapFrom(src => src.TorneoFase != null && src.TorneoFase.EsExcluyente));

        CreateMap<TorneoFase, TorneoFaseDTO>()
            .ForMember(dest => dest.FaseFormatoNombre, opt => opt.MapFrom(src => src.FaseFormato != null ? src.FaseFormato.Nombre : string.Empty))
            .ForMember(dest => dest.InstanciaEliminacionDirectaNombre, opt => opt.MapFrom(src => src.InstanciaEliminacionDirecta != null ? src.InstanciaEliminacionDirecta.Nombre : null))
            .ForMember(dest => dest.EstadoFaseNombre, opt => opt.MapFrom(src => src.EstadoFase != null ? src.EstadoFase.Estado : string.Empty))
            .ForMember(dest => dest.SePuedeEditar, opt => opt.MapFrom(src =>
                src.Zonas == null || !src.Zonas.Any()))
            .ForMember(dest => dest.Zonas, opt => opt.MapFrom(src => src.Zonas != null ? src.Zonas : new List<TorneoZona>()))
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.Torneo, opt => opt.Ignore())
            .ForMember(dest => dest.FaseFormato, opt => opt.Ignore())
            .ForMember(dest => dest.InstanciaEliminacionDirecta, opt => opt.Ignore())
            .ForMember(dest => dest.EstadoFase, opt => opt.Ignore())
            .ForMember(dest => dest.Zonas, opt => opt.Ignore())
            .ForSourceMember(src => src.FaseFormatoNombre, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.InstanciaEliminacionDirectaNombre, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.EstadoFaseNombre, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.SePuedeEditar, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.Zonas, opt => opt.DoNotValidate());
        CreateMap<Equipo, EquipoDeLaZonaDTO>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.Club, opt => opt.MapFrom(src => src.Club != null ? src.Club.Nombre : string.Empty))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => GeneradorDeHash.GenerarAlfanumerico7Digitos(src.Id)));

        CreateMap<TorneoZona, TorneoZonaDTO>()
            .ForMember(dest => dest.Equipos, opt => opt.MapFrom<EquiposDeZonaResolver>())
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.TorneoFase, opt => opt.Ignore())
            .ForMember(dest => dest.Equipos, opt => opt.Ignore())
            .ForMember(dest => dest.Fechas, opt => opt.Ignore())
            .ForSourceMember(src => src.Equipos, opt => opt.DoNotValidate());
        CreateMap<TorneoFecha, TorneoFechaDTO>()
            .ForMember(dest => dest.InstanciaEliminacionDirectaNombre, opt => opt.MapFrom(src => src.InstanciaEliminacionDirecta != null ? src.InstanciaEliminacionDirecta.Nombre : null))
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.Zona, opt => opt.Ignore())
            .ForMember(dest => dest.InstanciaEliminacionDirecta, opt => opt.Ignore())
            .ForSourceMember(src => src.InstanciaEliminacionDirectaNombre, opt => opt.DoNotValidate());
        CreateMap<TorneoAgrupador, TorneoAgrupadorDTO>()
            .ForMember(dest => dest.CantidadDeTorneos, opt => opt.MapFrom(src => src.Torneos != null ? src.Torneos.Count : 0))
            .ForMember(dest => dest.Torneos, opt => opt.MapFrom(src => src.Torneos))
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.Torneos, opt => opt.Ignore());

        CreateMap<Equipo, EquipoDTO>()
            .ForMember(dest => dest.ClubNombre, x => x.MapFrom(src => src.Club.Nombre))
            .ForMember(dest => dest.ZonaExcluyente, x => x.MapFrom(src => src.ZonaExcluyente))
            .ForMember(dest => dest.ZonasNoExcluyentes, x => x.MapFrom(src => src.ZonasNoExcluyentes != null ? src.ZonasNoExcluyentes.Select(ez => ez.ZonaNoExcluyente).ToList() : new List<TorneoZona>()))
            .ForMember(dest => dest.CodigoAlfanumerico,
                x => x.MapFrom(src => GeneradorDeHash.GenerarAlfanumerico7Digitos(src.Id)))
            .PreserveReferences();

        CreateMap<EquipoDTO, Equipo>()
            .ForMember(dest => dest.Club, opt => opt.Ignore())
            .ForMember(dest => dest.ZonaExcluyente, opt => opt.Ignore())
            .ForMember(dest => dest.ZonaExcluyenteId, opt => opt.MapFrom(src => src.ZonaExcluyente != null ? src.ZonaExcluyente.Id : null))
            .ForMember(dest => dest.ZonasNoExcluyentes, opt => opt.Ignore())
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
            .ForMember(dest => dest.Torneo, x => x.MapFrom(src => src.Equipo.ZonaExcluyente != null && src.Equipo.ZonaExcluyente.TorneoFase != null && src.Equipo.ZonaExcluyente.TorneoFase.Torneo != null ? src.Equipo.ZonaExcluyente.TorneoFase.Torneo.Nombre : ""))
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
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => src.ZonaExcluyente != null && src.ZonaExcluyente.TorneoFase != null && src.ZonaExcluyente.TorneoFase.Torneo != null ? src.ZonaExcluyente.TorneoFase.Torneo.Nombre : ""))
            .ForMember(dest => dest.CodigoAlfanumerico, opt => opt.MapFrom(src => GeneradorDeHash.GenerarAlfanumerico7Digitos(src.Id)));

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
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => src.Equipo.ZonaExcluyente != null && src.Equipo.ZonaExcluyente.TorneoFase != null && src.Equipo.ZonaExcluyente.TorneoFase.Torneo != null ? src.Equipo.ZonaExcluyente.TorneoFase.Torneo.Nombre : ""));

        CreateMap<JugadorEquipo, CarnetDigitalPendienteDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Jugador.Id))
            .ForMember(dest => dest.DNI, opt => opt.MapFrom(src => src.Jugador.DNI))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Jugador.Nombre))
            .ForMember(dest => dest.Apellido, opt => opt.MapFrom(src => src.Jugador.Apellido))
            .ForMember(dest => dest.FechaNacimiento, opt => opt.MapFrom(src => src.Jugador.FechaNacimiento))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.EstadoJugadorId))
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => src.Equipo.ZonaExcluyente != null && src.Equipo.ZonaExcluyente.TorneoFase != null && src.Equipo.ZonaExcluyente.TorneoFase.Torneo != null ? src.Equipo.ZonaExcluyente.TorneoFase.Torneo.Nombre : ""))
            .ForMember(dest => dest.Motivo, opt => opt.MapFrom(src => src.Motivo));

        // CreateMap<string, DateTime>().ConvertUsing(s => 
        //     DateTime.ParseExact(s, "dd-MM-yyyy", CultureInfo.InvariantCulture)
        // );
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

public class EquiposDeZonaResolver : IValueResolver<TorneoZona, TorneoZonaDTO, List<EquipoDeLaZonaDTO>>
{
    public List<EquipoDeLaZonaDTO> Resolve(TorneoZona source, TorneoZonaDTO destination, List<EquipoDeLaZonaDTO> destMember, ResolutionContext context)
    {
        List<Equipo> equipos;
        if (source.TorneoFase != null && source.TorneoFase.EsExcluyente)
            equipos = source.Equipos != null ? source.Equipos.ToList() : new List<Equipo>();
        else if (source.TorneoFase != null && !source.TorneoFase.EsExcluyente)
            equipos = source.EquiposZonaNoExcluyente != null ? source.EquiposZonaNoExcluyente.Select(e => e.Equipo).Where(e => e != null).ToList()! : new List<Equipo>();
        else
        {
            // TorneoFase puede ser null si la zona se cargó sin Include (ej. desde TorneoFase.Zonas).
            // Una zona solo tiene equipos en una u otra colección, nunca en ambas.
            var fromExcluyente = source.Equipos != null ? source.Equipos.ToList() : new List<Equipo>();
            var fromNoExcluyente = source.EquiposZonaNoExcluyente != null ? source.EquiposZonaNoExcluyente.Select(e => e.Equipo).Where(e => e != null).ToList()! : new List<Equipo>();
            equipos = fromExcluyente.Count > 0 ? fromExcluyente : fromNoExcluyente;
        }
        return context.Mapper.Map<List<EquipoDeLaZonaDTO>>(equipos);
    }
}

public class CantidadEquiposDeZonaResolver : IValueResolver<TorneoZona, ZonaDeFaseDTO, int>
{
    public int Resolve(TorneoZona source, ZonaDeFaseDTO destination, int destMember, ResolutionContext context)
    {
        if (source.TorneoFase != null && source.TorneoFase.EsExcluyente)
            return source.Equipos != null ? source.Equipos.Count : 0;
        return source.EquiposZonaNoExcluyente != null ? source.EquiposZonaNoExcluyente.Count : 0;
    }
}