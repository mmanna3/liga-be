using Microsoft.AspNetCore.Authorization;

namespace Api.Api.Authorization;
public class AutorizarCualquierUsuarioAttribute : AuthorizeAttribute
{
    public AutorizarCualquierUsuarioAttribute()
    {
        Roles = "SuperAdministrador,Administrador,Usuario,Delegado";
    }
}
