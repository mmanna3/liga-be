using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace Api.TestsDeIntegracion;

public class TorneoCategoriaIT : TestBase
{
    public TorneoCategoriaIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedData(context);
    }

    private static void SeedData(AppDbContext context)
    {
        // TorneoAgrupador "General" (Id=1) ya existe por HasData en AppDbContext
    }

    private static async Task<int> CrearTorneoDePrueba(CustomWebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var torneo = new Torneo { Id = 0, Nombre = "Torneo Test Categorias", Anio = 2026, TorneoAgrupadorId = 1, EsVisibleEnApp = true, SeVenLosGolesEnTablaDePosiciones = true };
        context.Torneos.Add(torneo);
        await context.SaveChangesAsync();
        return torneo.Id;
    }

    [Fact]
    public async Task ListarCategorias_TorneoExistente_DevuelveLista()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var response = await client.GetAsync($"/api/Torneo/{torneoId}/categorias");

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<TorneoCategoriaDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Empty(content);
    }

    [Fact]
    public async Task CrearCategoria_DatosCorrectos_200()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var dto = new TorneoCategoriaDTO
        {
            Nombre = "Sub-15",
            AnioDesde = 2010,
            AnioHasta = 2015
        };

        var response = await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/categorias", dto);

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<TorneoCategoriaDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.True(content.Id > 0);
        Assert.Equal("Sub-15", content.Nombre);
        Assert.Equal(2010, content.AnioDesde);
        Assert.Equal(2015, content.AnioHasta);
        Assert.Equal(torneoId, content.TorneoId);
    }

    [Fact]
    public async Task CrearCategoria_TorneoInexistente_400()
    {
        var client = await GetAuthenticatedClient();

        var dto = new TorneoCategoriaDTO
        {
            Nombre = "Sub-15",
            AnioDesde = 2010,
            AnioHasta = 2015
        };

        var response = await client.PostAsJsonAsync("/api/Torneo/99999/categorias", dto);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var mensaje = await response.Content.ReadAsStringAsync();
        Assert.Contains("torneo", mensaje.ToLowerInvariant());
    }

    [Fact]
    public async Task CrearCategoria_AnioDesdeMayorQueAnioHasta_400()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var dto = new TorneoCategoriaDTO
        {
            Nombre = "Sub-15",
            AnioDesde = 2015,
            AnioHasta = 2010
        };

        var response = await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/categorias", dto);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var mensaje = await response.Content.ReadAsStringAsync();
        Assert.Contains("año", mensaje.ToLowerInvariant());
    }

    [Fact]
    public async Task ObtenerCategoria_PorId_DevuelveCorrecto()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        TorneoCategoria categoria;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            categoria = new TorneoCategoria
            {
                Id = 0,
                Nombre = "Sub-18",
                AnioDesde = 2007,
                AnioHasta = 2009,
                TorneoId = torneoId
            };
            context.TorneoCategorias.Add(categoria);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/Torneo/{torneoId}/categorias/{categoria.Id}");

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<TorneoCategoriaDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(categoria.Id, content.Id);
        Assert.Equal("Sub-18", content.Nombre);
        Assert.Equal(2007, content.AnioDesde);
        Assert.Equal(2009, content.AnioHasta);
    }

    [Fact]
    public async Task ObtenerCategoria_CategoriaDeOtroTorneo_404()
    {
        var torneo1Id = await CrearTorneoDePrueba(Factory);
        int categoriaId;
        int torneo2Id;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var torneo2 = new Torneo { Id = 0, Nombre = "Otro Torneo", Anio = 2026, TorneoAgrupadorId = 1, EsVisibleEnApp = true, SeVenLosGolesEnTablaDePosiciones = true };
            context.Torneos.Add(torneo2);
            await context.SaveChangesAsync();
            torneo2Id = torneo2.Id;

            var categoria = new TorneoCategoria
            {
                Id = 0,
                Nombre = "Sub-18",
                AnioDesde = 2007,
                AnioHasta = 2009,
                TorneoId = torneo2Id
            };
            context.TorneoCategorias.Add(categoria);
            await context.SaveChangesAsync();
            categoriaId = categoria.Id;
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/Torneo/{torneo1Id}/categorias/{categoriaId}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ModificarCategoria_DatosCorrectos_204()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        TorneoCategoria categoria;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            categoria = new TorneoCategoria
            {
                Id = 0,
                Nombre = "Para Modificar",
                AnioDesde = 2010,
                AnioHasta = 2012,
                TorneoId = torneoId
            };
            context.TorneoCategorias.Add(categoria);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var dto = new TorneoCategoriaDTO
        {
            Id = categoria.Id,
            Nombre = "Modificado",
            AnioDesde = 2011,
            AnioHasta = 2013,
            TorneoId = torneoId
        };

        var response = await client.PutAsJsonAsync($"/api/Torneo/{torneoId}/categorias/{categoria.Id}", dto);

        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var actualizado = context.TorneoCategorias.Find(categoria.Id);
            Assert.NotNull(actualizado);
            Assert.Equal("Modificado", actualizado.Nombre);
            Assert.Equal(2011, actualizado.AnioDesde);
            Assert.Equal(2013, actualizado.AnioHasta);
        }
    }

    [Fact]
    public async Task ModificarCategoria_AnioDesdeMayorQueAnioHasta_400()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        TorneoCategoria categoria;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            categoria = new TorneoCategoria
            {
                Id = 0,
                Nombre = "Para Modificar",
                AnioDesde = 2010,
                AnioHasta = 2012,
                TorneoId = torneoId
            };
            context.TorneoCategorias.Add(categoria);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var dto = new TorneoCategoriaDTO
        {
            Id = categoria.Id,
            Nombre = "Modificado",
            AnioDesde = 2015,
            AnioHasta = 2010,
            TorneoId = torneoId
        };

        var response = await client.PutAsJsonAsync($"/api/Torneo/{torneoId}/categorias/{categoria.Id}", dto);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EliminarCategoria_Existente_200()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        TorneoCategoria categoria;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            categoria = new TorneoCategoria
            {
                Id = 0,
                Nombre = "Para Eliminar",
                AnioDesde = 2010,
                AnioHasta = 2012,
                TorneoId = torneoId
            };
            context.TorneoCategorias.Add(categoria);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var response = await client.DeleteAsync($"/api/Torneo/{torneoId}/categorias/{categoria.Id}");

        response.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Null(context.TorneoCategorias.Find(categoria.Id));
        }
    }

    [Fact]
    public async Task EliminarCategoria_CategoriaDeOtroTorneo_404()
    {
        var torneo1Id = await CrearTorneoDePrueba(Factory);
        int categoriaId;
        int torneo2Id;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var torneo2 = new Torneo { Id = 0, Nombre = "Otro Torneo", Anio = 2026, TorneoAgrupadorId = 1, EsVisibleEnApp = true, SeVenLosGolesEnTablaDePosiciones = true };
            context.Torneos.Add(torneo2);
            await context.SaveChangesAsync();
            torneo2Id = torneo2.Id;

            var categoria = new TorneoCategoria
            {
                Id = 0,
                Nombre = "Sub-18",
                AnioDesde = 2007,
                AnioHasta = 2009,
                TorneoId = torneo2Id
            };
            context.TorneoCategorias.Add(categoria);
            await context.SaveChangesAsync();
            categoriaId = categoria.Id;
        }

        var client = await GetAuthenticatedClient();
        var response = await client.DeleteAsync($"/api/Torneo/{torneo1Id}/categorias/{categoriaId}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.NotNull(context.TorneoCategorias.Find(categoriaId));
        }
    }

    [Fact]
    public async Task ListarCategorias_ConCategoriasCreadas_DevuelveTodasDelTorneo()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var dto1 = new TorneoCategoriaDTO { Nombre = "Sub-15", AnioDesde = 2010, AnioHasta = 2015 };
        var dto2 = new TorneoCategoriaDTO { Nombre = "Sub-18", AnioDesde = 2007, AnioHasta = 2009 };

        await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/categorias", dto1);
        await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/categorias", dto2);

        var response = await client.GetAsync($"/api/Torneo/{torneoId}/categorias");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<TorneoCategoriaDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(2, content.Count);
        Assert.Contains(content, c => c.Nombre == "Sub-15");
        Assert.Contains(content, c => c.Nombre == "Sub-18");
    }
}
