using Api.Core.Logica;
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
        
        builder.Services.AddScoped<IClubRepo, ClubRepo>();
        builder.Services.AddScoped<IClubCore, ClubCore>();
        
        builder.Services.AddScoped<IJugadorRepo, JugadorRepo>();
        builder.Services.AddScoped<IJugadorCore, JugadorCore>();
        
        builder.Services.AddScoped<IEquipoRepo, EquipoRepo>();
        builder.Services.AddScoped<IEquipoCore, EquipoCore>();
        
        builder.Services.AddScoped<IDelegadoRepo, DelegadoRepo>();
        builder.Services.AddScoped<IDelegadoCore, DelegadoCore>();
        
        builder.Services.AddScoped<IPublicoCore, PublicoCore>();
        
        builder.Services.AddScoped<IImagenJugadorRepo, ImagenJugadorRepo>();
        builder.Services.AddScoped<AppPaths, AppPathsWebApp>();
        
        return builder;
    }
}