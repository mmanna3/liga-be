using Microsoft.AspNetCore.Authorization;

namespace Api.Api.Authorization;

/// <summary>
/// Permite acceso solo a SuperAdministrador y Administrador.
/// </summary>
public class AutorizarSoloAdminAttribute : AuthorizeAttribute
{
    public AutorizarSoloAdminAttribute()
    {
        Roles = "SuperAdministrador,Administrador";
    }
}
