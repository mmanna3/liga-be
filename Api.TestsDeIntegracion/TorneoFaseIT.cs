using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace Api.TestsDeIntegracion;

public class TorneoFaseIT : TestBase
{
    public TorneoFaseIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedData(context);
    }

    private static void SeedData(AppDbContext context)
    {
        // Valores predefinidos ya existen por HasData en AppDbContext
    }

    private static async Task<int> CrearTorneoDePrueba(CustomWebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var torneo = new Torneo { Id = 0, Nombre = "Torneo Test Fases", Anio = 2026, TorneoAgrupadorId = 1 };
        context.Torneos.Add(torneo);
        await context.SaveChangesAsync();
        return torneo.Id;
    }

    private static TorneoFaseDTO CrearDtoFaseGrupos(int numero = 1)
    {
        return new TorneoFaseDTO
        {
            Nombre = $"Fase {numero}",
            Numero = numero,
            TipoDeFase = TipoDeFaseEnum.TodosContraTodos,
            InstanciaEliminacionDirectaId = null,
            EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
            EsVisibleEnApp = true
        };
    }

    private static TorneoFaseDTO CrearDtoFaseEliminacionDirecta(int numero = 2, int instanciaId = 8)
    {
        return new TorneoFaseDTO
        {
            Nombre = $"Fase {numero}",
            Numero = numero,
            TipoDeFase = TipoDeFaseEnum.EliminacionDirecta,
            InstanciaEliminacionDirectaId = instanciaId,
            EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
            EsVisibleEnApp = false
        };
    }

    [Fact]
    public async Task ListarFases_TorneoExistente_DevuelveLista()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var response = await client.GetAsync($"/api/Torneo/{torneoId}/fases");

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<TorneoFaseDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Empty(content);
    }

    [Fact]
    public async Task CrearFase_DatosCorrectos_200()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var dto = CrearDtoFaseGrupos(1);

        var response = await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/fases", dto);

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<TorneoFaseDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.True(content.Id > 0);
        Assert.Equal(1, content.Numero);
        Assert.Equal(TipoDeFaseEnum.TodosContraTodos, content.TipoDeFase);
        Assert.Equal("Todos contra todos", content.TipoDeFaseNombre);
        Assert.Null(content.InstanciaEliminacionDirectaId);
        Assert.Null(content.InstanciaEliminacionDirectaNombre);
        Assert.Equal("Inicio pendiente", content.EstadoFaseNombre);
        Assert.Equal(torneoId, content.TorneoId);
    }

    [Fact]
    public async Task CrearFase_EliminacionDirectaConInstancia_200()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var dto = CrearDtoFaseEliminacionDirecta(1, 8);

        var response = await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/fases", dto);

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<TorneoFaseDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.True(content.Id > 0);
        Assert.Equal(8, content.InstanciaEliminacionDirectaId);
        Assert.Equal("Cuartos de final", content.InstanciaEliminacionDirectaNombre);
        Assert.Equal("Eliminación directa", content.TipoDeFaseNombre);
    }

    [Fact]
    public async Task CrearFase_TorneoInexistente_400()
    {
        var client = await GetAuthenticatedClient();

        var dto = CrearDtoFaseGrupos();

        var response = await client.PostAsJsonAsync("/api/Torneo/99999/fases", dto);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var mensaje = await response.Content.ReadAsStringAsync();
        Assert.Contains("torneo", mensaje.ToLowerInvariant());
    }

    [Fact]
    public async Task CrearFase_TodosContraTodosConInstancia_400()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var dto = CrearDtoFaseGrupos(1);
        dto.InstanciaEliminacionDirectaId = 8;

        var response = await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/fases", dto);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var mensaje = await response.Content.ReadAsStringAsync();
        Assert.Contains("eliminación", mensaje.ToLowerInvariant());
    }

    [Fact]
    public async Task ObtenerFase_PorId_DevuelveCorrecto()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        TorneoFase fase;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            fase = new FaseTodosContraTodos
            {
                Id = 0,
                Nombre = "",
                Numero = 1,
                TorneoId = torneoId,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            context.TorneoFases.Add(fase);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/Torneo/{torneoId}/fases/{fase.Id}");

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<TorneoFaseDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(fase.Id, content.Id);
        Assert.Equal(1, content.Numero);
        Assert.Equal("Todos contra todos", content.TipoDeFaseNombre);
        Assert.Equal("Inicio pendiente", content.EstadoFaseNombre);
    }

    [Fact]
    public async Task ObtenerFase_FaseDeOtroTorneo_404()
    {
        var torneo1Id = await CrearTorneoDePrueba(Factory);
        int faseId;
        int torneo2Id;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var torneo2 = new Torneo { Id = 0, Nombre = "Otro Torneo", Anio = 2026, TorneoAgrupadorId = 1 };
            context.Torneos.Add(torneo2);
            await context.SaveChangesAsync();
            torneo2Id = torneo2.Id;

            var fase = new FaseTodosContraTodos
            {
                Id = 0,
                Nombre = "",
                Numero = 1,
                TorneoId = torneo2Id,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            context.TorneoFases.Add(fase);
            await context.SaveChangesAsync();
            faseId = fase.Id;
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/Torneo/{torneo1Id}/fases/{faseId}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ModificarFase_DatosCorrectos_204()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        TorneoFase fase;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            fase = new FaseTodosContraTodos
            {
                Id = 0,
                Nombre = "",
                Numero = 1,
                TorneoId = torneoId,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            context.TorneoFases.Add(fase);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var dto = CrearDtoFaseGrupos(1);
        dto.Id = fase.Id;
        dto.TorneoId = torneoId;
        dto.EsVisibleEnApp = false;

        var response = await client.PutAsJsonAsync($"/api/Torneo/{torneoId}/fases/{fase.Id}", dto);

        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var actualizado = context.TorneoFases.Find(fase.Id);
            Assert.NotNull(actualizado);
            Assert.False(actualizado.EsVisibleEnApp);
        }
    }

    [Fact]
    public async Task EliminarFase_Existente_200()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        TorneoFase fase;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            fase = new FaseTodosContraTodos
            {
                Id = 0,
                Nombre = "",
                Numero = 1,
                TorneoId = torneoId,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            context.TorneoFases.Add(fase);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var response = await client.DeleteAsync($"/api/Torneo/{torneoId}/fases/{fase.Id}");

        response.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Null(context.TorneoFases.Find(fase.Id));
        }
    }

    [Fact]
    public async Task EliminarFase_FaseDeOtroTorneo_404()
    {
        var torneo1Id = await CrearTorneoDePrueba(Factory);
        int faseId;
        int torneo2Id;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var torneo2 = new Torneo { Id = 0, Nombre = "Otro Torneo", Anio = 2026, TorneoAgrupadorId = 1 };
            context.Torneos.Add(torneo2);
            await context.SaveChangesAsync();
            torneo2Id = torneo2.Id;

            var fase = new FaseTodosContraTodos
            {
                Id = 0,
                Nombre = "",
                Numero = 1,
                TorneoId = torneo2Id,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            context.TorneoFases.Add(fase);
            await context.SaveChangesAsync();
            faseId = fase.Id;
        }

        var client = await GetAuthenticatedClient();
        var response = await client.DeleteAsync($"/api/Torneo/{torneo1Id}/fases/{faseId}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.NotNull(context.TorneoFases.Find(faseId));
        }
    }

    [Fact]
    public async Task ListarFases_ConFasesCreadas_DevuelveTodasDelTorneo()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/fases", CrearDtoFaseGrupos(1));
        await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/fases", CrearDtoFaseEliminacionDirecta(2));

        var response = await client.GetAsync($"/api/Torneo/{torneoId}/fases");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<TorneoFaseDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(2, content.Count);
        Assert.Contains(content, f => f.Numero == 1);
        Assert.Contains(content, f => f.Numero == 2);

        var faseGrupos = content.Single(f => f.Numero == 1);
        Assert.Equal("Todos contra todos", faseGrupos.TipoDeFaseNombre);
        Assert.Null(faseGrupos.InstanciaEliminacionDirectaNombre);

        var faseEliminacion = content.Single(f => f.Numero == 2);
        Assert.Equal("Eliminación directa", faseEliminacion.TipoDeFaseNombre);
        Assert.Equal("Cuartos de final", faseEliminacion.InstanciaEliminacionDirectaNombre);
    }

    [Fact]
    public async Task ObtenerFase_SinZonas_SePuedeEditarTrue()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        TorneoFase fase;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            fase = new FaseTodosContraTodos
            {
                Id = 0,
                Nombre = "",
                Numero = 1,
                TorneoId = torneoId,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            context.TorneoFases.Add(fase);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/Torneo/{torneoId}/fases/{fase.Id}");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<TorneoFaseDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.True(content.SePuedeEditar);
    }

    [Fact]
    public async Task ObtenerFase_ConZonasSinFechas_SePuedeEditarFalse()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        TorneoFase fase;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            fase = new FaseTodosContraTodos
            {
                Id = 0,
                Nombre = "",
                Numero = 1,
                TorneoId = torneoId,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            context.TorneoFases.Add(fase);
            await context.SaveChangesAsync();

            context.TorneoZonas.Add(new ZonaTodosContraTodos { Id = 0, Nombre = "Zona A", TorneoFaseId = fase.Id });
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/Torneo/{torneoId}/fases/{fase.Id}");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<TorneoFaseDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.False(content.SePuedeEditar);
    }

    [Fact]
    public async Task ObtenerFase_ConZonasConFechas_SePuedeEditarFalse()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        TorneoFase fase;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            fase = new FaseTodosContraTodos
            {
                Id = 0,
                Nombre = "",
                Numero = 1,
                TorneoId = torneoId,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            context.TorneoFases.Add(fase);
            await context.SaveChangesAsync();

            var zona = new ZonaTodosContraTodos { Id = 0, Nombre = "Zona A", TorneoFaseId = fase.Id };
            context.TorneoZonas.Add(zona);
            await context.SaveChangesAsync();

            context.TorneoFechas.Add(new TorneoFecha
            {
                Id = 0,
                Dia = new DateOnly(2026, 5, 10),
                Numero = 1,
                ZonaId = zona.Id,
                EsVisibleEnApp = true
            });
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/Torneo/{torneoId}/fases/{fase.Id}");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<TorneoFaseDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.False(content.SePuedeEditar);
    }

    [Fact]
    public async Task ObtenerFase_DevuelveNombresMapeadosCorrectamente()
    {
        var torneoId = await CrearTorneoDePrueba(Factory);
        TorneoFase fase;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            fase = new FaseEliminacionDirecta
            {
                Id = 0,
                Nombre = "",
                Numero = 1,
                TorneoId = torneoId,
                InstanciaEliminacionDirectaId = 4,
                EstadoFaseId = 200,
                EsVisibleEnApp = true
            };
            context.TorneoFases.Add(fase);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/Torneo/{torneoId}/fases/{fase.Id}");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<TorneoFaseDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal("Eliminación directa", content.TipoDeFaseNombre);
        Assert.Equal("Semifinal", content.InstanciaEliminacionDirectaNombre);
        Assert.Equal("En curso", content.EstadoFaseNombre);
    }
}
