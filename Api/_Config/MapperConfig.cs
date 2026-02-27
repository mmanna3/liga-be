using System.Globalization;
using Api.Core.DTOs;
using Api.Core.Entidades;
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
            // .ForMember(dest => dest.Equipos, opt => opt.MapFrom(src => src.Equipos))
            // .ForMember(dest => dest.Delegados, opt => opt.MapFrom(src => src.Delegados))
            .PreserveReferences().ReverseMap();
        CreateMap<Torneo, TorneoDTO>().PreserveReferences().ReverseMap();

        CreateMap<Equipo, EquipoDTO>()
            .ForMember(dest => dest.ClubNombre, x => x.MapFrom(src => src.Club.Nombre))
            .ForMember(dest => dest.TorneoNombre, x => x.MapFrom(src => src.Torneo != null ? src.Torneo.Nombre : null))
            .ForMember(dest => dest.CodigoAlfanumerico,
                x => x.MapFrom(src => GeneradorDeHash.GenerarAlfanumerico7Digitos(src.Id)))
            .PreserveReferences();

        CreateMap<EquipoDTO, Equipo>()
            .ForMember(dest => dest.Club, opt => opt.Ignore())
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
            .ForMember(dest => dest.Nombre, x => x.MapFrom(src => src.Equipo.Nombre))
            .ForMember(dest => dest.Club, x => x.MapFrom(src => src.Equipo.Club.Nombre))
            .ForMember(dest => dest.Torneo, x => x.MapFrom(src => src.Equipo.Torneo.Nombre))
            .ForMember(dest => dest.Estado, x => x.MapFrom(src => src.EstadoJugador.Id))
            .ForMember(dest => dest.FechaPagoDeFichaje, opt => opt.MapFrom<FechaPagoResolver>())
            .ForMember(dest => dest.Motivo, x => x.MapFrom(src => src.Motivo))
            .PreserveReferences().ReverseMap();

        CreateMap<Delegado, DelegadoDTO>()
            .ForMember(dest => dest.NombreUsuario, x => x.MapFrom(src => src.Usuario != null ? src.Usuario.NombreUsuario : null))
            .ForMember(dest => dest.BlanqueoPendiente, x => x.MapFrom(src => src.Usuario != null && src.Usuario.Password == null))
            .ForMember(dest => dest.EstadoDelegado, x => x.MapFrom(src => src.EstadoDelegado))
            .ForMember(dest => dest.ClubNombre, x => x.MapFrom(src => src.Club.Nombre))
            .ForMember(dest => dest.EquiposDelClub, x => x.MapFrom(src => src.Club.Equipos.Select(e => e.Nombre).ToList()))
            .PreserveReferences()
            .ReverseMap()
            .ForMember(dest => dest.EstadoDelegado, opt => opt.Ignore())
            .ForMember(dest => dest.EstadoDelegadoId, opt => opt.Ignore())
            .ForMember(dest => dest.Club, opt => opt.Ignore());
        
        
        CreateMap<Jugador, JugadorDTO>()
            .ForMember(dest => dest.Equipos, x => x.MapFrom(src => src.JugadorEquipos))
            .PreserveReferences().ReverseMap();
        
        CreateMap<JugadorEquipo, JugadorEquipoDTO>().PreserveReferences().ReverseMap();
        CreateMap<EstadoJugador, EstadoJugadorDTO>().PreserveReferences().ReverseMap();
        CreateMap<EstadoDelegado, EstadoDelegadoDTO>().PreserveReferences();
        
        CreateMap<JugadorEquipo, EquipoDTO>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Equipo.Nombre))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Equipo.Id));

        CreateMap<Equipo, EquipoBaseDTO>()
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => src.Torneo!.Nombre))
            .ForMember(dest => dest.CodigoAlfanumerico, opt => opt.MapFrom(src => GeneradorDeHash.GenerarAlfanumerico7Digitos(src.Id)));

        CreateMap<JugadorEquipo, CarnetDigitalDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Jugador.Id))
            .ForMember(dest => dest.DNI, opt => opt.MapFrom(src => src.Jugador.DNI))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Jugador.Nombre))
            .ForMember(dest => dest.Apellido, opt => opt.MapFrom(src => src.Jugador.Apellido))
            .ForMember(dest => dest.FechaNacimiento, opt => opt.MapFrom(src => src.Jugador.FechaNacimiento))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.EstadoJugadorId))
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => src.Equipo.Torneo!.Nombre));

        CreateMap<JugadorEquipo, CarnetDigitalPendienteDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Jugador.Id))
            .ForMember(dest => dest.DNI, opt => opt.MapFrom(src => src.Jugador.DNI))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Jugador.Nombre))
            .ForMember(dest => dest.Apellido, opt => opt.MapFrom(src => src.Jugador.Apellido))
            .ForMember(dest => dest.FechaNacimiento, opt => opt.MapFrom(src => src.Jugador.FechaNacimiento))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.EstadoJugadorId))
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => src.Equipo.Torneo!.Nombre))
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