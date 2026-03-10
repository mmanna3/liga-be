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
            .PreserveReferences().ReverseMap()
            .ForMember(dest => dest.DelegadoClubs, opt => opt.Ignore());
        CreateMap<Torneo, TorneoDTO>()
            .ForMember(dest => dest.TorneoAgrupadorNombre, opt => opt.MapFrom(src => src.TorneoAgrupador != null ? src.TorneoAgrupador.Nombre : string.Empty))
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.TorneoAgrupador, opt => opt.Ignore());

        CreateMap<TorneoCategoria, TorneoCategoriaDTO>()
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.Torneo, opt => opt.Ignore());
        CreateMap<TorneoFase, TorneoFaseDTO>()
            .ForMember(dest => dest.FaseFormatoNombre, opt => opt.MapFrom(src => src.FaseFormato != null ? src.FaseFormato.Nombre : string.Empty))
            .ForMember(dest => dest.InstanciaEliminacionDirectaNombre, opt => opt.MapFrom(src => src.InstanciaEliminacionDirecta != null ? src.InstanciaEliminacionDirecta.Nombre : null))
            .ForMember(dest => dest.FaseTipoDeVueltaNombre, opt => opt.MapFrom(src => src.FaseTipoDeVuelta != null ? src.FaseTipoDeVuelta.Nombre : string.Empty))
            .ForMember(dest => dest.EstadoFaseNombre, opt => opt.MapFrom(src => src.EstadoFase != null ? src.EstadoFase.Estado : string.Empty))
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.Torneo, opt => opt.Ignore())
            .ForMember(dest => dest.FaseFormato, opt => opt.Ignore())
            .ForMember(dest => dest.InstanciaEliminacionDirecta, opt => opt.Ignore())
            .ForMember(dest => dest.FaseTipoDeVuelta, opt => opt.Ignore())
            .ForMember(dest => dest.EstadoFase, opt => opt.Ignore())
            .ForSourceMember(src => src.FaseFormatoNombre, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.InstanciaEliminacionDirectaNombre, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.FaseTipoDeVueltaNombre, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.EstadoFaseNombre, opt => opt.DoNotValidate());
        CreateMap<TorneoZona, TorneoZonaDTO>()
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.TorneoFase, opt => opt.Ignore())
            .ForMember(dest => dest.Equipos, opt => opt.Ignore())
            .ForMember(dest => dest.Fechas, opt => opt.Ignore());
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
            .ForMember(dest => dest.TorneoId, x => x.MapFrom(src => src.ZonaActual != null ? src.ZonaActual.TorneoFase!.TorneoId : (int?)null))
            .ForMember(dest => dest.TorneoNombre, x => x.MapFrom(src => src.ZonaActual != null && src.ZonaActual.TorneoFase != null && src.ZonaActual.TorneoFase.Torneo != null ? src.ZonaActual.TorneoFase.Torneo.Nombre : null))
            .ForMember(dest => dest.CodigoAlfanumerico,
                x => x.MapFrom(src => GeneradorDeHash.GenerarAlfanumerico7Digitos(src.Id)))
            .PreserveReferences();

        CreateMap<EquipoDTO, Equipo>()
            .ForMember(dest => dest.Club, opt => opt.Ignore())
            .ForMember(dest => dest.ZonaActual, opt => opt.Ignore())
            .ForMember(dest => dest.ZonaActualId, opt => opt.Ignore())
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
            .ForMember(dest => dest.Torneo, x => x.MapFrom(src => src.Equipo.ZonaActual != null && src.Equipo.ZonaActual.TorneoFase != null && src.Equipo.ZonaActual.TorneoFase.Torneo != null ? src.Equipo.ZonaActual.TorneoFase.Torneo.Nombre : ""))
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
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => src.ZonaActual != null && src.ZonaActual.TorneoFase != null && src.ZonaActual.TorneoFase.Torneo != null ? src.ZonaActual.TorneoFase.Torneo.Nombre : ""))
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
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => src.Equipo.ZonaActual != null && src.Equipo.ZonaActual.TorneoFase != null && src.Equipo.ZonaActual.TorneoFase.Torneo != null ? src.Equipo.ZonaActual.TorneoFase.Torneo.Nombre : ""));

        CreateMap<JugadorEquipo, CarnetDigitalPendienteDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Jugador.Id))
            .ForMember(dest => dest.DNI, opt => opt.MapFrom(src => src.Jugador.DNI))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Jugador.Nombre))
            .ForMember(dest => dest.Apellido, opt => opt.MapFrom(src => src.Jugador.Apellido))
            .ForMember(dest => dest.FechaNacimiento, opt => opt.MapFrom(src => src.Jugador.FechaNacimiento))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.EstadoJugadorId))
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => src.Equipo.ZonaActual != null && src.Equipo.ZonaActual.TorneoFase != null && src.Equipo.ZonaActual.TorneoFase.Torneo != null ? src.Equipo.ZonaActual.TorneoFase.Torneo.Nombre : ""))
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