using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Api.Authorization;

public class AutorizarConApiKeyAttribute : Attribute, IActionFilter
{
    private const string Header = "X-Api-Key";

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var apiKey = config["AppSettings:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            context.Result = new ObjectResult("ApiKey no configurado en el servidor.") { StatusCode = 500 };
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(Header, out var valorRecibido) || valorRecibido != apiKey)
            context.Result = new UnauthorizedResult();
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
