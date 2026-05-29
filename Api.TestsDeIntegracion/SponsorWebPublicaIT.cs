using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Logica;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;

namespace Api.TestsDeIntegracion;

public class SponsorWebPublicaIT : TestBase
{
    private const string PuntoRojoBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";

    public SponsorWebPublicaIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var paths = scope.ServiceProvider.GetRequiredService<AppPaths>();
        if (Directory.Exists(paths.ImagenesSponsorsAbsolute))
        {
            foreach (var archivo in Directory.GetFiles(paths.ImagenesSponsorsAbsolute))
                File.Delete(archivo);
        }
    }

    [Fact]
    public async Task PanelAdmin_ListarCrearYEliminar_PersisteEnCarpetaSponsors()
    {
        var client = await GetAuthenticatedClient();

        using var scope = Factory.Services.CreateScope();
        var paths = scope.ServiceProvider.GetRequiredService<AppPaths>();

        var listaInicial = await client.GetFromJsonAsync<List<SponsorWebPublicaDTO>>("/api/SponsorWebPublica");
        Assert.NotNull(listaInicial);
        Assert.Empty(listaInicial);

        var crearDto = new CrearSponsorWebPublicaDTO
        {
            Nombre = "Sponsor de prueba",
            ImagenBase64 = PuntoRojoBase64
        };
        var crearResponse = await client.PostAsJsonAsync("/api/SponsorWebPublica", crearDto);
        crearResponse.EnsureSuccessStatusCode();
        var creado = await crearResponse.Content.ReadFromJsonAsync<SponsorWebPublicaDTO>();
        Assert.NotNull(creado);
        Assert.True(creado.Id > 0);
        Assert.Equal("Sponsor de prueba", creado.Nombre);
        Assert.False(string.IsNullOrEmpty(creado.Imagen));

        var pathImagen = Path.Combine(paths.ImagenesSponsorsAbsolute, $"{creado.Id}.jpg");
        Assert.True(File.Exists(pathImagen));

        var listaConSponsor = await client.GetFromJsonAsync<List<SponsorWebPublicaDTO>>("/api/SponsorWebPublica");
        Assert.NotNull(listaConSponsor);
        Assert.Single(listaConSponsor);
        Assert.Equal(creado.Id, listaConSponsor[0].Id);
        Assert.False(string.IsNullOrEmpty(listaConSponsor[0].Imagen));

        var porId = await client.GetFromJsonAsync<SponsorWebPublicaDTO>($"/api/SponsorWebPublica/{creado.Id}");
        Assert.NotNull(porId);
        Assert.Equal(creado.Nombre, porId.Nombre);
        Assert.False(string.IsNullOrEmpty(porId.Imagen));

        var eliminarResponse = await client.DeleteAsync($"/api/SponsorWebPublica/{creado.Id}");
        eliminarResponse.EnsureSuccessStatusCode();
        Assert.False(File.Exists(pathImagen));

        var listaFinal = await client.GetFromJsonAsync<List<SponsorWebPublicaDTO>>("/api/SponsorWebPublica");
        Assert.NotNull(listaFinal);
        Assert.Empty(listaFinal);
    }
}
