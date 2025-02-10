using Api.Core.DTOs;
using Api.Core.Entidades;
using AutoMapper;

namespace Api._Config;

public class MapperConfig : Profile
{
    public MapperConfig()
    {   

        CreateMap<CategoriaDeServicio, CategoriaDeServicioDTO>().PreserveReferences().ReverseMap();
        
        CreateMap<string, TimeOnly>().ConvertUsing(s => TimeOnly.Parse(s));
        CreateMap<TimeOnly, string>().ConvertUsing(t => t.ToString(Utilidades.FormatoHora));
        CreateMap<string, DateOnly>().ConvertUsing(s => DateOnly.Parse(s, Utilidades.CultureInfoAr));
    }
}