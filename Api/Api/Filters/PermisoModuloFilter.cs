using Api.Api.Authorization;
using Api.Core.Enums;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Api.Filters;

public class PermisoModuloFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var moduloAttr = context.Controller.GetType().GetCustomAttributes(typeof(ModuloSistemaAttribute), true)
            .FirstOrDefault() as ModuloSistemaAttribute;

        if (moduloAttr == null)
        {
            await next();
            return;
        }

        var endpoint = context.ActionDescriptor.EndpointMetadata;
        if (endpoint.Any(m => m is AllowAnonymousAttribute))
        {
            await next();
            return;
        }

        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            await next();
            return;
        }

        var user = context.HttpContext.User;
        if (user.IsInRole("SuperAdministrador") || user.IsInRole("Delegado"))
        {
            await next();
            return;
        }

        var permisoServicio = context.HttpContext.RequestServices.GetRequiredService<IPermisoServicio>();
        var nivelRequerido = context.HttpContext.Request.Method == HttpMethods.Delete
            ? NivelAcceso.ControlTotal
            : NivelAcceso.Edicion;

        var autorizado = nivelRequerido == NivelAcceso.ControlTotal
            ? permisoServicio.PuedeEliminar(moduloAttr.Modulo)
            : permisoServicio.PuedeEditar(moduloAttr.Modulo);

        if (!autorizado)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
