using Api.Core.Logica;
using Api.Core.Repositorios;
using Api.Core.Servicios;
using Api.Core.Servicios.Interfaces;
using Api.Persistencia._Config;
using Api.Persistencia.Repositorios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
        
        // Registrar el servicio de autenticación
        builder.Services.AddScoped<IAuthService, AuthService>();
        
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
            });
        
        return builder;
    }
}