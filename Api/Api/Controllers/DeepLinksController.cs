using System.Net;
using Api._Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Api.Api.Controllers;

/// <summary>
/// Universal Links (iOS) y App Links (Android) del registro de árbitros.
/// Las rutas absolutas le ganan al fallback del SPA. Sin config devuelven 404.
/// </summary>
[ApiController]
[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public class DeepLinksController : ControllerBase
{
    private readonly DeepLinksOptions _opciones;

    public DeepLinksController(IOptionsSnapshot<DeepLinksOptions> opciones)
    {
        _opciones = opciones.Value;
    }

    [HttpGet("/.well-known/apple-app-site-association")]
    public IActionResult AppleAppSiteAssociation()
    {
        if (!_opciones.EstaConfigurado) return NotFound();

        return new JsonResult(new
        {
            applinks = new
            {
                details = new[]
                {
                    new
                    {
                        appIDs = new[] { $"{_opciones.AppleTeamId}.{_opciones.IosBundleId}" },
                        components = new[] { new Dictionary<string, string> { ["/"] = "/registro-arbitro" } }
                    }
                }
            }
        });
    }

    [HttpGet("/.well-known/assetlinks.json")]
    public IActionResult AssetLinks()
    {
        if (!_opciones.EstaConfigurado) return NotFound();

        return new JsonResult(new[]
        {
            new
            {
                relation = new[] { "delegate_permission/common.handle_all_urls" },
                target = new
                {
                    @namespace = "android_app",
                    package_name = _opciones.AndroidPackage,
                    sha256_cert_fingerprints = _opciones.AndroidSha256Fingerprints
                }
            }
        });
    }

    [HttpGet("/registro-arbitro")]
    public IActionResult RegistroArbitro()
    {
        if (!_opciones.EstaConfigurado) return NotFound();

        var userAgent = Request.Headers.UserAgent.ToString();

        if (userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("iPod", StringComparison.OrdinalIgnoreCase))
            return Redirect(_opciones.AppStoreUrl);

        if (userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase))
            return Redirect(_opciones.PlayStoreUrl);

        var appStore = WebUtility.HtmlEncode(_opciones.AppStoreUrl);
        var playStore = WebUtility.HtmlEncode(_opciones.PlayStoreUrl);
        var html = $"""
            <!DOCTYPE html>
            <html lang="es">
            <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>EDeFI</title>
            </head>
            <body style="font-family: sans-serif; text-align: center; margin: 2rem;">
            <p>Descargá la app de EDeFI para registrarte como árbitro</p>
            <p><a href="{appStore}">App Store (iPhone)</a></p>
            <p><a href="{playStore}">Google Play (Android)</a></p>
            </body>
            </html>
            """;
        return Content(html, "text/html; charset=utf-8");
    }
}
