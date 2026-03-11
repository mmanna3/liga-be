using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace Api.TestsDeIntegracion;

public class TorneoFechasIT : TestBase
{
    public TorneoFechasIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedData(context);
    }

    private static void SeedData(AppDbContext context)
    {
    }

    private static async Task<int> CrearTorneoZonaDePrueba(CustomWebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var torneo = new Torneo { Id = 0, Nombre = "Torneo Test Fechas", Anio = 2026, TorneoAgrupadorId = 1 };
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

        var zona = new TorneoZona { Id = 0, Nombre = "Zona única", TorneoFaseId = fase.Id };
        context.TorneoZonas.Add(zona);
        await context.SaveChangesAsync();
        return zona.Id;
    }

    [Fact]
    public async Task ListarFechas_ZonaExistente_DevuelveLista()
    {
        var zonaId = await CrearTorneoZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var response = await client.GetAsync($"/api/TorneoZona/{zonaId}/fechas");

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<TorneoFechaDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Empty(content);
    }

    [Fact]
    public async Task CrearFecha_DatosCorrectos_200()
    {
        var zonaId = await CrearTorneoZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var dto = new TorneoFechaDTO
        {
            Dia = new DateOnly(2026, 3, 15),
            Numero = 1,
            EsVisibleEnApp = true
        };

        var response = await client.PostAsJsonAsync($"/api/TorneoZona/{zonaId}/fechas", dto);

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<TorneoFechaDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.True(content.Id > 0);
        Assert.Equal(new DateOnly(2026, 3, 15), content.Dia);
        Assert.Equal(1, content.Numero);
        Assert.Equal(zonaId, content.ZonaId);
        Assert.True(content.EsVisibleEnApp);
    }

    [Fact]
    public async Task CrearFecha_ZonaInexistente_400()
    {
        var client = await GetAuthenticatedClient();

        var dto = new TorneoFechaDTO
        {
            Dia = new DateOnly(2026, 3, 15),
            Numero = 1,
            EsVisibleEnApp = true
        };

        var response = await client.PostAsJsonAsync("/api/TorneoZona/99999/fechas", dto);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var mensaje = await response.Content.ReadAsStringAsync();
        Assert.Contains("zona", mensaje.ToLowerInvariant());
    }

    [Fact]
    public async Task ObtenerFecha_PorId_DevuelveCorrecto()
    {
        var zonaId = await CrearTorneoZonaDePrueba(Factory);
        TorneoFecha fecha;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            fecha = new TorneoFecha
            {
                Id = 0,
                Dia = new DateOnly(2026, 4, 20),
                Numero = 2,
                ZonaId = zonaId,
                EsVisibleEnApp = true
            };
            context.TorneoFechas.Add(fecha);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/TorneoZona/{zonaId}/fechas/{fecha.Id}");

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<TorneoFechaDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(fecha.Id, content.Id);
        Assert.Equal(new DateOnly(2026, 4, 20), content.Dia);
        Assert.Equal(2, content.Numero);
    }

    [Fact]
    public async Task ObtenerFecha_FechaDeOtraZona_404()
    {
        var zona1Id = await CrearTorneoZonaDePrueba(Factory);
        int fechaId;
        int zona2Id;
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

            var zona2 = new TorneoZona { Id = 0, Nombre = "Zona B", TorneoFaseId = fase2.Id };
            context.TorneoZonas.Add(zona2);
            await context.SaveChangesAsync();
            zona2Id = zona2.Id;

            var fecha = new TorneoFecha
            {
                Id = 0,
                Dia = new DateOnly(2026, 5, 1),
                Numero = 1,
                ZonaId = zona2Id,
                EsVisibleEnApp = true
            };
            context.TorneoFechas.Add(fecha);
            await context.SaveChangesAsync();
            fechaId = fecha.Id;
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/TorneoZona/{zona1Id}/fechas/{fechaId}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ModificarFecha_DatosCorrectos_204()
    {
        var zonaId = await CrearTorneoZonaDePrueba(Factory);
        TorneoFecha fecha;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            fecha = new TorneoFecha
            {
                Id = 0,
                Dia = new DateOnly(2026, 3, 1),
                Numero = 1,
                ZonaId = zonaId,
                EsVisibleEnApp = true
            };
            context.TorneoFechas.Add(fecha);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var dto = new TorneoFechaDTO
        {
            Id = fecha.Id,
            Dia = new DateOnly(2026, 3, 10),
            Numero = 1,
            ZonaId = zonaId,
            EsVisibleEnApp = false
        };

        var response = await client.PutAsJsonAsync($"/api/TorneoZona/{zonaId}/fechas/{fecha.Id}", dto);

        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var actualizado = context.TorneoFechas.Find(fecha.Id);
            Assert.NotNull(actualizado);
            Assert.Equal(new DateOnly(2026, 3, 10), actualizado.Dia);
            Assert.False(actualizado.EsVisibleEnApp);
        }
    }

    [Fact]
    public async Task EliminarFecha_Existente_200()
    {
        var zonaId = await CrearTorneoZonaDePrueba(Factory);
        TorneoFecha fecha;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            fecha = new TorneoFecha
            {
                Id = 0,
                Dia = new DateOnly(2026, 3, 1),
                Numero = 1,
                ZonaId = zonaId,
                EsVisibleEnApp = true
            };
            context.TorneoFechas.Add(fecha);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var response = await client.DeleteAsync($"/api/TorneoZona/{zonaId}/fechas/{fecha.Id}");

        response.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Null(context.TorneoFechas.Find(fecha.Id));
        }
    }

    [Fact]
    public async Task EliminarFecha_FechaDeOtraZona_404()
    {
        var zona1Id = await CrearTorneoZonaDePrueba(Factory);
        int fechaId;
        int zona2Id;
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

            var zona2 = new TorneoZona { Id = 0, Nombre = "Zona B", TorneoFaseId = fase2.Id };
            context.TorneoZonas.Add(zona2);
            await context.SaveChangesAsync();
            zona2Id = zona2.Id;

            var fecha = new TorneoFecha
            {
                Id = 0,
                Dia = new DateOnly(2026, 5, 1),
                Numero = 1,
                ZonaId = zona2Id,
                EsVisibleEnApp = true
            };
            context.TorneoFechas.Add(fecha);
            await context.SaveChangesAsync();
            fechaId = fecha.Id;
        }

        var client = await GetAuthenticatedClient();
        var response = await client.DeleteAsync($"/api/TorneoZona/{zona1Id}/fechas/{fechaId}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.NotNull(context.TorneoFechas.Find(fechaId));
        }
    }

    [Fact]
    public async Task ListarFechas_ConFechasCreadas_DevuelveTodasDeLaZona()
    {
        var zonaId = await CrearTorneoZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        await client.PostAsJsonAsync($"/api/TorneoZona/{zonaId}/fechas", new TorneoFechaDTO
        {
            Dia = new DateOnly(2026, 3, 1),
            Numero = 1,
            EsVisibleEnApp = true
        });
        await client.PostAsJsonAsync($"/api/TorneoZona/{zonaId}/fechas", new TorneoFechaDTO
        {
            Dia = new DateOnly(2026, 3, 8),
            Numero = 2,
            EsVisibleEnApp = true
        });

        var response = await client.GetAsync($"/api/TorneoZona/{zonaId}/fechas");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<TorneoFechaDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(2, content.Count);
        Assert.Contains(content, f => f.Numero == 1 && f.Dia == new DateOnly(2026, 3, 1));
        Assert.Contains(content, f => f.Numero == 2 && f.Dia == new DateOnly(2026, 3, 8));
    }
}
