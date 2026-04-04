using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace Api.TestsDeIntegracion;

public class TorneoAgrupadorIT : TestBase
{
    public TorneoAgrupadorIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedData(context);
    }

    private static void SeedData(AppDbContext context)
    {
        // El seed "General" (Id=1) ya existe por HasData en AppDbContext
    }

    [Fact]
    public async Task ListarTorneoAgrupadores_Funciona()
    {
        var client = await GetAuthenticatedClient();

        var response = await client.GetAsync("/api/torneoagrupador");

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<TorneoAgrupadorDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        Assert.Contains(content, ta => ta.Nombre == "General");
    }

    [Fact]
    public async Task CrearTorneoAgrupador_DatosCorrectos_200()
    {
        var client = await GetAuthenticatedClient();

        var dto = new TorneoAgrupadorDTO
        {
            Nombre = "Torneos Locales",
            EsVisibleEnApp = true
        };

        var response = await client.PostAsJsonAsync("/api/torneoagrupador", dto);

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<TorneoAgrupadorDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.True(content.Id > 0);
        Assert.Equal("Torneos Locales", content.Nombre);
        Assert.True(content.EsVisibleEnApp);
    }

    [Fact]
    public async Task ObtenerTorneoAgrupador_PorId_DevuelveCorrecto()
    {
        var client = await GetAuthenticatedClient();

        var response = await client.GetAsync("/api/torneoagrupador/1");

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<TorneoAgrupadorDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(1, content.Id);
        Assert.Equal("General", content.Nombre);
    }

    [Fact]
    public async Task ModificarTorneoAgrupador_DatosCorrectos_204()
    {
        var client = await GetAuthenticatedClient();

        TorneoAgrupador agrupador;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            agrupador = new TorneoAgrupador { Id = 0, Nombre = "Para Modificar", EsVisibleEnApp = false };
            context.TorneoAgrupadores.Add(agrupador);
            context.SaveChanges();
        }

        var dto = new TorneoAgrupadorDTO
        {
            Id = agrupador.Id,
            Nombre = "Modificado",
            EsVisibleEnApp = true
        };

        var response = await client.PutAsJsonAsync($"/api/torneoagrupador/{agrupador.Id}", dto);

        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var actualizado = context.TorneoAgrupadores.Find(agrupador.Id);
            Assert.NotNull(actualizado);
            Assert.Equal("Modificado", actualizado.Nombre);
            Assert.True(actualizado.EsVisibleEnApp);
        }
    }

    [Fact]
    public async Task EliminarTorneoAgrupador_SinTorneos_200()
    {
        var client = await GetAuthenticatedClient();

        TorneoAgrupador agrupador;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            agrupador = new TorneoAgrupador { Id = 0, Nombre = "Para Eliminar", EsVisibleEnApp = false };
            context.TorneoAgrupadores.Add(agrupador);
            context.SaveChanges();
        }

        var response = await client.DeleteAsync($"/api/torneoagrupador/{agrupador.Id}");

        response.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Null(context.TorneoAgrupadores.Find(agrupador.Id));
        }
    }

    [Fact]
    public async Task EliminarTorneoAgrupador_ConTorneos_400()
    {
        var client = await GetAuthenticatedClient();

        TorneoAgrupador agrupador;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            agrupador = new TorneoAgrupador { Id = 0, Nombre = "Con Torneos", EsVisibleEnApp = true };
            context.TorneoAgrupadores.Add(agrupador);
            context.SaveChanges();

            var torneo = new Torneo { Id = 0, Nombre = "Torneo Test", Anio = 2026, TorneoAgrupadorId = agrupador.Id, EsVisibleEnApp = true };
            context.Torneos.Add(torneo);
            context.SaveChanges();
        }

        var response = await client.DeleteAsync($"/api/torneoagrupador/{agrupador.Id}");

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var mensaje = await response.Content.ReadAsStringAsync();
        Assert.Contains("torneos asociados", mensaje);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.NotNull(context.TorneoAgrupadores.Find(agrupador.Id));
        }
    }

    [Fact]
    public async Task ObtenerTorneoAgrupadores_PorIds_DevuelveSolicitados()
    {
        var client = await GetAuthenticatedClient();

        int id2;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var agrupador2 = new TorneoAgrupador { Id = 0, Nombre = "Segundo", EsVisibleEnApp = true };
            context.TorneoAgrupadores.Add(agrupador2);
            context.SaveChanges();
            id2 = agrupador2.Id;
        }

        var response = await client.GetAsync($"/api/torneoagrupador/por-ids?ids=1&ids={id2}");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<TorneoAgrupadorDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(2, content.Count);
        Assert.Contains(content, ta => ta.Id == 1 && ta.Nombre == "General");
        Assert.Contains(content, ta => ta.Id == id2 && ta.Nombre == "Segundo");
    }
}
