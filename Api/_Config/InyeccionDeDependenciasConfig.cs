using Api.Core.Repositorios;
using Api.Core.Servicios;
using Api.Core.Servicios.Interfaces;
using Api.Persistencia._Config;
using Api.Persistencia.Repositorios;

namespace Api._Config;

public static class InyeccionDeDependenciasConfig
{
    public static WebApplicationBuilder Configurar(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IBDVirtual, BDVirtual>();
        builder.Services.AddScoped<ICategoriaDeServicioRepo, CategoriaDeServicioRepo>();
        builder.Services.AddScoped<ICategoriaDeServicioCore, CategoriaDeServicioCore>();
        
        return builder;
    }
}