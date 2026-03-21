using Microsoft.AspNetCore.Authorization;

namespace Api.Api.Authorization;

/// <summary>
/// Permite acceso a SuperAdministrador, Administrador y Usuario.
/// </summary>
public class AutorizarCualquierUsuarioAdministrativoAttribute : AuthorizeAttribute
{
    public AutorizarCualquierUsuarioAdministrativoAttribute()
    {
        Roles = "SuperAdministrador,Administrador,Usuario";
    }
}
