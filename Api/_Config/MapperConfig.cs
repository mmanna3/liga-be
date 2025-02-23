using Api.Core.DTOs;
using Api.Core.Entidades;
using AutoMapper;

namespace Api._Config;

public class MapperConfig : Profile
{
    public MapperConfig()
    {   
        CreateMap<Club, ClubDTO>().PreserveReferences().ReverseMap();
        CreateMap<Equipo, EquipoDTO>().PreserveReferences().ReverseMap();
        CreateMap<Delegado, DelegadoDTO>().PreserveReferences().ReverseMap();
        CreateMap<Jugador, JugadorDTO>().PreserveReferences().ReverseMap();
        CreateMap<JugadorEquipo, JugadorEquipoDTO>().PreserveReferences().ReverseMap();
        CreateMap<EstadoJugadorDTO, EstadoJugadorDTO>().PreserveReferences().ReverseMap();
        
        CreateMap<string, TimeOnly>().ConvertUsing(s => TimeOnly.Parse(s));
        CreateMap<TimeOnly, string>().ConvertUsing(t => t.ToString(Utilidades.FormatoHora));
        CreateMap<string, DateOnly>().ConvertUsing(s => DateOnly.Parse(s, Utilidades.CultureInfoAr));
    }
}