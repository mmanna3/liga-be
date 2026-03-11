using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace Api.TestsDeIntegracion;

public class TorneoZonaIT : TestBase
{
    public TorneoZonaIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedData(context);
    }

    private static void SeedData(AppDbContext context)
    {
    }

    private static async Task<int> CrearTorneoFaseDePrueba(CustomWebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var torneo = new Torneo { Id = 0, Nombre = "Torneo Test Zonas", Anio = 2026, TorneoAgrupadorId = 1 };
        context.Torneos.Add(torneo);
        await context.SaveChangesAsync();

        var fase = new TorneoFase
        {
            Id = 0,
            Numero = 1,
            TorneoId = torneo.Id,
            FaseFormatoId = 1,
            EstadoFaseId = 100,
            EsVisibleEnApp = true
        };
        context.TorneoFases.Add(fase);
        await context.SaveChangesAsync();
        return fase.Id;
    }

    [Fact]
    public async Task ListarZonas_FaseExistente_DevuelveLista()
    {
        var faseId = await CrearTorneoFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var response = await client.GetAsync($"/api/TorneoFase/{faseId}/zonas");

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<TorneoZonaDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Empty(content);
    }

    [Fact]
    public async Task CrearZona_DatosCorrectos_200()
    {
        var faseId = await CrearTorneoFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var dto = new TorneoZonaDTO { Nombre = "Zona A" };

        var response = await client.PostAsJsonAsync($"/api/TorneoFase/{faseId}/zonas", dto);

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<TorneoZonaDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.True(content.Id > 0);
        Assert.Equal("Zona A", content.Nombre);
        Assert.Equal(faseId, content.TorneoFaseId);
    }

    [Fact]
    public async Task CrearZona_FaseInexistente_400()
    {
        var client = await GetAuthenticatedClient();

        var dto = new TorneoZonaDTO { Nombre = "Zona A" };

        var response = await client.PostAsJsonAsync("/api/TorneoFase/99999/zonas", dto);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var mensaje = await response.Content.ReadAsStringAsync();
        Assert.Contains("fase", mensaje.ToLowerInvariant());
    }

    [Fact]
    public async Task ObtenerZona_PorId_DevuelveCorrecto()
    {
        var faseId = await CrearTorneoFaseDePrueba(Factory);
        TorneoZona zona;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            zona = new TorneoZona
            {
                Id = 0,
                Nombre = "Zona B",
                TorneoFaseId = faseId
            };
            context.TorneoZonas.Add(zona);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/TorneoFase/{faseId}/zonas/{zona.Id}");

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<TorneoZonaDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(zona.Id, content.Id);
        Assert.Equal("Zona B", content.Nombre);
    }

    [Fact]
    public async Task ObtenerZona_ZonaDeOtraFase_404()
    {
        var fase1Id = await CrearTorneoFaseDePrueba(Factory);
        int zonaId;
        int fase2Id;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var torneo2 = new Torneo { Id = 0, Nombre = "Otro Torneo", Anio = 2026, TorneoAgrupadorId = 1 };
            context.Torneos.Add(torneo2);
            await context.SaveChangesAsync();

            var fase2 = new TorneoFase
            {
                Id = 0,
                Numero = 1,
                TorneoId = torneo2.Id,
                FaseFormatoId = 1,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            context.TorneoFases.Add(fase2);
            await context.SaveChangesAsync();
            fase2Id = fase2.Id;

            var zona = new TorneoZona
            {
                Id = 0,
                Nombre = "Zona B",
                TorneoFaseId = fase2Id
            };
            context.TorneoZonas.Add(zona);
            await context.SaveChangesAsync();
            zonaId = zona.Id;
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/TorneoFase/{fase1Id}/zonas/{zonaId}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ModificarZona_DatosCorrectos_204()
    {
        var faseId = await CrearTorneoFaseDePrueba(Factory);
        TorneoZona zona;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            zona = new TorneoZona
            {
                Id = 0,
                Nombre = "Para Modificar",
                TorneoFaseId = faseId
            };
            context.TorneoZonas.Add(zona);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var dto = new TorneoZonaDTO
        {
            Id = zona.Id,
            Nombre = "Zona Modificada",
            TorneoFaseId = faseId
        };

        var response = await client.PutAsJsonAsync($"/api/TorneoFase/{faseId}/zonas/{zona.Id}", dto);

        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var actualizado = context.TorneoZonas.Find(zona.Id);
            Assert.NotNull(actualizado);
            Assert.Equal("Zona Modificada", actualizado.Nombre);
        }
    }

    [Fact]
    public async Task EliminarZona_Existente_200()
    {
        var faseId = await CrearTorneoFaseDePrueba(Factory);
        TorneoZona zona;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            zona = new TorneoZona
            {
                Id = 0,
                Nombre = "Para Eliminar",
                TorneoFaseId = faseId
            };
            context.TorneoZonas.Add(zona);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var response = await client.DeleteAsync($"/api/TorneoFase/{faseId}/zonas/{zona.Id}");

        response.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Null(context.TorneoZonas.Find(zona.Id));
        }
    }

    [Fact]
    public async Task EliminarZona_ZonaDeOtraFase_404()
    {
        var fase1Id = await CrearTorneoFaseDePrueba(Factory);
        int zonaId;
        int fase2Id;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var torneo2 = new Torneo { Id = 0, Nombre = "Otro Torneo", Anio = 2026, TorneoAgrupadorId = 1 };
            context.Torneos.Add(torneo2);
            await context.SaveChangesAsync();

            var fase2 = new TorneoFase
            {
                Id = 0,
                Numero = 1,
                TorneoId = torneo2.Id,
                FaseFormatoId = 1,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            context.TorneoFases.Add(fase2);
            await context.SaveChangesAsync();
            fase2Id = fase2.Id;

            var zona = new TorneoZona
            {
                Id = 0,
                Nombre = "Zona B",
                TorneoFaseId = fase2Id
            };
            context.TorneoZonas.Add(zona);
            await context.SaveChangesAsync();
            zonaId = zona.Id;
        }

        var client = await GetAuthenticatedClient();
        var response = await client.DeleteAsync($"/api/TorneoFase/{fase1Id}/zonas/{zonaId}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.NotNull(context.TorneoZonas.Find(zonaId));
        }
    }

    [Fact]
    public async Task ListarZonas_ConZonasCreadas_DevuelveTodasDeLaFase()
    {
        var faseId = await CrearTorneoFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        await client.PostAsJsonAsync($"/api/TorneoFase/{faseId}/zonas", new TorneoZonaDTO { Nombre = "Zona A" });
        await client.PostAsJsonAsync($"/api/TorneoFase/{faseId}/zonas", new TorneoZonaDTO { Nombre = "Zona B" });

        var response = await client.GetAsync($"/api/TorneoFase/{faseId}/zonas");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<TorneoZonaDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(2, content.Count);
        Assert.Contains(content, z => z.Nombre == "Zona A");
        Assert.Contains(content, z => z.Nombre == "Zona B");
    }
}
