using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api._Config;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Api.TestsUtilidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable InconsistentNaming

namespace Api.TestsDeIntegracion;

public class FechasIT : TestBase
{
    private static readonly JsonSerializerOptions FechaJsonOptions = CreateFechaJsonOptions();

    private static JsonSerializerOptions CreateFechaJsonOptions()
    {
        var o = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        o.Converters.Add(new DateOnlyJsonConverter());
        o.Converters.Add(new FechaJsonConverter());
        o.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        return o;
    }

    private static FechaTodosContraTodosDTO DeserializeFechaTct(string json) =>
        JsonSerializer.Deserialize<FechaTodosContraTodosDTO>(json, FechaJsonOptions)!;

    private static List<FechaTodosContraTodosDTO> DeserializeFechaTctList(string json) =>
        JsonSerializer.Deserialize<List<FechaTodosContraTodosDTO>>(json, FechaJsonOptions) ?? [];

    private static FechaEliminacionDirectaDTO DeserializeFechaEd(string json) =>
        JsonSerializer.Deserialize<FechaEliminacionDirectaDTO>(json, FechaJsonOptions)!;

    private global::Api.TestsUtilidades.Utilidades? _utilidades;
    private Club? _club;

    public FechasIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedData(context);
    }

    private void SeedData(AppDbContext context)
    {
        _utilidades = new global::Api.TestsUtilidades.Utilidades(context);
        _club = _utilidades.DadoQueExisteElClub();
        _utilidades.DadoQueExisteElEquipo(_club);
        context.SaveChanges();
    }

    private static async Task<int> CrearZonaDePrueba(CustomWebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var torneo = new Torneo { Id = 0, Nombre = "Torneo Test Fechas", Anio = 2026, TorneoAgrupadorId = 1, EsVisibleEnApp = true, SeVenLosGolesEnTablaDePosiciones = true };
        context.Torneos.Add(torneo);
        await context.SaveChangesAsync();

        var fase = new FaseTodosContraTodos
        {
            Id = 0,
            Nombre = "",
            Numero = 1,
            TorneoId = torneo.Id,
            EstadoFaseId = 100,
            EsVisibleEnApp = true
        };
        context.Fases.Add(fase);
        await context.SaveChangesAsync();

        var zona = new ZonaTodosContraTodos { Id = 0, Nombre = "Zona única", FaseId = fase.Id };
        context.Zonas.Add(zona);
        await context.SaveChangesAsync();
        return zona.Id;
    }

    private static async Task<int> CrearZonaEliminacionDirectaDePrueba(CustomWebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var torneo = new Torneo { Id = 0, Nombre = "Torneo Test ED Fechas", Anio = 2026, TorneoAgrupadorId = 1, EsVisibleEnApp = true, SeVenLosGolesEnTablaDePosiciones = true };
        context.Torneos.Add(torneo);
        await context.SaveChangesAsync();

        var fase = new FaseEliminacionDirecta
        {
            Id = 0,
            Nombre = "Fase ED",
            Numero = 1,
            TorneoId = torneo.Id,
            EstadoFaseId = 100,
            EsVisibleEnApp = true
        };
        context.Fases.Add(fase);
        await context.SaveChangesAsync();

        var cat = new TorneoCategoria
        {
            Id = 0,
            Nombre = "Cat ED",
            AnioDesde = 2010,
            AnioHasta = 2020,
            TorneoId = torneo.Id
        };
        context.TorneoCategorias.Add(cat);
        await context.SaveChangesAsync();

        var zona = new ZonaEliminacionDirecta
        {
            Id = 0,
            Nombre = "Zona ED",
            FaseId = fase.Id,
            CategoriaId = cat.Id
        };
        context.Zonas.Add(zona);
        await context.SaveChangesAsync();
        return zona.Id;
    }

    private static async Task<int> ObtenerTorneoIdDeZona(CustomWebApplicationFactory<Program> factory, int zonaId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await (from z in context.Zonas
            join f in context.Fases on z.FaseId equals f.Id
            where z.Id == zonaId
            select f.TorneoId).FirstAsync();
    }

    private static async Task SeedTorneoCategorias(CustomWebApplicationFactory<Program> factory, int torneoId, int cantidad)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        for (var i = 0; i < cantidad; i++)
        {
            context.TorneoCategorias.Add(new TorneoCategoria
            {
                Id = 0,
                Nombre = $"Cat Test {i}",
                AnioDesde = 2010,
                AnioHasta = 2020,
                TorneoId = torneoId
            });
        }
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task ListarFechas_ZonaExistente_DevuelveLista()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var response = await client.GetAsync($"/api/Zona/{zonaId}/fechas");

        response.EnsureSuccessStatusCode();

        var content = DeserializeFechaTctList(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Empty(content);
    }

    [Fact]
    public async Task CrearFecha_DatosCorrectos_200()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var dto = new FechaTodosContraTodosDTO
        {
            Dia = new DateOnly(2026, 3, 15),
            Numero = 1,
            EsVisibleEnApp = true
        };

        var response = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas", dto, FechaJsonOptions);

        response.EnsureSuccessStatusCode();

        var content = DeserializeFechaTct(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.True(content.Id > 0);
        Assert.Equal(new DateOnly(2026, 3, 15), content.Dia);
        Assert.Equal(1, Assert.IsType<FechaTodosContraTodosDTO>(content).Numero);
        Assert.Equal(zonaId, content.ZonaId);
        Assert.True(content.EsVisibleEnApp);
    }

    [Fact]
    public async Task CrearFecha_ZonaInexistente_400()
    {
        var client = await GetAuthenticatedClient();

        var dto = new FechaTodosContraTodosDTO
        {
            Dia = new DateOnly(2026, 3, 15),
            Numero = 1,
            EsVisibleEnApp = true
        };

        var response = await client.PostAsJsonAsync("/api/Zona/99999/fechas", dto, FechaJsonOptions);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var mensaje = await response.Content.ReadAsStringAsync();
        Assert.Contains("zona", mensaje.ToLowerInvariant());
    }

    [Fact]
    public async Task ObtenerFecha_PorId_DevuelveCorrecto()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        Fecha fecha;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            fecha = new FechaTodosContraTodos
            {
                Id = 0,
                Dia = new DateOnly(2026, 4, 20),
                Numero = 2,
                ZonaId = zonaId,
                EsVisibleEnApp = true
            };
            context.Fechas.Add(fecha);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/Zona/{zonaId}/fechas/{fecha.Id}");

        response.EnsureSuccessStatusCode();

        var content = DeserializeFechaTct(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(fecha.Id, content.Id);
        Assert.Equal(new DateOnly(2026, 4, 20), content.Dia);
        Assert.Equal(2, Assert.IsType<FechaTodosContraTodosDTO>(content).Numero);
    }

    [Fact]
    public async Task ObtenerFecha_FechaDeOtraZona_404()
    {
        var zona1Id = await CrearZonaDePrueba(Factory);
        int fechaId;
        int zona2Id;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var torneo2 = new Torneo { Id = 0, Nombre = "Otro Torneo", Anio = 2026, TorneoAgrupadorId = 1, EsVisibleEnApp = true, SeVenLosGolesEnTablaDePosiciones = true };
            context.Torneos.Add(torneo2);
            await context.SaveChangesAsync();

            var fase2 = new FaseTodosContraTodos
            {
                Id = 0,
                Nombre = "",
                Numero = 1,
                TorneoId = torneo2.Id,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            context.Fases.Add(fase2);
            await context.SaveChangesAsync();

            var zona2 = new ZonaTodosContraTodos { Id = 0, Nombre = "Zona B", FaseId = fase2.Id };
            context.Zonas.Add(zona2);
            await context.SaveChangesAsync();
            zona2Id = zona2.Id;

            var fecha = new FechaTodosContraTodos
            {
                Id = 0,
                Dia = new DateOnly(2026, 5, 1),
                Numero = 1,
                ZonaId = zona2Id,
                EsVisibleEnApp = true
            };
            context.Fechas.Add(fecha);
            await context.SaveChangesAsync();
            fechaId = fecha.Id;
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/Zona/{zona1Id}/fechas/{fechaId}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ModificarFecha_DatosCorrectos_204()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        Fecha fecha;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            fecha = new FechaTodosContraTodos
            {
                Id = 0,
                Dia = new DateOnly(2026, 3, 1),
                Numero = 1,
                ZonaId = zonaId,
                EsVisibleEnApp = true
            };
            context.Fechas.Add(fecha);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var dto = new FechaTodosContraTodosDTO
        {
            Id = fecha.Id,
            Dia = new DateOnly(2026, 3, 10),
            Numero = 1,
            ZonaId = zonaId,
            EsVisibleEnApp = false
        };

        var response = await client.PutAsJsonAsync($"/api/Zona/{zonaId}/fechas/{fecha.Id}", dto, FechaJsonOptions);

        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var actualizado = context.Fechas.Find(fecha.Id);
            Assert.NotNull(actualizado);
            Assert.Equal(new DateOnly(2026, 3, 10), actualizado.Dia);
            Assert.False(actualizado.EsVisibleEnApp);
        }
    }

    [Fact]
    public async Task EliminarFecha_Existente_200()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        Fecha fecha;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            fecha = new FechaTodosContraTodos
            {
                Id = 0,
                Dia = new DateOnly(2026, 3, 1),
                Numero = 1,
                ZonaId = zonaId,
                EsVisibleEnApp = true
            };
            context.Fechas.Add(fecha);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var response = await client.DeleteAsync($"/api/Zona/{zonaId}/fechas/{fecha.Id}");

        response.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Null(context.Fechas.Find(fecha.Id));
        }
    }

    [Fact]
    public async Task EliminarFecha_FechaDeOtraZona_404()
    {
        var zona1Id = await CrearZonaDePrueba(Factory);
        int fechaId;
        int zona2Id;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var torneo2 = new Torneo { Id = 0, Nombre = "Otro Torneo", Anio = 2026, TorneoAgrupadorId = 1, EsVisibleEnApp = true, SeVenLosGolesEnTablaDePosiciones = true };
            context.Torneos.Add(torneo2);
            await context.SaveChangesAsync();

            var fase2 = new FaseTodosContraTodos
            {
                Id = 0,
                Nombre = "",
                Numero = 1,
                TorneoId = torneo2.Id,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            context.Fases.Add(fase2);
            await context.SaveChangesAsync();

            var zona2 = new ZonaTodosContraTodos { Id = 0, Nombre = "Zona B", FaseId = fase2.Id };
            context.Zonas.Add(zona2);
            await context.SaveChangesAsync();
            zona2Id = zona2.Id;

            var fecha = new FechaTodosContraTodos
            {
                Id = 0,
                Dia = new DateOnly(2026, 5, 1),
                Numero = 1,
                ZonaId = zona2Id,
                EsVisibleEnApp = true
            };
            context.Fechas.Add(fecha);
            await context.SaveChangesAsync();
            fechaId = fecha.Id;
        }

        var client = await GetAuthenticatedClient();
        var response = await client.DeleteAsync($"/api/Zona/{zona1Id}/fechas/{fechaId}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.NotNull(context.Fechas.Find(fechaId));
        }
    }

    [Fact]
    public async Task ListarFechas_ConFechasCreadas_DevuelveTodasDeLaZona()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas", new FechaTodosContraTodosDTO
        {
            Dia = new DateOnly(2026, 3, 1),
            Numero = 1,
            EsVisibleEnApp = true
        }, FechaJsonOptions);
        await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas", new FechaTodosContraTodosDTO
        {
            Dia = new DateOnly(2026, 3, 8),
            Numero = 2,
            EsVisibleEnApp = true
        }, FechaJsonOptions);

        var response = await client.GetAsync($"/api/Zona/{zonaId}/fechas");
        response.EnsureSuccessStatusCode();

        var content = DeserializeFechaTctList(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(2, content.Count);
        Assert.Contains(content, f => f is FechaTodosContraTodosDTO t && t.Numero == 1 && f.Dia == new DateOnly(2026, 3, 1));
        Assert.Contains(content, f => f is FechaTodosContraTodosDTO t && t.Numero == 2 && f.Dia == new DateOnly(2026, 3, 8));
    }

    [Fact]
    public async Task CrearFechasMasivamente_DatosCorrectos_200()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var dtos = new List<FechaTodosContraTodosDTO>
        {
            new() { Dia = new DateOnly(2026, 3, 1), Numero = 1, EsVisibleEnApp = true },
            new() { Dia = new DateOnly(2026, 3, 8), Numero = 2, EsVisibleEnApp = true },
            new() { Dia = new DateOnly(2026, 3, 15), Numero = 3, EsVisibleEnApp = false }
        };

        var response = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas/crear-fechas-todoscontratodos-masivamente", dtos, FechaJsonOptions);
        response.EnsureSuccessStatusCode();

        var creados = DeserializeFechaTctList(await response.Content.ReadAsStringAsync());
        Assert.NotNull(creados);
        Assert.Equal(3, creados.Count);
        Assert.All(creados, f => Assert.True(f.Id > 0));
        Assert.Contains(creados, f => f.Numero == 1 && f.Dia == new DateOnly(2026, 3, 1));
        Assert.Contains(creados, f => f.Numero == 2 && f.Dia == new DateOnly(2026, 3, 8));
        Assert.Contains(creados, f => f.Numero == 3 && f.Dia == new DateOnly(2026, 3, 15));

        var listResponse = await client.GetAsync($"/api/Zona/{zonaId}/fechas");
        listResponse.EnsureSuccessStatusCode();
        var list = DeserializeFechaTctList(await listResponse.Content.ReadAsStringAsync());
        Assert.NotNull(list);
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public async Task ModificarFechasMasivamente_FechasNoIncluidasEnArray_SeEliminan()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var postResponse = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas/crear-fechas-todoscontratodos-masivamente",
            new List<FechaTodosContraTodosDTO>
            {
                new() { Dia = new DateOnly(2026, 3, 1), Numero = 1, EsVisibleEnApp = true },
                new() { Dia = new DateOnly(2026, 3, 8), Numero = 2, EsVisibleEnApp = true },
                new() { Dia = new DateOnly(2026, 3, 15), Numero = 3, EsVisibleEnApp = true }
            }, FechaJsonOptions);
        postResponse.EnsureSuccessStatusCode();
        var creados = DeserializeFechaTctList(await postResponse.Content.ReadAsStringAsync())!;

        var dtosModificar = new List<FechaDTO>
        {
            new FechaTodosContraTodosDTO { Id = creados[0].Id, Dia = new DateOnly(2026, 3, 5), Numero = 1, ZonaId = zonaId, EsVisibleEnApp = true },
            new FechaTodosContraTodosDTO { Id = creados[2].Id, Dia = new DateOnly(2026, 3, 20), Numero = 3, ZonaId = zonaId, EsVisibleEnApp = true }
        };

        var putResponse = await client.PutAsJsonAsync($"/api/Zona/{zonaId}/fechas/modificar-fechas-masivamente", dtosModificar, FechaJsonOptions);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, putResponse.StatusCode);

        var listResponse = await client.GetAsync($"/api/Zona/{zonaId}/fechas");
        listResponse.EnsureSuccessStatusCode();
        var fechas = DeserializeFechaTctList(await listResponse.Content.ReadAsStringAsync());
        Assert.NotNull(fechas);
        Assert.Equal(2, fechas.Count);
        Assert.Contains(fechas, f => f.Dia == new DateOnly(2026, 3, 5));
        Assert.Contains(fechas, f => f.Dia == new DateOnly(2026, 3, 20));
        Assert.Contains(fechas, f => f is FechaTodosContraTodosDTO t && t.Numero == 1);
        Assert.Contains(fechas, f => f is FechaTodosContraTodosDTO t && t.Numero == 2);
    }

    [Fact]
    public async Task EliminarFecha_RenumeraLasSiguientesConsecutivamente()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var postResponse = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas/crear-fechas-todoscontratodos-masivamente",
            new List<FechaTodosContraTodosDTO>
            {
                new() { Dia = new DateOnly(2026, 3, 1), Numero = 1, EsVisibleEnApp = true },
                new() { Dia = new DateOnly(2026, 3, 8), Numero = 2, EsVisibleEnApp = true },
                new() { Dia = new DateOnly(2026, 3, 15), Numero = 3, EsVisibleEnApp = true },
                new() { Dia = new DateOnly(2026, 3, 22), Numero = 4, EsVisibleEnApp = true }
            }, FechaJsonOptions);
        postResponse.EnsureSuccessStatusCode();
        var creados = DeserializeFechaTctList(await postResponse.Content.ReadAsStringAsync())!;
        var fechaAEliminarId = creados[1].Id;

        var deleteResponse = await client.DeleteAsync($"/api/Zona/{zonaId}/fechas/{fechaAEliminarId}");
        deleteResponse.EnsureSuccessStatusCode();

        var listResponse = await client.GetAsync($"/api/Zona/{zonaId}/fechas");
        listResponse.EnsureSuccessStatusCode();
        var fechas = DeserializeFechaTctList(await listResponse.Content.ReadAsStringAsync());
        Assert.NotNull(fechas);
        Assert.Equal(3, fechas.Count);
        Assert.Contains(fechas, f => f is FechaTodosContraTodosDTO t && t.Numero == 1);
        Assert.Contains(fechas, f => f is FechaTodosContraTodosDTO t && t.Numero == 2);
        Assert.Contains(fechas, f => f is FechaTodosContraTodosDTO t && t.Numero == 3);
        Assert.DoesNotContain(fechas, f => f is FechaTodosContraTodosDTO t && t.Numero == 4);
    }

    [Fact]
    public async Task ModificarFechasMasivamente_ConFechaNuevaSinId_CreaLaFechaNueva()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var postResponse = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas/crear-fechas-todoscontratodos-masivamente",
            new List<FechaTodosContraTodosDTO>
            {
                new() { Dia = new DateOnly(2026, 3, 1), Numero = 1, EsVisibleEnApp = true }
            }, FechaJsonOptions);
        postResponse.EnsureSuccessStatusCode();
        var creados = DeserializeFechaTctList(await postResponse.Content.ReadAsStringAsync())!;
        var fechaExistenteId = creados[0].Id;

        var dtosModificar = new List<FechaDTO>
        {
            new FechaTodosContraTodosDTO { Id = fechaExistenteId, Dia = new DateOnly(2026, 3, 10), Numero = 1, ZonaId = zonaId, EsVisibleEnApp = false },
            new FechaTodosContraTodosDTO { Dia = new DateOnly(2026, 3, 17), Numero = 2, EsVisibleEnApp = true }
        };

        var putResponse = await client.PutAsJsonAsync($"/api/Zona/{zonaId}/fechas/modificar-fechas-masivamente", dtosModificar, FechaJsonOptions);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, putResponse.StatusCode);

        var listResponse = await client.GetAsync($"/api/Zona/{zonaId}/fechas");
        listResponse.EnsureSuccessStatusCode();
        var fechas = DeserializeFechaTctList(await listResponse.Content.ReadAsStringAsync());
        Assert.NotNull(fechas);
        Assert.Equal(2, fechas.Count);

        var fechaModificada = fechas.FirstOrDefault(f => f.Id == fechaExistenteId);
        Assert.NotNull(fechaModificada);
        Assert.Equal(new DateOnly(2026, 3, 10), fechaModificada.Dia);
        Assert.False(fechaModificada.EsVisibleEnApp);

        var fechaNueva = fechas.FirstOrDefault(f => f.Dia == new DateOnly(2026, 3, 17));
        Assert.NotNull(fechaNueva);
        Assert.True(fechaNueva.Id > 0);
        Assert.Equal(2, Assert.IsType<FechaTodosContraTodosDTO>(fechaNueva).Numero);
    }

    [Fact]
    public async Task CrearFechasMasivamente_ConJornadas_CreaFechasConJornadas()
    {
        Assert.NotNull(_club);
        var zonaId = await CrearZonaDePrueba(Factory);
        var torneoId = await ObtenerTorneoIdDeZona(Factory, zonaId);
        await SeedTorneoCategorias(Factory, torneoId, 3);
        var client = await GetAuthenticatedClient();

        int equipo1Id;
        int equipo2Id;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var equipos = context.Equipos.Where(e => e.ClubId == _club.Id).ToList();
            equipo1Id = equipos[0].Id;
            if (equipos.Count < 2)
            {
                var eq2 = new Equipo { Id = 0, Nombre = "Equipo 2 Fechas", ClubId = _club.Id, Jugadores = [] };
                context.Equipos.Add(eq2);
                context.SaveChanges();
                equipo2Id = eq2.Id;
            }
            else
            {
                equipo2Id = equipos[1].Id;
            }
        }

        var dtos = new List<FechaTodosContraTodosDTO>
        {
            new()
            {
                Dia = new DateOnly(2026, 3, 1),
                Numero = 1,
                EsVisibleEnApp = true,
                Jornadas =
                [
                    new JornadaDTO
                    {
                        Tipo = "Normal",
                        ResultadosVerificados = false,
                        LocalId = equipo1Id,
                        VisitanteId = equipo2Id
                    },
                    new JornadaDTO
                    {
                        Tipo = "Libre",
                        ResultadosVerificados = false,
                        EquipoLocalId = equipo1Id
                    }
                ]
            }
        };

        var response = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas/crear-fechas-todoscontratodos-masivamente", dtos, FechaJsonOptions);
        response.EnsureSuccessStatusCode();

        var creados = DeserializeFechaTctList(await response.Content.ReadAsStringAsync());
        Assert.NotNull(creados);
        Assert.Single(creados);
        Assert.NotNull(creados[0].Jornadas);
        Assert.Equal(2, creados[0].Jornadas!.Count);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var fecha = context.Fechas.Include(f => f.Jornadas).First(f => f.Id == creados[0].Id);
            Assert.Equal(2, fecha.Jornadas.Count);
            var jornadaIds = fecha.Jornadas.Select(j => j.Id).ToList();
            var partidos = context.Partidos.Where(p => jornadaIds.Contains(p.JornadaId)).ToList();
            Assert.Equal(6, partidos.Count); // 3 categorías × 2 jornadas
            Assert.All(partidos, p =>
            {
                Assert.Equal("", p.ResultadoLocal);
                Assert.Equal("", p.ResultadoVisitante);
            });
        }
    }

    [Fact]
    public async Task CrearFechasMasivamente_NormalAntesQueInterzonal_LosIdsDeJornadaSiguenElOrdenDelPayload()
    {
        Assert.NotNull(_club);
        var zonaId = await CrearZonaDePrueba(Factory);
        var torneoId = await ObtenerTorneoIdDeZona(Factory, zonaId);
        await SeedTorneoCategorias(Factory, torneoId, 1);
        var client = await GetAuthenticatedClient();

        int e1, e2, e3;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var equipos = context.Equipos.Where(eq => eq.ClubId == _club.Id).OrderBy(eq => eq.Id).ToList();
            while (equipos.Count < 3)
            {
                var nuevo = new Equipo
                {
                    Id = 0,
                    Nombre = $"Eq orden jornada {equipos.Count}",
                    ClubId = _club.Id,
                    Jugadores = []
                };
                context.Equipos.Add(nuevo);
                context.SaveChanges();
                equipos.Add(nuevo);
            }

            e1 = equipos[0].Id;
            e2 = equipos[1].Id;
            e3 = equipos[2].Id;
        }

        var dtos = new List<FechaTodosContraTodosDTO>
        {
            new()
            {
                Dia = new DateOnly(2026, 4, 9),
                Numero = 1,
                EsVisibleEnApp = true,
                Jornadas =
                [
                    new JornadaDTO
                    {
                        Tipo = "Normal",
                        ResultadosVerificados = false,
                        LocalId = e1,
                        VisitanteId = e2
                    },
                    new JornadaDTO
                    {
                        Tipo = "Interzonal",
                        ResultadosVerificados = false,
                        EquipoId = e3,
                        LocalOVisitante = LocalVisitanteEnum.Local
                    }
                ]
            }
        };

        var response = await client.PostAsJsonAsync(
            $"/api/Zona/{zonaId}/fechas/crear-fechas-todoscontratodos-masivamente", dtos, FechaJsonOptions);
        response.EnsureSuccessStatusCode();

        var creados = DeserializeFechaTctList(await response.Content.ReadAsStringAsync());
        Assert.NotNull(creados);
        Assert.Single(creados);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var jornadas = await context.Jornadas.Where(j => j.FechaId == creados[0].Id).ToListAsync();
            Assert.Equal(2, jornadas.Count);
            var normal = jornadas.OfType<JornadaNormal>().Single();
            var interzonal = jornadas.OfType<JornadaInterzonal>().Single();
            Assert.True(
                normal.Id < interzonal.Id,
                $"Se esperaba Id de jornada Normal ({normal.Id}) < Id de Interzonal ({interzonal.Id}), " +
                "coincidiendo con el orden del array enviado (Normal primero, Interzonal segundo).");
        }
    }

    [Fact]
    public async Task CrearFechasMasivamente_CincoCategoriasYTresJornadas_CreaQuincePartidos()
    {
        Assert.NotNull(_club);
        var zonaId = await CrearZonaDePrueba(Factory);
        var torneoId = await ObtenerTorneoIdDeZona(Factory, zonaId);
        await SeedTorneoCategorias(Factory, torneoId, 5);
        var client = await GetAuthenticatedClient();

        int e1, e2;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var equipos = context.Equipos.Where(e => e.ClubId == _club.Id).ToList();
            e1 = equipos[0].Id;
            if (equipos.Count < 2)
            {
                var eq2 = new Equipo { Id = 0, Nombre = "Eq 15", ClubId = _club.Id, Jugadores = [] };
                context.Equipos.Add(eq2);
                context.SaveChanges();
                e2 = eq2.Id;
            }
            else
            {
                e2 = equipos[1].Id;
            }
        }

        var dtos = new List<FechaTodosContraTodosDTO>
        {
            new()
            {
                Dia = new DateOnly(2026, 4, 1),
                Numero = 1,
                EsVisibleEnApp = true,
                Jornadas =
                [
                    new JornadaDTO { Tipo = "Normal", ResultadosVerificados = false, LocalId = e1, VisitanteId = e2 },
                    new JornadaDTO { Tipo = "Libre", ResultadosVerificados = false, EquipoLocalId = e1 },
                    new JornadaDTO { Tipo = "Normal", ResultadosVerificados = false, LocalId = e2, VisitanteId = e1 }
                ]
            }
        };

        var response = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas/crear-fechas-todoscontratodos-masivamente", dtos, FechaJsonOptions);
        response.EnsureSuccessStatusCode();
        var creados = DeserializeFechaTctList(await response.Content.ReadAsStringAsync());
        Assert.NotNull(creados);
        Assert.Single(creados);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var jornadaIds = await context.Jornadas.Where(j => j.FechaId == creados[0].Id).Select(j => j.Id).ToListAsync();
            Assert.Equal(3, jornadaIds.Count);
            var total = await context.Partidos.CountAsync(p => jornadaIds.Contains(p.JornadaId));
            Assert.Equal(15, total); // 5 categorías × 3 jornadas
        }
    }

    [Fact]
    public async Task ModificarFechasMasivamente_ConJornadas_EliminaCreaModificaJornadas()
    {
        Assert.NotNull(_club);
        var zonaId = await CrearZonaDePrueba(Factory);
        var torneoId = await ObtenerTorneoIdDeZona(Factory, zonaId);
        await SeedTorneoCategorias(Factory, torneoId, 2);
        var client = await GetAuthenticatedClient();

        int equipo1Id;
        int equipo2Id;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var equipos = context.Equipos.Where(e => e.ClubId == _club.Id).ToList();
            equipo1Id = equipos[0].Id;
            if (equipos.Count < 2)
            {
                var eq2 = new Equipo { Id = 0, Nombre = "Equipo 2 Jornadas", ClubId = _club.Id, Jugadores = [] };
                context.Equipos.Add(eq2);
                context.SaveChanges();
                equipo2Id = eq2.Id;
            }
            else
            {
                equipo2Id = equipos[1].Id;
            }
        }

        var postResponse = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas/crear-fechas-todoscontratodos-masivamente",
            new List<FechaTodosContraTodosDTO>
            {
                new()
                {
                    Dia = new DateOnly(2026, 3, 1),
                    Numero = 1,
                    EsVisibleEnApp = true,
                    Jornadas =
                    [
                        new JornadaDTO { Tipo = "Normal", ResultadosVerificados = false, LocalId = equipo1Id, VisitanteId = equipo2Id },
                        new JornadaDTO { Tipo = "Libre", ResultadosVerificados = false, EquipoLocalId = equipo1Id }
                    ]
                }
            }, FechaJsonOptions);
        postResponse.EnsureSuccessStatusCode();
        var creados = DeserializeFechaTctList(await postResponse.Content.ReadAsStringAsync())!;
        var fechaId = creados[0].Id;
        var jornadaNormalId = creados[0].Jornadas!.First(j => j.Tipo == "Normal").Id;

        var dtosModificar = new List<FechaDTO>
        {
            new FechaTodosContraTodosDTO
            {
                Id = fechaId,
                Dia = new DateOnly(2026, 3, 1),
                Numero = 1,
                ZonaId = zonaId,
                EsVisibleEnApp = true,
                Jornadas =
                [
                    new JornadaDTO { Id = jornadaNormalId, Tipo = "Normal", ResultadosVerificados = true, LocalId = equipo1Id, VisitanteId = equipo2Id },
                    new JornadaDTO { Tipo = "Libre", ResultadosVerificados = false, EquipoLocalId = equipo2Id }
                ]
            }
        };

        var putResponse = await client.PutAsJsonAsync($"/api/Zona/{zonaId}/fechas/modificar-fechas-masivamente", dtosModificar, FechaJsonOptions);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, putResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/Zona/{zonaId}/fechas/{fechaId}");
        getResponse.EnsureSuccessStatusCode();
        var fecha = DeserializeFechaTct(await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(fecha);
        Assert.NotNull(fecha.Jornadas);
        Assert.Equal(2, fecha.Jornadas.Count);

        var jornadaModificada = fecha.Jornadas.FirstOrDefault(j => j.Id == jornadaNormalId);
        Assert.NotNull(jornadaModificada);
        Assert.True(jornadaModificada.ResultadosVerificados);

        var jornadaLibre = fecha.Jornadas.FirstOrDefault(j => j.Tipo == "Libre");
        Assert.NotNull(jornadaLibre);
        Assert.Equal(equipo2Id, jornadaLibre.EquipoLocalId);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var jornadas = context.Jornadas.Where(j => j.FechaId == fechaId).ToList();
            Assert.Equal(2, jornadas.Count);
            var jornadaIds = jornadas.Select(j => j.Id).ToList();
            var partidos = context.Partidos.Where(p => jornadaIds.Contains(p.JornadaId)).ToList();
            Assert.Equal(4, partidos.Count); // 2 categorías × 2 jornadas
        }
    }

    [Fact]
    public async Task ModificarFecha_EliminaJornada_EliminaPartidosDeEsaJornada()
    {
        Assert.NotNull(_club);
        var zonaId = await CrearZonaDePrueba(Factory);
        var torneoId = await ObtenerTorneoIdDeZona(Factory, zonaId);
        await SeedTorneoCategorias(Factory, torneoId, 2);
        var client = await GetAuthenticatedClient();

        int equipo1Id;
        int equipo2Id;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var equipos = context.Equipos.Where(e => e.ClubId == _club.Id).ToList();
            equipo1Id = equipos[0].Id;
            if (equipos.Count < 2)
            {
                var eq2 = new Equipo { Id = 0, Nombre = "Eq del part", ClubId = _club.Id, Jugadores = [] };
                context.Equipos.Add(eq2);
                context.SaveChanges();
                equipo2Id = eq2.Id;
            }
            else
            {
                equipo2Id = equipos[1].Id;
            }
        }

        var postResponse = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas/crear-fechas-todoscontratodos-masivamente",
            new List<FechaTodosContraTodosDTO>
            {
                new()
                {
                    Dia = new DateOnly(2026, 5, 1),
                    Numero = 1,
                    EsVisibleEnApp = true,
                    Jornadas =
                    [
                        new JornadaDTO { Tipo = "Normal", ResultadosVerificados = false, LocalId = equipo1Id, VisitanteId = equipo2Id },
                        new JornadaDTO { Tipo = "Libre", ResultadosVerificados = false, EquipoLocalId = equipo1Id }
                    ]
                }
            }, FechaJsonOptions);
        postResponse.EnsureSuccessStatusCode();
        var creados = DeserializeFechaTctList(await postResponse.Content.ReadAsStringAsync())!;
        var fechaId = creados[0].Id;
        var jornadaNormalId = creados[0].Jornadas!.First(j => j.Tipo == "Normal").Id;
        var jornadaLibreId = creados[0].Jornadas!.First(j => j.Tipo == "Libre").Id;

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(2, await context.Partidos.CountAsync(p => p.JornadaId == jornadaLibreId));
        }

        var dtoModificar = new FechaTodosContraTodosDTO
        {
            Id = fechaId,
            Dia = new DateOnly(2026, 5, 1),
            Numero = 1,
            ZonaId = zonaId,
            EsVisibleEnApp = true,
            Jornadas =
            [
                new JornadaDTO
                {
                    Id = jornadaNormalId,
                    Tipo = "Normal",
                    ResultadosVerificados = false,
                    LocalId = equipo1Id,
                    VisitanteId = equipo2Id
                }
            ]
        };

        var putResponse = await client.PutAsJsonAsync($"/api/Zona/{zonaId}/fechas/{fechaId}", dtoModificar, FechaJsonOptions);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, putResponse.StatusCode);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.False(await context.Jornadas.AnyAsync(j => j.Id == jornadaLibreId));
            Assert.Equal(0, await context.Partidos.CountAsync(p => p.JornadaId == jornadaLibreId));
            var jornadaIdsRestantes = await context.Jornadas.Where(j => j.FechaId == fechaId).Select(j => j.Id).ToListAsync();
            Assert.Single(jornadaIdsRestantes);
            Assert.Equal(2, await context.Partidos.CountAsync(p => jornadaIdsRestantes.Contains(p.JornadaId)));
        }
    }

    [Fact]
    public async Task CrearFechasMasivamente_FechaEnFormatoISO8601ConHora_DeserializaCorrectamente()
    {
        Assert.NotNull(_club);
        var zonaId = await CrearZonaDePrueba(Factory);
        var torneoId = await ObtenerTorneoIdDeZona(Factory, zonaId);
        await SeedTorneoCategorias(Factory, torneoId, 2);
        var client = await GetAuthenticatedClient();

        int equipo1Id, equipo2Id, equipo3Id;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var equipos = context.Equipos.Where(e => e.ClubId == _club.Id).ToList();
            equipo1Id = equipos[0].Id;

            var eq2 = new Equipo { Id = 0, Nombre = "Equipo ISO 2", ClubId = _club.Id, Jugadores = [] };
            var eq3 = new Equipo { Id = 0, Nombre = "Equipo ISO 3", ClubId = _club.Id, Jugadores = [] };
            context.Equipos.AddRange(eq2, eq3);
            context.SaveChanges();
            equipo2Id = eq2.Id;
            equipo3Id = eq3.Id;
        }

        // Simula el payload que envía el frontend: fechas en formato ISO 8601 con hora y zona horaria
        var json = $$"""
            [
                {
                    "numero": 1,
                    "dia": "2026-03-21T03:00:00.000Z",
                    "esVisibleEnApp": false,
                    "jornadas": [
                        { "tipo": "Normal", "resultadosVerificados": false, "localId": "{{equipo1Id}}", "visitanteId": "{{equipo2Id}}" },
                        { "tipo": "Libre", "resultadosVerificados": false, "equipoLocalId": "{{equipo3Id}}" }
                    ]
                },
                {
                    "numero": 2,
                    "dia": "2026-03-28T03:00:00.000Z",
                    "esVisibleEnApp": false,
                    "jornadas": [
                        { "tipo": "Normal", "resultadosVerificados": false, "localId": "{{equipo1Id}}", "visitanteId": "{{equipo3Id}}" },
                        { "tipo": "Libre", "resultadosVerificados": false, "equipoLocalId": "{{equipo2Id}}" }
                    ]
                }
            ]
            """;

        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/api/Zona/{zonaId}/fechas/crear-fechas-todoscontratodos-masivamente", content);
        response.EnsureSuccessStatusCode();

        var creados = DeserializeFechaTctList(await response.Content.ReadAsStringAsync());
        Assert.NotNull(creados);
        Assert.Equal(2, creados.Count);
        Assert.Contains(creados, f => f.Numero == 1 && f.Dia == new DateOnly(2026, 3, 21));
        Assert.Contains(creados, f => f.Numero == 2 && f.Dia == new DateOnly(2026, 3, 28));
        Assert.All(creados, f => Assert.Equal(2, f.Jornadas!.Count));

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var fechaIds = creados.Select(f => f.Id).ToList();
            var jornadaIds = await context.Jornadas.Where(j => fechaIds.Contains(j.FechaId)).Select(j => j.Id).ToListAsync();
            Assert.Equal(4, jornadaIds.Count);
            var totalPartidos = await context.Partidos.CountAsync(p => jornadaIds.Contains(p.JornadaId));
            Assert.Equal(8, totalPartidos); // 2 fechas × 2 jornadas × 2 categorías
        }
    }

    [Fact]
    public async Task CrearFechasEliminacionDirectaMasivamente_SinNumero_200()
    {
        var zonaId = await CrearZonaEliminacionDirectaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var dto = new FechaEliminacionDirectaDTO
        {
            Dia = new DateOnly(2026, 6, 1),
            EsVisibleEnApp = true,
            InstanciaId = 16
        };

        var response = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas/crear-fechas-eliminaciondirecta-masivamente", dto, FechaJsonOptions);
        response.EnsureSuccessStatusCode();

        var creado = DeserializeFechaEd(await response.Content.ReadAsStringAsync());
        Assert.Equal(16, creado.InstanciaId);
        Assert.Equal(new DateOnly(2026, 6, 1), creado.Dia);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(4, await context.Fechas.CountAsync(f => f.ZonaId == zonaId));
        }
    }

    [Fact]
    public async Task CrearFechasEliminacionDirectaMasivamente_CadaJornadaTienePartido_IncluidaInstanciaFinal()
    {
        var zonaId = await CrearZonaEliminacionDirectaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var dto = new FechaEliminacionDirectaDTO
        {
            Dia = new DateOnly(2026, 6, 1),
            EsVisibleEnApp = true,
            InstanciaId = 16
        };

        var response = await client.PostAsJsonAsync(
            $"/api/Zona/{zonaId}/fechas/crear-fechas-eliminaciondirecta-masivamente", dto, FechaJsonOptions);
        response.EnsureSuccessStatusCode();

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var categoriaZonaId = await context.Zonas.OfType<ZonaEliminacionDirecta>()
            .Where(z => z.Id == zonaId)
            .Select(z => z.CategoriaId)
            .FirstAsync();

        var fechaIds = await context.Fechas.Where(f => f.ZonaId == zonaId).Select(f => f.Id).ToListAsync();
        var jornadas = await context.Jornadas.Where(j => fechaIds.Contains(j.FechaId)).ToListAsync();
        Assert.NotEmpty(jornadas);

        var fechaFinal = await context.Fechas.OfType<FechaEliminacionDirecta>()
            .FirstAsync(f => f.ZonaId == zonaId && f.InstanciaId == 2);
        Assert.Contains(jornadas, j => j.FechaId == fechaFinal.Id);

        foreach (var j in jornadas)
        {
            var partidosDeJornada = await context.Partidos
                .Where(p => p.JornadaId == j.Id && p.CategoriaId == categoriaZonaId)
                .CountAsync();
            Assert.Equal(1, partidosDeJornada);
        }
    }

    [Fact]
    public async Task PropagarEliminacionDirecta_CuartosASemifinal_SemifinalTienePartidosTrasCargarResultados()
    {
        Assert.NotNull(_club);
        var zonaId = await CrearZonaEliminacionDirectaDePrueba(Factory);
        int categoriaZonaId;
        var equiposIds = new int[8];
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            categoriaZonaId = await context.Zonas.OfType<ZonaEliminacionDirecta>()
                .Where(z => z.Id == zonaId)
                .Select(z => z.CategoriaId)
                .FirstAsync();
            var equipos = Enumerable.Range(0, 8)
                .Select(i => new Equipo
                {
                    Id = 0,
                    Nombre = $"ED Prop {i}",
                    ClubId = _club.Id,
                    Jugadores = [],
                    Zonas = []
                })
                .ToList();
            context.Equipos.AddRange(equipos);
            await context.SaveChangesAsync();
            for (var i = 0; i < 8; i++)
                equiposIds[i] = equipos[i].Id;
            context.EquipoZona.AddRange(equipos.Select(e => new EquipoZona { Id = 0, EquipoId = e.Id, ZonaId = zonaId }));
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var dto = new FechaEliminacionDirectaDTO
        {
            Dia = new DateOnly(2026, 7, 1),
            EsVisibleEnApp = true,
            InstanciaId = 8,
            Jornadas =
            [
                new JornadaDTO
                {
                    Tipo = "Normal",
                    ResultadosVerificados = false,
                    LocalId = equiposIds[0],
                    VisitanteId = equiposIds[1]
                },
                new JornadaDTO
                {
                    Tipo = "Normal",
                    ResultadosVerificados = false,
                    LocalId = equiposIds[2],
                    VisitanteId = equiposIds[3]
                },
                new JornadaDTO
                {
                    Tipo = "Normal",
                    ResultadosVerificados = false,
                    LocalId = equiposIds[4],
                    VisitanteId = equiposIds[5]
                },
                new JornadaDTO
                {
                    Tipo = "Normal",
                    ResultadosVerificados = false,
                    LocalId = equiposIds[6],
                    VisitanteId = equiposIds[7]
                }
            ]
        };

        var response = await client.PostAsJsonAsync(
            $"/api/Zona/{zonaId}/fechas/crear-fechas-eliminaciondirecta-masivamente", dto, FechaJsonOptions);
        response.EnsureSuccessStatusCode();

        List<int> jornadasCuartosIds;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var fechaCuartosId = await context.Fechas.OfType<FechaEliminacionDirecta>()
                .Where(f => f.ZonaId == zonaId && f.InstanciaId == 8)
                .Select(f => f.Id)
                .FirstAsync();
            jornadasCuartosIds = await context.Jornadas
                .Where(j => j.FechaId == fechaCuartosId)
                .OrderBy(j => j.Id)
                .Select(j => j.Id)
                .ToListAsync();
            Assert.Equal(4, jornadasCuartosIds.Count);
        }

        foreach (var jornadaId in jornadasCuartosIds)
        {
            int partidoId;
            using (var scope = Factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                partidoId = await context.Partidos.Where(p => p.JornadaId == jornadaId).Select(p => p.Id).FirstAsync();
            }

            var cargar = new CargarResultadosDTO
            {
                JornadaId = jornadaId,
                ResultadosVerificados = true,
                Partidos =
                [
                    new PartidoDTO
                    {
                        Id = partidoId,
                        CategoriaId = categoriaZonaId,
                        ResultadoLocal = "1",
                        ResultadoVisitante = "0"
                    }
                ]
            };
            var postRes = await client.PostAsJsonAsync(
                $"/api/Zona/{zonaId}/fechas/cargar-resultados/{jornadaId}", cargar, FechaJsonOptions);
            postRes.EnsureSuccessStatusCode();
        }

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var fechaSemiId = await context.Fechas.OfType<FechaEliminacionDirecta>()
                .Where(f => f.ZonaId == zonaId && f.InstanciaId == 4)
                .Select(f => f.Id)
                .FirstAsync();
            var jornadasSemi = await context.Jornadas.Where(j => j.FechaId == fechaSemiId).OrderBy(j => j.Id).ToListAsync();
            Assert.Equal(2, jornadasSemi.Count);
            Assert.All(jornadasSemi, j => Assert.IsType<JornadaNormal>(j));
            foreach (var j in jornadasSemi)
            {
                var n = await context.Partidos.CountAsync(p => p.JornadaId == j.Id && p.CategoriaId == categoriaZonaId);
                Assert.Equal(1, n);
            }
        }
    }

    [Fact]
    public async Task CrearFechasEliminacionDirectaMasivamente_ConVariasCategoriasEnTorneo_UnSoloPartidoPorJornada()
    {
        Assert.NotNull(_club);
        var zonaId = await CrearZonaEliminacionDirectaDePrueba(Factory);
        var torneoId = await ObtenerTorneoIdDeZona(Factory, zonaId);
        await SeedTorneoCategorias(Factory, torneoId, 2);

        int categoriaZonaId;
        int equipo1Id, equipo2Id;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            categoriaZonaId = await context.Zonas.OfType<ZonaEliminacionDirecta>()
                .Where(z => z.Id == zonaId)
                .Select(z => z.CategoriaId)
                .FirstAsync();

            var eq1 = new Equipo { Id = 0, Nombre = "ED Eq A", ClubId = _club.Id, Jugadores = [], Zonas = [] };
            var eq2 = new Equipo { Id = 0, Nombre = "ED Eq B", ClubId = _club.Id, Jugadores = [], Zonas = [] };
            context.Equipos.AddRange(eq1, eq2);
            context.SaveChanges();
            equipo1Id = eq1.Id;
            equipo2Id = eq2.Id;
            context.EquipoZona.AddRange(
                new EquipoZona { Id = 0, EquipoId = equipo1Id, ZonaId = zonaId },
                new EquipoZona { Id = 0, EquipoId = equipo2Id, ZonaId = zonaId });
            context.SaveChanges();
        }

        var client = await GetAuthenticatedClient();
        var dto = new FechaEliminacionDirectaDTO
        {
            Dia = new DateOnly(2026, 6, 1),
            EsVisibleEnApp = true,
            InstanciaId = 16,
            Jornadas =
            [
                new JornadaDTO
                    { Tipo = "Normal", ResultadosVerificados = false, LocalId = equipo1Id, VisitanteId = equipo2Id },
                new JornadaDTO
                    { Tipo = "Normal", ResultadosVerificados = false, LocalId = equipo2Id, VisitanteId = equipo1Id }
            ]
        };

        var response = await client.PostAsJsonAsync(
            $"/api/Zona/{zonaId}/fechas/crear-fechas-eliminaciondirecta-masivamente", dto, FechaJsonOptions);
        response.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var fechaIds = await context.Fechas.Where(f => f.ZonaId == zonaId).Select(f => f.Id).ToListAsync();
            Assert.Equal(4, fechaIds.Count);
            var jornadaIds = await context.Jornadas.Where(j => fechaIds.Contains(j.FechaId)).Select(j => j.Id).ToListAsync();
            Assert.Equal(9, jornadaIds.Count);
            var partidos = await context.Partidos.Where(p => jornadaIds.Contains(p.JornadaId)).ToListAsync();
            Assert.Equal(9, partidos.Count);
            Assert.All(partidos, p => Assert.Equal(categoriaZonaId, p.CategoriaId));
        }
    }

    [Fact]
    public async Task BorrarFechasEliminacionDirectaMasivamente_ZonaEd_EliminaFechasJornadasPartidos()
    {
        var zonaId = await CrearZonaEliminacionDirectaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var dto = new FechaEliminacionDirectaDTO
        {
            Dia = new DateOnly(2026, 6, 1),
            EsVisibleEnApp = true,
            InstanciaId = 16
        };
        var post = await client.PostAsJsonAsync(
            $"/api/Zona/{zonaId}/fechas/crear-fechas-eliminaciondirecta-masivamente", dto, FechaJsonOptions);
        post.EnsureSuccessStatusCode();

        var delete = await client.DeleteAsync($"/api/Zona/{zonaId}/fechas/borrar-fechas-eliminaciondirecta-masivamente");
        delete.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, delete.StatusCode);

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(0, await context.Fechas.CountAsync(f => f.ZonaId == zonaId));
        Assert.Equal(0, await context.Jornadas.CountAsync());
        Assert.Equal(0, await context.Partidos.CountAsync());
    }

    [Fact]
    public async Task BorrarFechasEliminacionDirectaMasivamente_ZonaTodosContraTodos_400()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();
        var response = await client.DeleteAsync($"/api/Zona/{zonaId}/fechas/borrar-fechas-eliminaciondirecta-masivamente");
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
