using Api.Core.Logica;
using Api.Core.Repositorios;
using Api.Core.Servicios;
using Api.Core.Servicios.Interfaces;
using Api.Persistencia._Config;
using Api.Persistencia.Repositorios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AutoMapper;

namespace Api._Config;

public static class InyeccionDeDependenciasConfig
{
    public static WebApplicationBuilder Configurar(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IBDVirtual, BDVirtual>();
        
        builder.Services.AddScoped<IClubRepo, ClubRepo>();
        builder.Services.AddScoped<IImagenEscudoRepo, ImagenEscudoRepo>();
        builder.Services.AddScoped<IClubCore, ClubCore>();
        
        builder.Services.AddScoped<IJugadorRepo, JugadorRepo>();
        builder.Services.AddScoped<IJugadorCore, JugadorCore>();
        
        builder.Services.AddScoped<IEquipoRepo, EquipoRepo>();
        builder.Services.AddScoped<IEquipoCore, EquipoCore>();
        
        builder.Services.AddScoped<ITorneoRepo, TorneoRepo>();
        builder.Services.AddScoped<ITorneoCore, TorneoCore>();

        builder.Services.AddScoped<ITorneoAgrupadorRepo, TorneoAgrupadorRepo>();
        builder.Services.AddScoped<ITorneoAgrupadorCore, TorneoAgrupadorCore>();

        builder.Services.AddScoped<ITorneoCategoriaRepo, TorneoCategoriaRepo>();
        builder.Services.AddScoped<ITorneoCategoriaCore, TorneoCategoriaCore>();

        builder.Services.AddScoped<ITorneoFaseRepo, TorneoFaseRepo>();
        builder.Services.AddScoped<ITorneoFaseCore, TorneoFaseCore>();

        builder.Services.AddScoped<ITorneoZonaRepo, TorneoZonaRepo>();
        builder.Services.AddScoped<ITorneoZonaCore, TorneoZonaCore>();

        builder.Services.AddScoped<ITorneoFechaRepo, TorneoFechaRepo>();
        builder.Services.AddScoped<ITorneoFechaCore, TorneoFechaCore>();

        builder.Services.AddScoped<IFixtureAlgoritmoRepo, FixtureAlgoritmoRepo>();
        builder.Services.AddScoped<IFixtureAlgoritmoCore, FixtureAlgoritmoCore>();
        
        builder.Services.AddScoped<IDelegadoRepo, DelegadoRepo>();
        builder.Services.AddScoped<IImagenDelegadoRepo, ImagenDelegadoRepo>();
        builder.Services.AddScoped<IUsuarioRepo, UsuarioRepo>();
        builder.Services.AddScoped<IDelegadoCore, DelegadoCore>();
        
        builder.Services.AddScoped<IHistorialDePagosRepo, HistorialDePagosRepo>();
        builder.Services.AddScoped<IReporteCore, ReporteCore>();

        builder.Services.AddScoped<IBackupCore, BackupCore>();
        builder.Services.AddScoped<IGoogleDriveCore, GoogleDriveCore>();
        
        builder.Services.AddScoped<IPublicoCore, PublicoCore>();
        
        builder.Services.AddScoped<IImagenJugadorRepo, ImagenJugadorRepo>();
        builder.Services.AddScoped<AppPaths, AppPathsWebApp>();
        
        // Registrar el servicio de autenticación
        builder.Services.AddScoped<IAuthService, AuthCore>();
        builder.Services.AddScoped<IAppCarnetDigitalCore, AppCarnetDigitalCore>();
        
        // Configurar la autenticación JWT
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Obtener la clave secreta de la configuración o usar una clave por defecto
                string claveSecreta = builder.Configuration.GetSection("AppSettings:Token").Value ?? "clave_secreta_por_defecto_para_desarrollo_con_longitud_suficiente_para_hmac_sha512";
                
                // Asegurar que la clave tenga al menos 64 bytes (512 bits) para HMAC-SHA512
                if (Encoding.UTF8.GetByteCount(claveSecreta) < 64)
                {
                    // Extender la clave hasta alcanzar al menos 64 bytes
                    claveSecreta = claveSecreta.PadRight(64, '_');
                }
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveSecreta)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                // Si la autenticación falla (token inválido/vencido) pero el endpoint tiene [AllowAnonymous],
                // permitir continuar como anónimo en lugar de devolver 401
                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var endpoint = context.HttpContext.GetEndpoint();
                        if (endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null)
                        {
                            context.NoResult();
                            return Task.CompletedTask;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        
        return builder;
    }
}