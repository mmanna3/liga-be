using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion;

public class GrupoDeFasesIT : TestBase
{
    public GrupoDeFasesIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private static async Task<int> CrearTorneo(CustomWebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var torneo = new Torneo
        {
            Id = 0,
            Nombre = "Torneo Grupos",
            Anio = 2026,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };
        ctx.Torneos.Add(torneo);
        await ctx.SaveChangesAsync();
        return torneo.Id;
    }

    [Fact]
    public async Task CrearGrupo_Raiz_200()
    {
        var torneoId = await CrearTorneo(Factory);
        var client = await GetAuthenticatedClient();

        var dto = new GrupoDeFasesDTO
        {
            Nombre = "Grupo A",
            Numero = 1,
            GrupoDeFasesPadreId = null,
            EsVisibleEnApp = true
        };

        var response = await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/grupos-de-fases", dto);
        response.EnsureSuccessStatusCode();

        var creado = JsonConvert.DeserializeObject<GrupoDeFasesDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(creado);
        Assert.True(creado.Id > 0);
        Assert.Equal("Grupo A", creado.Nombre);
        Assert.Equal(torneoId, creado.TorneoId);
        Assert.True(creado.EsVisibleEnApp);
    }

    [Fact]
    public async Task CrearSubgrupo_DentroDeGrupoRaiz_200()
    {
        var torneoId = await CrearTorneo(Factory);
        var client = await GetAuthenticatedClient();

        var padre = new GrupoDeFasesDTO { Nombre = "Grupo padre", Numero = 1, EsVisibleEnApp = true };
        var respPadre = await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/grupos-de-fases", padre);
        respPadre.EnsureSuccessStatusCode();
        var grupoPadre = JsonConvert.DeserializeObject<GrupoDeFasesDTO>(await respPadre.Content.ReadAsStringAsync());
        Assert.NotNull(grupoPadre);

        var sub = new GrupoDeFasesDTO
        {
            Nombre = "Subgrupo",
            Numero = 1,
            GrupoDeFasesPadreId = grupoPadre.Id,
            EsVisibleEnApp = true
        };
        var response = await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/grupos-de-fases", sub);
        response.EnsureSuccessStatusCode();

        var creado = JsonConvert.DeserializeObject<GrupoDeFasesDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(creado);
        Assert.Equal(grupoPadre.Id, creado.GrupoDeFasesPadreId);
    }

    [Fact]
    public async Task CrearSubgrupo_DentroDeSubgrupo_400()
    {
        var torneoId = await CrearTorneo(Factory);
        var client = await GetAuthenticatedClient();

        int padreId, subId;
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var padre = new GrupoDeFases { Id = 0, Nombre = "P", Numero = 1, TorneoId = torneoId, EsVisibleEnApp = true };
            ctx.Set<GrupoDeFases>().Add(padre);
            await ctx.SaveChangesAsync();
            padreId = padre.Id;

            var sub = new GrupoDeFases { Id = 0, Nombre = "S", Numero = 1, TorneoId = torneoId, GrupoDeFasesPadreId = padreId, EsVisibleEnApp = true };
            ctx.Set<GrupoDeFases>().Add(sub);
            await ctx.SaveChangesAsync();
            subId = sub.Id;
        }

        var dto = new GrupoDeFasesDTO
        {
            Nombre = "Tercer nivel",
            Numero = 1,
            GrupoDeFasesPadreId = subId,
            EsVisibleEnApp = true
        };
        var response = await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/grupos-de-fases", dto);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EliminarGrupo_DevuelveFasesAlPadre()
    {
        var torneoId = await CrearTorneo(Factory);
        int grupoId, faseId;
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var grupo = new GrupoDeFases { Id = 0, Nombre = "G", Numero = 1, TorneoId = torneoId, EsVisibleEnApp = true };
            ctx.Set<GrupoDeFases>().Add(grupo);
            await ctx.SaveChangesAsync();
            grupoId = grupo.Id;

            var fase = new FaseTodosContraTodos
            {
                Id = 0,
                Nombre = "Fase en grupo",
                Numero = 1,
                TorneoId = torneoId,
                GrupoDeFasesId = grupoId,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            ctx.Fases.Add(fase);
            await ctx.SaveChangesAsync();
            faseId = fase.Id;
        }

        var client = await GetAuthenticatedClient();
        var response = await client.DeleteAsync($"/api/Torneo/{torneoId}/grupos-de-fases/{grupoId}");
        response.EnsureSuccessStatusCode();

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var fase = ctx.Fases.Find(faseId);
            Assert.NotNull(fase);
            Assert.Null(fase.GrupoDeFasesId);
        }
    }

    [Fact]
    public async Task CambiarVisibilidadEnApp_OcultaGrupo()
    {
        var torneoId = await CrearTorneo(Factory);
        var client = await GetAuthenticatedClient();

        var dto = new GrupoDeFasesDTO
        {
            Nombre = "Grupo visible",
            Numero = 1,
            EsVisibleEnApp = true
        };
        var response = await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/grupos-de-fases", dto);
        response.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<GrupoDeFasesDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(creado);

        var putResponse = await client.PutAsJsonAsync(
            $"/api/Torneo/{torneoId}/grupos-de-fases/{creado.Id}/visibilidad-en-app",
            new CambiarVisibilidadEnAppDTO { EsVisibleEnApp = false });
        putResponse.EnsureSuccessStatusCode();

        var getResponse = await client.GetAsync($"/api/Torneo/{torneoId}/grupos-de-fases/{creado.Id}");
        getResponse.EnsureSuccessStatusCode();
        var obtenido = JsonConvert.DeserializeObject<GrupoDeFasesDTO>(await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(obtenido);
        Assert.False(obtenido.EsVisibleEnApp);
    }

    [Fact]
    public async Task CambiarVisibilidadEnApp_VuelveAMostrarGrupo()
    {
        var torneoId = await CrearTorneo(Factory);
        var client = await GetAuthenticatedClient();

        var dto = new GrupoDeFasesDTO
        {
            Nombre = "Grupo toggle",
            Numero = 1,
            EsVisibleEnApp = true
        };
        var response = await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/grupos-de-fases", dto);
        response.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<GrupoDeFasesDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(creado);

        var ocultar = await client.PutAsJsonAsync(
            $"/api/Torneo/{torneoId}/grupos-de-fases/{creado.Id}/visibilidad-en-app",
            new CambiarVisibilidadEnAppDTO { EsVisibleEnApp = false });
        ocultar.EnsureSuccessStatusCode();

        var mostrar = await client.PutAsJsonAsync(
            $"/api/Torneo/{torneoId}/grupos-de-fases/{creado.Id}/visibilidad-en-app",
            new CambiarVisibilidadEnAppDTO { EsVisibleEnApp = true });
        mostrar.EnsureSuccessStatusCode();

        var getResponse = await client.GetAsync($"/api/Torneo/{torneoId}/grupos-de-fases/{creado.Id}");
        getResponse.EnsureSuccessStatusCode();
        var obtenido = JsonConvert.DeserializeObject<GrupoDeFasesDTO>(await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(obtenido);
        Assert.True(obtenido.EsVisibleEnApp);
    }

    [Fact]
    public async Task CambiarVisibilidadEnApp_GrupoInexistente_400()
    {
        var torneoId = await CrearTorneo(Factory);
        var client = await GetAuthenticatedClient();

        var response = await client.PutAsJsonAsync(
            $"/api/Torneo/{torneoId}/grupos-de-fases/999999/visibilidad-en-app",
            new CambiarVisibilidadEnAppDTO { EsVisibleEnApp = false });

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
