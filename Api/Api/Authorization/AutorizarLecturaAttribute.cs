using Microsoft.AspNetCore.Authorization;

namespace Api.Api.Authorization;

/// <summary>
/// Permite acceso de lectura a SuperAdministrador y Administrador.
/// </summary>
public class AutorizarLecturaAttribute : AuthorizeAttribute
{
    public AutorizarLecturaAttribute()
    {
        Roles = "SuperAdministrador,Administrador";
    }
}
