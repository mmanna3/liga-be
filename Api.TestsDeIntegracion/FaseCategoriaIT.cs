using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion;

public class FaseCategoriaIT : TestBase
{
    public FaseCategoriaIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedData(context);
    }

    private static void SeedData(AppDbContext context)
    {
    }

    private static async Task<(int TorneoId, int FaseId)> CrearTorneoConPlantillaYFase(
        CustomWebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var torneo = new Torneo
        {
            Id = 0,
            Nombre = "Torneo Fase Cat",
            Anio = 2026,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };
        context.Torneos.Add(torneo);
        await context.SaveChangesAsync();

        context.TorneoCategorias.AddRange(
            new TorneoCategoria
            {
                Id = 0, Nombre = "Sub-15", AnioDesde = 2010, AnioHasta = 2015, TorneoId = torneo.Id, Orden = 1
            },
            new TorneoCategoria
            {
                Id = 0, Nombre = "Sub-18", AnioDesde = 2007, AnioHasta = 2009, TorneoId = torneo.Id, Orden = 2
            });

        var fase = new FaseTodosContraTodos
        {
            Id = 0,
            Nombre = "Fase 1",
            Numero = 1,
            TorneoId = torneo.Id,
            EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
            EsVisibleEnApp = true
        };
        context.Fases.Add(fase);
        await context.SaveChangesAsync();

        return (torneo.Id, fase.Id);
    }

    [Fact]
    public async Task CrearFase_SinCategoriasEnPayload_CopiaPlantillaDelTorneo()
    {
        var (torneoId, _) = await CrearTorneoConPlantillaYFase(Factory);
        var client = await GetAuthenticatedClient();

        var response = await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/fases", new FaseDTO
        {
            Nombre = "Segunda fase",
            Numero = 2,
            TipoDeFase = TipoDeFaseEnum.TodosContraTodos,
            EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
            EsVisibleEnApp = true,
            TorneoId = torneoId
        });

        response.EnsureSuccessStatusCode();
        var faseCreada = JsonConvert.DeserializeObject<FaseDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(faseCreada);

        var catsResponse = await client.GetAsync($"/api/Torneo/{torneoId}/fases/{faseCreada.Id}/categorias");
        catsResponse.EnsureSuccessStatusCode();
        var cats = JsonConvert.DeserializeObject<List<FaseCategoriaDTO>>(await catsResponse.Content.ReadAsStringAsync());
        Assert.NotNull(cats);
        Assert.Equal(2, cats.Count);
        Assert.Contains(cats, c => c.Nombre == "Sub-15");
        Assert.Contains(cats, c => c.Nombre == "Sub-18");
    }

    [Fact]
    public async Task CrearFase_ConCategoriasPersonalizadas_PersisteSoloLasIndicadas()
    {
        var (torneoId, _) = await CrearTorneoConPlantillaYFase(Factory);
        var client = await GetAuthenticatedClient();

        var response = await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/fases", new FaseDTO
        {
            Nombre = "Fase custom",
            Numero = 2,
            TipoDeFase = TipoDeFaseEnum.TodosContraTodos,
            EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
            EsVisibleEnApp = true,
            TorneoId = torneoId,
            Categorias =
            [
                new FaseCategoriaDTO { Nombre = "Única", AnioDesde = 2012, AnioHasta = 2013, Orden = 1 }
            ]
        });

        response.EnsureSuccessStatusCode();
        var faseCreada = JsonConvert.DeserializeObject<FaseDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(faseCreada);

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var cats = await context.FaseCategorias.Where(c => c.FaseId == faseCreada.Id).ToListAsync();
        Assert.Single(cats);
        Assert.Equal("Única", cats[0].Nombre);
    }

    [Fact]
    public async Task ModificarFase_ReemplazarCategorias_ActualizaListado()
    {
        var (torneoId, faseId) = await CrearTorneoConPlantillaYFase(Factory);
        var client = await GetAuthenticatedClient();

        await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/fases/{faseId}/categorias", new FaseCategoriaDTO
        {
            Nombre = "Cat A", AnioDesde = 2010, AnioHasta = 2011, Orden = 1
        });

        var putResponse = await client.PutAsJsonAsync($"/api/Torneo/{torneoId}/fases/{faseId}", new FaseDTO
        {
            Id = faseId,
            Nombre = "Fase 1",
            Numero = 1,
            TipoDeFase = TipoDeFaseEnum.TodosContraTodos,
            EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
            EsVisibleEnApp = true,
            TorneoId = torneoId,
            Categorias =
            [
                new FaseCategoriaDTO { Nombre = "Nueva", AnioDesde = 2014, AnioHasta = 2015, Orden = 1 }
            ]
        });
        putResponse.EnsureSuccessStatusCode();

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var cats = await context.FaseCategorias.Where(c => c.FaseId == faseId).ToListAsync();
        Assert.Single(cats);
        Assert.Equal("Nueva", cats[0].Nombre);
    }

    [Fact]
    public async Task EliminarPlantillaTorneo_NoRompeCategoriasDeFase()
    {
        var (torneoId, faseId) = await CrearTorneoConPlantillaYFase(Factory);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.FaseCategorias.Add(new FaseCategoria
            {
                Id = 0,
                Nombre = "Copiada",
                AnioDesde = 2010,
                AnioHasta = 2011,
                FaseId = faseId,
                Orden = 1
            });
            context.TorneoCategorias.RemoveRange(context.TorneoCategorias.Where(c => c.TorneoId == torneoId));
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var catsResponse = await client.GetAsync($"/api/Torneo/{torneoId}/fases/{faseId}/categorias");
        catsResponse.EnsureSuccessStatusCode();
        var cats = JsonConvert.DeserializeObject<List<FaseCategoriaDTO>>(await catsResponse.Content.ReadAsStringAsync());
        Assert.NotNull(cats);
        Assert.Single(cats);
        Assert.Equal("Copiada", cats[0].Nombre);
    }
}
