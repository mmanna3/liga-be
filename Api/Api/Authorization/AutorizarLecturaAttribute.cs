using Microsoft.AspNetCore.Authorization;

namespace Api.Api.Authorization;

/// <summary>
/// Permite acceso de lectura a SuperAdministrador, Administrador y Consulta.
/// </summary>
public class AutorizarLecturaAttribute : AuthorizeAttribute
{
    public AutorizarLecturaAttribute()
    {
        Roles = "SuperAdministrador,Administrador,Consulta";
    }
}
