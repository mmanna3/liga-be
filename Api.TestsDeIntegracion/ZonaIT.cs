using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Logica;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;
using Api.TestsDeIntegracion._Config;
using Api.TestsUtilidades;
using Api.TestsUtilidades;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace Api.TestsDeIntegracion;

public class ZonaIT : TestBase
{
    private Utilidades? _utilidades;
    private Club? _club;

    public ZonaIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedData(context);
    }

    private void SeedData(AppDbContext context)
    {
        _utilidades = new Utilidades(context);
        _club = _utilidades.DadoQueExisteElClub();
        _utilidades.DadoQueExisteElEquipo(_club);
        context.SaveChanges();
    }

    private static async Task<int> CrearFaseDePrueba(CustomWebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var torneo = new Torneo { Id = 0, Nombre = "Torneo Test Zonas", Anio = 2026, TorneoAgrupadorId = 1, EsVisibleEnApp = true, SeVenLosGolesEnTablaDePosiciones = true };
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
        return fase.Id;
    }

    /// <summary>
    /// Torneo con dos categorías y una fase de eliminación directa (para zonas con CategoriaId).
    /// </summary>
    private static async Task<(int faseId, int categoriaId1, int categoriaId2)> CrearFaseEliminacionDirectaConCategorias(
        CustomWebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var torneo = new Torneo { Id = 0, Nombre = "Torneo ED Zonas", Anio = 2026, TorneoAgrupadorId = 1, EsVisibleEnApp = true, SeVenLosGolesEnTablaDePosiciones = true };
        context.Torneos.Add(torneo);
        await context.SaveChangesAsync();

        var fase = new FaseEliminacionDirecta
        {
            Id = 0,
            Nombre = "Playoffs",
            Numero = 1,
            TorneoId = torneo.Id,
            EstadoFaseId = 100,
            EsVisibleEnApp = true
        };
        context.Fases.Add(fase);
        await context.SaveChangesAsync();

        var cat1 = await CategoriasDePrueba.AgregarFaseCategoria(context, fase.Id, "Cat A", 1);
        var cat2 = await CategoriasDePrueba.AgregarFaseCategoria(context, fase.Id, "Cat B", 2);

        return (fase.Id, cat1.Id, cat2.Id);
    }

    [Fact]
    public async Task ListarZonas_FaseExistente_DevuelveLista()
    {
        var faseId = await CrearFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var response = await client.GetAsync($"/api/Fase/{faseId}/zonas");

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<ZonaDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Empty(content);
    }

    [Fact]
    public async Task CrearZona_DatosCorrectos_200()
    {
        var faseId = await CrearFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var dto = new ZonaDTO { Nombre = "Zona A", Orden = 1 };

        var response = await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas", dto);

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<ZonaDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.True(content.Id > 0);
        Assert.Equal("Zona A", content.Nombre);
        Assert.Equal(faseId, content.FaseId);
    }

    [Fact]
    public async Task CrearZona_FaseInexistente_400()
    {
        var client = await GetAuthenticatedClient();

        var dto = new ZonaDTO { Nombre = "Zona A", Orden = 1 };

        var response = await client.PostAsJsonAsync("/api/Fase/99999/zonas", dto);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var mensaje = await response.Content.ReadAsStringAsync();
        Assert.Contains("fase", mensaje.ToLowerInvariant());
    }

    [Fact]
    public async Task ObtenerZona_PorId_DevuelveCorrecto()
    {
        var faseId = await CrearFaseDePrueba(Factory);
        Zona zona;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            zona = new ZonaTodosContraTodos
            {
                Id = 0,
                Nombre = "Zona B",
                FaseId = faseId,
                Orden = 1
            };
            context.Zonas.Add(zona);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/Fase/{faseId}/zonas/{zona.Id}");

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<ZonaDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(zona.Id, content.Id);
        Assert.Equal("Zona B", content.Nombre);
    }

    [Fact]
    public async Task ObtenerZona_ZonaDeOtraFase_404()
    {
        var fase1Id = await CrearFaseDePrueba(Factory);
        int zonaId;
        int fase2Id;
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
            fase2Id = fase2.Id;

            var zona = new ZonaTodosContraTodos
            {
                Id = 0,
                Nombre = "Zona B",
                FaseId = fase2Id,
                Orden = 1
            };
            context.Zonas.Add(zona);
            await context.SaveChangesAsync();
            zonaId = zona.Id;
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/Fase/{fase1Id}/zonas/{zonaId}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ModificarZona_DatosCorrectos_204()
    {
        var faseId = await CrearFaseDePrueba(Factory);
        Zona zona;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            zona = new ZonaTodosContraTodos
            {
                Id = 0,
                Nombre = "Para Modificar",
                FaseId = faseId,
                Orden = 1
            };
            context.Zonas.Add(zona);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var dto = new ZonaDTO
        {
            Id = zona.Id,
            Nombre = "Zona Modificada",
            FaseId = faseId,
            Orden = 1
        };

        var response = await client.PutAsJsonAsync($"/api/Fase/{faseId}/zonas/{zona.Id}", dto);

        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var actualizado = context.Zonas.Find(zona.Id);
            Assert.NotNull(actualizado);
            Assert.Equal("Zona Modificada", actualizado.Nombre);
        }
    }

    [Fact]
    public async Task EliminarZona_Existente_200()
    {
        var faseId = await CrearFaseDePrueba(Factory);
        Zona zona;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            zona = new ZonaTodosContraTodos
            {
                Id = 0,
                Nombre = "Para Eliminar",
                FaseId = faseId,
                Orden = 1
            };
            context.Zonas.Add(zona);
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var response = await client.DeleteAsync($"/api/Fase/{faseId}/zonas/{zona.Id}");

        response.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Null(context.Zonas.Find(zona.Id));
        }
    }

    [Fact]
    public async Task EliminarZona_ZonaDeOtraFase_404()
    {
        var fase1Id = await CrearFaseDePrueba(Factory);
        int zonaId;
        int fase2Id;
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
            fase2Id = fase2.Id;

            var zona = new ZonaTodosContraTodos
            {
                Id = 0,
                Nombre = "Zona B",
                FaseId = fase2Id,
                Orden = 1
            };
            context.Zonas.Add(zona);
            await context.SaveChangesAsync();
            zonaId = zona.Id;
        }

        var client = await GetAuthenticatedClient();
        var response = await client.DeleteAsync($"/api/Fase/{fase1Id}/zonas/{zonaId}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.NotNull(context.Zonas.Find(zonaId));
        }
    }

    [Fact]
    public async Task ListarZonas_ConZonasCreadas_DevuelveTodasDeLaFase()
    {
        var faseId = await CrearFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas", new ZonaDTO { Nombre = "Zona A", Orden = 1 });
        await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas", new ZonaDTO { Nombre = "Zona B", Orden = 2 });

        var response = await client.GetAsync($"/api/Fase/{faseId}/zonas");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<ZonaDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(2, content.Count);
        Assert.Contains(content, z => z.Nombre == "Zona A");
        Assert.Contains(content, z => z.Nombre == "Zona B");
    }

    [Fact]
    public async Task CrearZona_ConEquipos_AsignaEquiposALaZona()
    {
        var faseId = await CrearFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        int equipoId;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            equipoId = context.Equipos.First(e => e.ClubId == _club!.Id).Id;
        }

        var dto = new ZonaDTO
        {
            Nombre = "Zona con Equipos",
            Orden = 1,
            Equipos =
            [
                new EquipoDeLaZonaDTO { Id = equipoId.ToString(), Nombre = "ignorado", Club = "ignorado", Codigo = "ignorado" }
            ]
        };

        var response = await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas", dto);
        response.EnsureSuccessStatusCode();

        var creado = JsonConvert.DeserializeObject<ZonaDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(creado);
        Assert.True(creado.Id > 0);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var equipo = context.Equipos.Include(e => e.Zonas).FirstOrDefault(e => e.Id == equipoId);
            Assert.NotNull(equipo);
            Assert.True(equipo.Zonas.Any(ez => ez.ZonaId == creado.Id));
        }
    }

    [Fact]
    public async Task ObtenerZona_PorId_DevuelveEquiposCompletos()
    {
        Assert.NotNull(_club);
        var faseId = await CrearFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        int equipoId;
        string equipoNombre;
        string clubNombre;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var equipo = context.Equipos.Include(e => e.Club).FirstOrDefault(e => e.ClubId == _club.Id);
            Assert.NotNull(equipo);
            equipoId = equipo.Id;
            equipoNombre = equipo.Nombre;
            clubNombre = equipo.Club?.Nombre ?? _club.Nombre;
        }

        var dtoCrear = new ZonaDTO
        {
            Nombre = "Zona para GET",
            Orden = 1,
            Equipos = [new EquipoDeLaZonaDTO { Id = equipoId.ToString() }]
        };
        var postResponse = await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas", dtoCrear);
        postResponse.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<ZonaDTO>(await postResponse.Content.ReadAsStringAsync())!;

        var getResponse = await client.GetAsync($"/api/Fase/{faseId}/zonas/{creado.Id}");
        getResponse.EnsureSuccessStatusCode();

        var zona = JsonConvert.DeserializeObject<ZonaDTO>(await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(zona);
        Assert.NotNull(zona.Equipos);
        Assert.Single(zona.Equipos);
        Assert.Equal(equipoId.ToString(), zona.Equipos[0].Id);
        Assert.Equal(equipoNombre, zona.Equipos[0].Nombre);
        Assert.Equal(clubNombre, zona.Equipos[0].Club);
        Assert.Equal(GeneradorDeHash.GenerarAlfanumerico7Digitos(equipoId), zona.Equipos[0].Codigo);
    }

    [Fact]
    public async Task ObtenerZona_EquipoDeLaZonaTieneZonaYEquipoZona()
    {
        Assert.NotNull(_club);
        var faseId = await CrearFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        int equipoId;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var equipo = context.Equipos.FirstOrDefault(e => e.ClubId == _club.Id);
            Assert.NotNull(equipo);
            equipoId = equipo.Id;
        }

        var dtoCrear = new ZonaDTO
        {
            Nombre = "Zona Norte",
            Orden = 1,
            Equipos = [new EquipoDeLaZonaDTO { Id = equipoId.ToString() }]
        };
        var postResponse = await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas", dtoCrear);
        postResponse.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<ZonaDTO>(await postResponse.Content.ReadAsStringAsync())!;

        var getResponse = await client.GetAsync($"/api/Fase/{faseId}/zonas/{creado.Id}");
        getResponse.EnsureSuccessStatusCode();
        var zona = JsonConvert.DeserializeObject<ZonaDTO>(await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(zona);
        Assert.NotNull(zona.Equipos);
        Assert.Single(zona.Equipos);
    }

    [Fact]
    public async Task ListarZonas_DevuelveEquiposCompletos()
    {
        var faseId = await CrearFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        int equipoId;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            equipoId = context.Equipos.First(e => e.ClubId == _club!.Id).Id;
        }

        var dtoCrear = new ZonaDTO
        {
            Nombre = "Zona para Listar",
            Orden = 1,
            Equipos = [new EquipoDeLaZonaDTO { Id = equipoId.ToString() }]
        };
        await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas", dtoCrear);

        var response = await client.GetAsync($"/api/Fase/{faseId}/zonas");
        response.EnsureSuccessStatusCode();

        var zonas = JsonConvert.DeserializeObject<List<ZonaDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(zonas);
        var zonaConEquipos = zonas.FirstOrDefault(z => z.Nombre == "Zona para Listar");
        Assert.NotNull(zonaConEquipos);
        Assert.NotNull(zonaConEquipos.Equipos);
        Assert.Single(zonaConEquipos.Equipos);
        Assert.Equal(equipoId.ToString(), zonaConEquipos.Equipos[0].Id);
        Assert.NotEmpty(zonaConEquipos.Equipos[0].Nombre);
        Assert.NotEmpty(zonaConEquipos.Equipos[0].Club);
        Assert.NotEmpty(zonaConEquipos.Equipos[0].Codigo);
    }

    [Fact]
    public async Task ModificarZona_EquiposNull_NoModificaEquipos()
    {
        var faseId = await CrearFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        int equipoId;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            equipoId = context.Equipos.First(e => e.ClubId == _club!.Id).Id;
        }

        var dtoCrear = new ZonaDTO
        {
            Nombre = "Zona Original",
            Orden = 1,
            Equipos = [new EquipoDeLaZonaDTO { Id = equipoId.ToString() }]
        };
        var postResponse = await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas", dtoCrear);
        postResponse.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<ZonaDTO>(await postResponse.Content.ReadAsStringAsync())!;

        var dtoModificar = new ZonaDTO
        {
            Id = creado.Id,
            Nombre = "Zona Renombrada",
            FaseId = faseId,
            Orden = 1
            // Equipos = null (no enviar, no modificar)
        };

        var putResponse = await client.PutAsJsonAsync($"/api/Fase/{faseId}/zonas/{creado.Id}", dtoModificar);
        putResponse.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var equipo = context.Equipos.Include(e => e.Zonas).FirstOrDefault(e => e.Id == equipoId);
            Assert.NotNull(equipo);
            Assert.True(equipo.Zonas.Any(ez => ez.ZonaId == creado.Id));
        }
    }

    [Fact]
    public async Task ModificarZona_EquiposVacio_BorraTodosLosEquipos()
    {
        var faseId = await CrearFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        int equipoId;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            equipoId = context.Equipos.First(e => e.ClubId == _club!.Id).Id;
        }

        var dtoCrear = new ZonaDTO
        {
            Nombre = "Zona para Vaciar",
            Orden = 1,
            Equipos = [new EquipoDeLaZonaDTO { Id = equipoId.ToString() }]
        };
        var postResponse = await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas", dtoCrear);
        postResponse.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<ZonaDTO>(await postResponse.Content.ReadAsStringAsync())!;

        var dtoModificar = new ZonaDTO
        {
            Id = creado.Id,
            Nombre = creado.Nombre,
            FaseId = faseId,
            Orden = creado.Orden,
            Equipos = []
        };

        var putResponse = await client.PutAsJsonAsync($"/api/Fase/{faseId}/zonas/{creado.Id}", dtoModificar);
        putResponse.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var equipo = context.Equipos.Include(e => e.Zonas).FirstOrDefault(e => e.Id == equipoId);
            Assert.NotNull(equipo);
            Assert.False(equipo.Zonas.Any(ez => ez.ZonaId == creado.Id));
        }
    }

    [Fact]
    public async Task ModificarZona_EquiposConItems_ReemplazaEquipos()
    {
        var faseId = await CrearFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        int equipo1Id;
        int equipo2Id;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var equipos = context.Equipos.Where(e => e.ClubId == _club!.Id).ToList();
            equipo1Id = equipos[0].Id;
            if (equipos.Count < 2)
            {
                var eq2 = new Equipo { Id = 0, Nombre = "Equipo 2", ClubId = _club!.Id, Jugadores = [], Zonas = new List<EquipoZona>() };
                context.Equipos.Add(eq2);
                context.SaveChanges();
                equipo2Id = eq2.Id;
            }
            else
            {
                equipo2Id = equipos[1].Id;
            }
        }

        var dtoCrear = new ZonaDTO
        {
            Nombre = "Zona para Reemplazar",
            Orden = 1,
            Equipos = [new EquipoDeLaZonaDTO { Id = equipo1Id.ToString() }]
        };
        var postResponse = await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas", dtoCrear);
        postResponse.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<ZonaDTO>(await postResponse.Content.ReadAsStringAsync())!;

        var dtoModificar = new ZonaDTO
        {
            Id = creado.Id,
            Nombre = creado.Nombre,
            FaseId = faseId,
            Orden = creado.Orden,
            Equipos = [new EquipoDeLaZonaDTO { Id = equipo2Id.ToString() }]
        };

        var putResponse = await client.PutAsJsonAsync($"/api/Fase/{faseId}/zonas/{creado.Id}", dtoModificar);
        putResponse.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var eq1 = context.Equipos.Include(e => e.Zonas).FirstOrDefault(e => e.Id == equipo1Id);
            var eq2 = context.Equipos.Include(e => e.Zonas).FirstOrDefault(e => e.Id == equipo2Id);
            Assert.NotNull(eq1);
            Assert.NotNull(eq2);
            Assert.False(eq1.Zonas.Any(ez => ez.ZonaId == creado.Id));
            Assert.True(eq2.Zonas.Any(ez => ez.ZonaId == creado.Id));
        }
    }

    [Fact]
    public async Task CrearZonasMasivamente_DatosCorrectos_200()
    {
        Assert.NotNull(_club);
        var faseId = await CrearFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        int equipo1Id;
        int equipo2Id;
        string equipo1Nombre;
        string clubNombre;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var equipos = context.Equipos.Where(e => e.ClubId == _club.Id).ToList();
            equipo1Id = equipos[0].Id;
            equipo1Nombre = equipos[0].Nombre;
            clubNombre = equipos[0].Club?.Nombre ?? _club.Nombre;
            if (equipos.Count < 2)
            {
                var eq2 = new Equipo { Id = 0, Nombre = "Equipo Masivo 2", ClubId = _club.Id, Jugadores = [] };
                context.Equipos.Add(eq2);
                context.SaveChanges();
                equipo2Id = eq2.Id;
            }
            else
            {
                equipo2Id = equipos[1].Id;
            }
        }

        var dtos = new List<ZonaDTO>
        {
            new() { Nombre = "Zona A", Orden = 1, Equipos = [new EquipoDeLaZonaDTO { Id = equipo1Id.ToString() }] },
            new() { Nombre = "Zona B", Orden = 2 },
            new() { Nombre = "Zona C", Orden = 3, Equipos = [new EquipoDeLaZonaDTO { Id = equipo2Id.ToString() }] }
        };

        var response = await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas/crear-zonas-masivamente", dtos);
        response.EnsureSuccessStatusCode();

        var creados = JsonConvert.DeserializeObject<List<ZonaDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(creados);
        Assert.Equal(3, creados.Count);
        Assert.All(creados, z => Assert.True(z.Id > 0));
        Assert.Contains(creados, z => z.Nombre == "Zona A");
        Assert.Contains(creados, z => z.Nombre == "Zona B");
        Assert.Contains(creados, z => z.Nombre == "Zona C");

        var zonaA = creados.First(z => z.Nombre == "Zona A");
        Assert.NotNull(zonaA.Equipos);
        Assert.Single(zonaA.Equipos);
        Assert.Equal(equipo1Id.ToString(), zonaA.Equipos[0].Id);
        Assert.Equal(equipo1Nombre, zonaA.Equipos[0].Nombre);
        Assert.Equal(clubNombre, zonaA.Equipos[0].Club);
        Assert.Equal(GeneradorDeHash.GenerarAlfanumerico7Digitos(equipo1Id), zonaA.Equipos[0].Codigo);

        var getResponse = await client.GetAsync($"/api/Fase/{faseId}/zonas/{zonaA.Id}");
        getResponse.EnsureSuccessStatusCode();
        var zonaAGet = JsonConvert.DeserializeObject<ZonaDTO>(await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(zonaAGet);
        Assert.NotNull(zonaAGet.Equipos);
        Assert.Single(zonaAGet.Equipos);
        Assert.Equal(equipo1Id.ToString(), zonaAGet.Equipos[0].Id);
        Assert.Equal(equipo1Nombre, zonaAGet.Equipos[0].Nombre);
        Assert.Equal(clubNombre, zonaAGet.Equipos[0].Club);
        Assert.NotEmpty(zonaAGet.Equipos[0].Codigo);
    }

    [Fact]
    public async Task CrearZonasMasivamente_FaseEliminacionDirecta_ConCategorias_200()
    {
        var (faseId, cat1Id, cat2Id) = await CrearFaseEliminacionDirectaConCategorias(Factory);
        var client = await GetAuthenticatedClient();

        var dtos = new List<ZonaDTO>
        {
            new() { Nombre = "Zona Cat A", Orden = 1, CategoriaId = cat1Id },
            new() { Nombre = "Zona Cat B", Orden = 2, CategoriaId = cat2Id }
        };

        var response = await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas/crear-zonas-masivamente", dtos);
        response.EnsureSuccessStatusCode();

        var creados = JsonConvert.DeserializeObject<List<ZonaDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(creados);
        Assert.Equal(2, creados.Count);
        Assert.Contains(creados, z => z.Nombre == "Zona Cat A" && z.CategoriaId == cat1Id);
        Assert.Contains(creados, z => z.Nombre == "Zona Cat B" && z.CategoriaId == cat2Id);
    }

    [Fact]
    public async Task ModificarZona_ConEquipoExistente_AgregaOtroEquipo_ConservaAmbos()
    {
        Assert.NotNull(_club);
        var faseId = await CrearFaseDePrueba(Factory);
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
                var eq2 = new Equipo { Id = 0, Nombre = "Equipo 2", ClubId = _club.Id, Jugadores = [] };
                context.Equipos.Add(eq2);
                context.SaveChanges();
                equipo2Id = eq2.Id;
            }
            else
            {
                equipo2Id = equipos[1].Id;
            }
        }

        var dtoCrear = new ZonaDTO
        {
            Nombre = "Zona Con Un Equipo",
            Orden = 1,
            Equipos = [new EquipoDeLaZonaDTO { Id = equipo1Id.ToString() }]
        };
        var postResponse = await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas", dtoCrear);
        postResponse.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<ZonaDTO>(await postResponse.Content.ReadAsStringAsync())!;

        var dtoModificar = new ZonaDTO
        {
            Id = creado.Id,
            Nombre = creado.Nombre,
            FaseId = faseId,
            Orden = creado.Orden,
            Equipos =
            [
                new EquipoDeLaZonaDTO { Id = equipo1Id.ToString() },
                new EquipoDeLaZonaDTO { Id = equipo2Id.ToString() }
            ]
        };

        var putResponse = await client.PutAsJsonAsync($"/api/Fase/{faseId}/zonas/{creado.Id}", dtoModificar);
        putResponse.EnsureSuccessStatusCode();

        var getResponse = await client.GetAsync($"/api/Fase/{faseId}/zonas/{creado.Id}");
        getResponse.EnsureSuccessStatusCode();
        var zona = JsonConvert.DeserializeObject<ZonaDTO>(await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(zona);
        Assert.NotNull(zona.Equipos);
        Assert.Equal(2, zona.Equipos.Count);
        Assert.Contains(zona.Equipos, e => e.Id == equipo1Id.ToString());
        Assert.Contains(zona.Equipos, e => e.Id == equipo2Id.ToString());
    }

    [Fact]
    public async Task ModificarZonasMasivamente_DatosCorrectos_204()
    {
        Assert.NotNull(_club);
        var faseId = await CrearFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        int equipo1Id;
        int equipo2Id;
        string equipo1Nombre;
        string equipo2Nombre;
        string clubNombre;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var equipos = context.Equipos.Where(e => e.ClubId == _club.Id).ToList();
            equipo1Id = equipos[0].Id;
            equipo1Nombre = equipos[0].Nombre;
            clubNombre = equipos[0].Club?.Nombre ?? _club.Nombre;
            if (equipos.Count < 2)
            {
                var eq2 = new Equipo { Id = 0, Nombre = "Equipo Mod Masivo 2", ClubId = _club.Id, Jugadores = [] };
                context.Equipos.Add(eq2);
                context.SaveChanges();
                equipo2Id = eq2.Id;
                equipo2Nombre = eq2.Nombre;
            }
            else
            {
                equipo2Id = equipos[1].Id;
                equipo2Nombre = equipos[1].Nombre;
            }
        }

        var postResponse = await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas/crear-zonas-masivamente",
            new List<ZonaDTO>
            {
                new() { Nombre = "Zona 1", Orden = 1 },
                new() { Nombre = "Zona 2", Orden = 2 }
            });
        postResponse.EnsureSuccessStatusCode();
        var creados = JsonConvert.DeserializeObject<List<ZonaDTO>>(await postResponse.Content.ReadAsStringAsync())!;

        var dtosModificar = new List<ZonaDTO>
        {
            new()
            {
                Id = creados[0].Id,
                Nombre = "Zona Uno",
                FaseId = faseId,
                Orden = 1,
                Equipos = [new EquipoDeLaZonaDTO { Id = equipo1Id.ToString() }]
            },
            new()
            {
                Id = creados[1].Id,
                Nombre = "Zona Dos",
                FaseId = faseId,
                Orden = 2,
                Equipos = [new EquipoDeLaZonaDTO { Id = equipo2Id.ToString() }]
            }
        };

        var putResponse = await client.PutAsJsonAsync($"/api/Fase/{faseId}/zonas/modificar-zonas-masivamente", dtosModificar);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, putResponse.StatusCode);

        var getZona1 = await client.GetAsync($"/api/Fase/{faseId}/zonas/{creados[0].Id}");
        getZona1.EnsureSuccessStatusCode();
        var zona1 = JsonConvert.DeserializeObject<ZonaDTO>(await getZona1.Content.ReadAsStringAsync());
        Assert.NotNull(zona1);
        Assert.Equal("Zona Uno", zona1.Nombre);
        Assert.NotNull(zona1.Equipos);
        Assert.Single(zona1.Equipos);
        Assert.Equal(equipo1Id.ToString(), zona1.Equipos[0].Id);
        Assert.Equal(equipo1Nombre, zona1.Equipos[0].Nombre);
        Assert.Equal(clubNombre, zona1.Equipos[0].Club);
        Assert.NotEmpty(zona1.Equipos[0].Codigo);

        var getZona2 = await client.GetAsync($"/api/Fase/{faseId}/zonas/{creados[1].Id}");
        getZona2.EnsureSuccessStatusCode();
        var zona2 = JsonConvert.DeserializeObject<ZonaDTO>(await getZona2.Content.ReadAsStringAsync());
        Assert.NotNull(zona2);
        Assert.Equal("Zona Dos", zona2.Nombre);
        Assert.NotNull(zona2.Equipos);
        Assert.Single(zona2.Equipos);
        Assert.Equal(equipo2Id.ToString(), zona2.Equipos[0].Id);
        Assert.Equal(equipo2Nombre, zona2.Equipos[0].Nombre);
        Assert.Equal(clubNombre, zona2.Equipos[0].Club);
        Assert.NotEmpty(zona2.Equipos[0].Codigo);
    }

    [Fact]
    public async Task ModificarZonasMasivamente_ArrayVacio_EliminaTodasLasZonas()
    {
        var faseId = await CrearFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas/crear-zonas-masivamente",
            new List<ZonaDTO>
            {
                new() { Nombre = "Zona A", Orden = 1 },
                new() { Nombre = "Zona B", Orden = 2 }
            });

        var putResponse = await client.PutAsJsonAsync($"/api/Fase/{faseId}/zonas/modificar-zonas-masivamente", new List<ZonaDTO>());
        Assert.Equal(System.Net.HttpStatusCode.NoContent, putResponse.StatusCode);

        var listResponse = await client.GetAsync($"/api/Fase/{faseId}/zonas");
        listResponse.EnsureSuccessStatusCode();
        var zonas = JsonConvert.DeserializeObject<List<ZonaDTO>>(await listResponse.Content.ReadAsStringAsync());
        Assert.NotNull(zonas);
        Assert.Empty(zonas);
    }

    [Fact]
    public async Task ModificarMasivamente_ZonaConUnEquipo_EnviaDosEquipos_GuardaAmbos()
    {
        Assert.NotNull(_club);
        var faseId = await CrearFaseDePrueba(Factory);
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
                var eq2 = new Equipo { Id = 0, Nombre = "Equipo 2", ClubId = _club.Id, Jugadores = [] };
                context.Equipos.Add(eq2);
                context.SaveChanges();
                equipo2Id = eq2.Id;
            }
            else
            {
                equipo2Id = equipos[1].Id;
            }
        }

        var postResponse = await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas/crear-zonas-masivamente",
            new List<ZonaDTO>
            {
                new() { Nombre = "Zona única", Orden = 1, Equipos = [new EquipoDeLaZonaDTO { Id = equipo1Id.ToString() }] }
            });
        postResponse.EnsureSuccessStatusCode();
        var creados = JsonConvert.DeserializeObject<List<ZonaDTO>>(await postResponse.Content.ReadAsStringAsync())!;
        var zonaId = creados[0].Id;

        var getAntes = await client.GetAsync($"/api/Fase/{faseId}/zonas/{zonaId}");
        var zonaAntes = JsonConvert.DeserializeObject<ZonaDTO>(await getAntes.Content.ReadAsStringAsync());
        Assert.NotNull(zonaAntes?.Equipos);
        Assert.Single(zonaAntes.Equipos);

        var dtosModificar = new List<ZonaDTO>
        {
            new()
            {
                Id = zonaId,
                Nombre = "Zona única",
                FaseId = faseId,
                Orden = 1,
                Equipos =
                [
                    new EquipoDeLaZonaDTO { Id = equipo2Id.ToString(), Nombre = "River", Club = "River Plate", Codigo = "X" },
                    new EquipoDeLaZonaDTO { Id = equipo1Id.ToString(), Nombre = "Boquita", Club = "Boca Jrs", Codigo = "X" }
                ]
            }
        };

        var putResponse = await client.PutAsJsonAsync($"/api/Fase/{faseId}/zonas/modificar-zonas-masivamente", dtosModificar);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, putResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/Fase/{faseId}/zonas");
        getResponse.EnsureSuccessStatusCode();
        var zonas = JsonConvert.DeserializeObject<List<ZonaDTO>>(await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(zonas);
        var zona = zonas.First(z => z.Id == zonaId);
        Assert.NotNull(zona.Equipos);
        Assert.Equal(2, zona.Equipos.Count);
        Assert.Contains(zona.Equipos, e => e.Id == equipo1Id.ToString());
        Assert.Contains(zona.Equipos, e => e.Id == equipo2Id.ToString());
    }

    [Fact]
    public async Task ModificarZonasMasivamente_ZonasNoIncluidasEnArray_SeEliminan()
    {
        var faseId = await CrearFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var postResponse = await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas/crear-zonas-masivamente",
            new List<ZonaDTO>
            {
                new() { Nombre = "Zona A", Orden = 1 },
                new() { Nombre = "Zona B", Orden = 2 },
                new() { Nombre = "Zona C", Orden = 3 }
            });
        postResponse.EnsureSuccessStatusCode();
        var creados = JsonConvert.DeserializeObject<List<ZonaDTO>>(await postResponse.Content.ReadAsStringAsync())!;

        var dtosModificar = new List<ZonaDTO>
        {
            new() { Id = creados[0].Id, Nombre = "Zona A Modificada", FaseId = faseId, Orden = 1 },
            new() { Id = creados[2].Id, Nombre = "Zona C Modificada", FaseId = faseId, Orden = 3 }
        };

        var putResponse = await client.PutAsJsonAsync($"/api/Fase/{faseId}/zonas/modificar-zonas-masivamente", dtosModificar);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, putResponse.StatusCode);

        var listResponse = await client.GetAsync($"/api/Fase/{faseId}/zonas");
        listResponse.EnsureSuccessStatusCode();
        var zonas = JsonConvert.DeserializeObject<List<ZonaDTO>>(await listResponse.Content.ReadAsStringAsync());
        Assert.NotNull(zonas);
        Assert.Equal(2, zonas.Count);
        Assert.Contains(zonas, z => z.Nombre == "Zona A Modificada");
        Assert.Contains(zonas, z => z.Nombre == "Zona C Modificada");
        Assert.DoesNotContain(zonas, z => z.Nombre == "Zona B");
    }

    [Fact]
    public async Task ModificarMasivamente_ConZonaNuevaSinId_CreaLaZonaNueva()
    {
        Assert.NotNull(_club);
        var faseId = await CrearFaseDePrueba(Factory);
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
                var eq2 = new Equipo { Id = 0, Nombre = "Equipo 2", ClubId = _club.Id, Jugadores = [] };
                context.Equipos.Add(eq2);
                context.SaveChanges();
                equipo2Id = eq2.Id;
            }
            else
            {
                equipo2Id = equipos[1].Id;
            }
        }

        var postResponse = await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas/crear-zonas-masivamente",
            new List<ZonaDTO>
            {
                new() { Nombre = "Zona existente", Orden = 1, Equipos = [new EquipoDeLaZonaDTO { Id = equipo1Id.ToString() }] }
            });
        postResponse.EnsureSuccessStatusCode();
        var creados = JsonConvert.DeserializeObject<List<ZonaDTO>>(await postResponse.Content.ReadAsStringAsync())!;
        var zonaExistenteId = creados[0].Id;

        var dtosModificar = new List<ZonaDTO>
        {
            new()
            {
                Id = zonaExistenteId,
                Nombre = "Zona existente modificada",
                FaseId = faseId,
                Orden = 1,
                Equipos = [new EquipoDeLaZonaDTO { Id = equipo1Id.ToString() }]
            },
            new()
            {
                Nombre = "Nueva Zona",
                FaseId = faseId,
                Orden = 2,
                Equipos = [new EquipoDeLaZonaDTO { Id = equipo2Id.ToString() }]
            }
        };

        var putResponse = await client.PutAsJsonAsync($"/api/Fase/{faseId}/zonas/modificar-zonas-masivamente", dtosModificar);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, putResponse.StatusCode);

        var listResponse = await client.GetAsync($"/api/Fase/{faseId}/zonas");
        listResponse.EnsureSuccessStatusCode();
        var zonas = JsonConvert.DeserializeObject<List<ZonaDTO>>(await listResponse.Content.ReadAsStringAsync());
        Assert.NotNull(zonas);
        Assert.Equal(2, zonas.Count);

        var zonaModificada = zonas.FirstOrDefault(z => z.Id == zonaExistenteId);
        Assert.NotNull(zonaModificada);
        Assert.Equal("Zona existente modificada", zonaModificada.Nombre);
        Assert.NotNull(zonaModificada.Equipos);
        Assert.Single(zonaModificada.Equipos);
        Assert.Equal(equipo1Id.ToString(), zonaModificada.Equipos[0].Id);

        var zonaNueva = zonas.FirstOrDefault(z => z.Nombre == "Nueva Zona");
        Assert.NotNull(zonaNueva);
        Assert.True(zonaNueva.Id > 0);
        Assert.NotNull(zonaNueva.Equipos);
        Assert.Single(zonaNueva.Equipos);
        Assert.Equal(equipo2Id.ToString(), zonaNueva.Equipos[0].Id);
    }

    [Fact]
    public async Task ModificarMasivamente_ZonasSinEquipos_SeGuardanCorrectamente()
    {
        var faseId = await CrearFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var postResponse = await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas/crear-zonas-masivamente",
            new List<ZonaDTO>
            {
                new() { Nombre = "Zona A", Orden = 1 }
            });
        postResponse.EnsureSuccessStatusCode();
        var creados = JsonConvert.DeserializeObject<List<ZonaDTO>>(await postResponse.Content.ReadAsStringAsync())!;

        var dtosModificar = new List<ZonaDTO>
        {
            new()
            {
                Id = creados[0].Id,
                Nombre = "Zona A Modificada",
                FaseId = faseId,
                Orden = 1
            },
            new()
            {
                Nombre = "Zona B Nueva",
                FaseId = faseId,
                Orden = 2
            }
        };

        var putResponse = await client.PutAsJsonAsync($"/api/Fase/{faseId}/zonas/modificar-zonas-masivamente", dtosModificar);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, putResponse.StatusCode);

        var listResponse = await client.GetAsync($"/api/Fase/{faseId}/zonas");
        listResponse.EnsureSuccessStatusCode();
        var zonas = JsonConvert.DeserializeObject<List<ZonaDTO>>(await listResponse.Content.ReadAsStringAsync());
        Assert.NotNull(zonas);
        Assert.Equal(2, zonas.Count);

        var zonaA = zonas.FirstOrDefault(z => z.Nombre == "Zona A Modificada");
        Assert.NotNull(zonaA);
        Assert.NotNull(zonaA.Equipos);
        Assert.Empty(zonaA.Equipos);

        var zonaB = zonas.FirstOrDefault(z => z.Nombre == "Zona B Nueva");
        Assert.NotNull(zonaB);
        Assert.True(zonaB.Id > 0);
        Assert.NotNull(zonaB.Equipos);
        Assert.Empty(zonaB.Equipos);
    }

    [Fact]
    public async Task CrearZonasMasivamente_RecibeOrdenYListarLoDevuelveOrdenado()
    {
        var faseId = await CrearFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        // Mando los Orden desordenados para verificar que se respeta lo que mandó el consumidor
        // y que la respuesta del listado los devuelve ordenados por Orden ascendente.
        var dtos = new List<ZonaDTO>
        {
            new() { Nombre = "Charlie", Orden = 3 },
            new() { Nombre = "Alfa", Orden = 1 },
            new() { Nombre = "Bravo", Orden = 2 }
        };

        var postResponse = await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas/crear-zonas-masivamente", dtos);
        postResponse.EnsureSuccessStatusCode();

        var creados = JsonConvert.DeserializeObject<List<ZonaDTO>>(await postResponse.Content.ReadAsStringAsync());
        Assert.NotNull(creados);
        Assert.Equal(3, creados.Count);
        Assert.Equal(1, creados.Single(z => z.Nombre == "Alfa").Orden);
        Assert.Equal(2, creados.Single(z => z.Nombre == "Bravo").Orden);
        Assert.Equal(3, creados.Single(z => z.Nombre == "Charlie").Orden);

        var getResponse = await client.GetAsync($"/api/Fase/{faseId}/zonas");
        getResponse.EnsureSuccessStatusCode();
        var zonas = JsonConvert.DeserializeObject<List<ZonaDTO>>(await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(zonas);
        Assert.Equal(3, zonas.Count);
        Assert.Equal(new[] { "Alfa", "Bravo", "Charlie" }, zonas.Select(z => z.Nombre).ToArray());
        Assert.Equal(new[] { 1, 2, 3 }, zonas.Select(z => z.Orden).ToArray());
    }

    [Fact]
    public async Task ModificarZonasMasivamente_RotaOrdenEntreZonasExistentes_204()
    {
        // Reproduce el bug del índice único (FaseId, Orden):
        // 4 zonas existentes con Orden 1..4 que se reasignan al revés/desordenadas.
        // Sin "liberar" los Orden previos, el primer UPDATE chocaría con el índice único.
        var faseId = await CrearFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var postResponse = await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas/crear-zonas-masivamente",
            new List<ZonaDTO>
            {
                new() { Nombre = "2013", Orden = 1 },
                new() { Nombre = "2014", Orden = 2 },
                new() { Nombre = "2015", Orden = 3 },
                new() { Nombre = "2016", Orden = 4 }
            });
        postResponse.EnsureSuccessStatusCode();
        var creados = JsonConvert.DeserializeObject<List<ZonaDTO>>(await postResponse.Content.ReadAsStringAsync())!;

        var z2013 = creados.Single(z => z.Nombre == "2013");
        var z2014 = creados.Single(z => z.Nombre == "2014");
        var z2015 = creados.Single(z => z.Nombre == "2015");
        var z2016 = creados.Single(z => z.Nombre == "2016");

        // Asigno Orden nuevos a las 4 zonas existentes; cada zona "viene desde" un Orden ya ocupado
        // por otra, así que ningún UPDATE individual puede aplicarse sin liberar primero.
        var dtosModificar = new List<ZonaDTO>
        {
            new() { Id = z2014.Id, Nombre = "2014", FaseId = faseId, Orden = 1, Equipos = [] },
            new() { Id = z2016.Id, Nombre = "2016", FaseId = faseId, Orden = 2, Equipos = [] },
            new() { Id = z2015.Id, Nombre = "2015", FaseId = faseId, Orden = 3, Equipos = [] },
            new() { Id = z2013.Id, Nombre = "2013", FaseId = faseId, Orden = 4, Equipos = [] }
        };

        var putResponse = await client.PutAsJsonAsync($"/api/Fase/{faseId}/zonas/modificar-zonas-masivamente", dtosModificar);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, putResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/Fase/{faseId}/zonas");
        getResponse.EnsureSuccessStatusCode();
        var zonas = JsonConvert.DeserializeObject<List<ZonaDTO>>(await getResponse.Content.ReadAsStringAsync())!;
        Assert.Equal(4, zonas.Count);
        // El listado viene ordenado por Orden ascendente.
        Assert.Equal(new[] { "2014", "2016", "2015", "2013" }, zonas.Select(z => z.Nombre).ToArray());
        Assert.Equal(new[] { 1, 2, 3, 4 }, zonas.Select(z => z.Orden).ToArray());
    }

    [Fact]
    public async Task CrearZonasMasivamente_SinOrden_RetornaBadRequest()
    {
        var faseId = await CrearFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        // Payload sin la propiedad "orden", para validar que es requerida.
        // Uso un objeto anónimo para esquivar el `required` del DTO en C# y construir el JSON crudo.
        var dtosSinOrden = new[]
        {
            new { nombre = "Zona Sin Orden" }
        };

        var response = await client.PostAsJsonAsync($"/api/Fase/{faseId}/zonas/crear-zonas-masivamente", dtosSinOrden);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
