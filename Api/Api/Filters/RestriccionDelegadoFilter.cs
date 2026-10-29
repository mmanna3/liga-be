using Api.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Api.Filters;

/// <summary>
/// Un delegado solo puede llamar endpoints anónimos o marcados con [PermitirDelegado].
/// </summary>
public class RestriccionDelegadoFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.User.IsInRole("Delegado"))
            return;

        var metadata = context.ActionDescriptor.EndpointMetadata;
        if (metadata.Any(m => m is AllowAnonymousAttribute or PermitirDelegadoAttribute))
            return;

        context.Result = new ForbidResult();
    }
}
