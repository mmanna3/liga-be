using Microsoft.AspNetCore.Authorization;

namespace Api.Api.Authorization;

/// <summary>
/// Permite acceso solo a SuperAdministrador.
/// </summary>
public class AutorizarSoloSuperAdminAttribute : AuthorizeAttribute
{
    public AutorizarSoloSuperAdminAttribute()
    {
        Roles = "SuperAdministrador";
    }
}
