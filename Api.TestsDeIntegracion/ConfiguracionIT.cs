using System.Net;
using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Logica;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;

namespace Api.TestsDeIntegracion;

public class ConfiguracionIT : TestBase
{
    public ConfiguracionIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task CambiarEscudoPorDefecto_ConDataUriValida_GuardaJpegEnPorDefecto()
    {
        const string dataUriJpeg =
            "data:image/jpeg;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";

        var client = await GetAuthenticatedClient();
        var response = await client.PutAsJsonAsync(
            "/api/configuracion/cambiar-escudo-por-defecto",
            new CambiarEscudoPorDefectoDTO { Escudo = dataUriJpeg });

        response.EnsureSuccessStatusCode();
        var ok = await response.Content.ReadFromJsonAsync<bool>();
        Assert.True(ok);

        using var scope = Factory.Services.CreateScope();
        var paths = scope.ServiceProvider.GetRequiredService<AppPaths>();
        Assert.True(File.Exists(paths.EscudoDefaultFileAbsolute));
        var bytes = await File.ReadAllBytesAsync(paths.EscudoDefaultFileAbsolute);
        Assert.True(bytes.Length >= 3);
        Assert.Equal(0xFF, bytes[0]);
        Assert.Equal(0xD8, bytes[1]);
        Assert.Equal(0xFF, bytes[2]);
    }

    [Fact]
    public async Task CambiarEscudoPorDefecto_ConImagenInvalida_Devuelve400()
    {
        var client = await GetAuthenticatedClient();
        var response = await client.PutAsJsonAsync(
            "/api/configuracion/cambiar-escudo-por-defecto",
            new CambiarEscudoPorDefectoDTO { Escudo = "data:image/jpeg;base64,!!!" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
