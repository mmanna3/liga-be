using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Logica;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;
using Api.TestsDeIntegracion._Config;
using Api.TestsUtilidades;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace Api.TestsDeIntegracion;

public class TorneoZonaIT : TestBase
{
    private Utilidades? _utilidades;
    private Club? _club;

    public TorneoZonaIT(CustomWebApplicationFactory<Program> factory) : base(factory)
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

    [Fact]
    public async Task CrearZona_ConEquipos_AsignaEquiposALaZona()
    {
        var faseId = await CrearTorneoFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        int equipoId;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            equipoId = context.Equipos.First(e => e.ClubId == _club!.Id).Id;
        }

        var dto = new TorneoZonaDTO
        {
            Nombre = "Zona con Equipos",
            Equipos =
            [
                new EquipoDeLaZonaDTO { Id = equipoId.ToString(), Nombre = "ignorado", Club = "ignorado", Codigo = "ignorado" }
            ]
        };

        var response = await client.PostAsJsonAsync($"/api/TorneoFase/{faseId}/zonas", dto);
        response.EnsureSuccessStatusCode();

        var creado = JsonConvert.DeserializeObject<TorneoZonaDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(creado);
        Assert.True(creado.Id > 0);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var equipo = context.Equipos.Find(equipoId);
            Assert.NotNull(equipo);
            Assert.Equal(creado.Id, equipo.ZonaActualId);
        }
    }

    [Fact]
    public async Task ObtenerZona_PorId_DevuelveEquiposCompletos()
    {
        Assert.NotNull(_club);
        var faseId = await CrearTorneoFaseDePrueba(Factory);
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

        var dtoCrear = new TorneoZonaDTO
        {
            Nombre = "Zona para GET",
            Equipos = [new EquipoDeLaZonaDTO { Id = equipoId.ToString() }]
        };
        var postResponse = await client.PostAsJsonAsync($"/api/TorneoFase/{faseId}/zonas", dtoCrear);
        postResponse.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<TorneoZonaDTO>(await postResponse.Content.ReadAsStringAsync())!;

        var getResponse = await client.GetAsync($"/api/TorneoFase/{faseId}/zonas/{creado.Id}");
        getResponse.EnsureSuccessStatusCode();

        var zona = JsonConvert.DeserializeObject<TorneoZonaDTO>(await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(zona);
        Assert.NotNull(zona.Equipos);
        Assert.Single(zona.Equipos);
        Assert.Equal(equipoId.ToString(), zona.Equipos[0].Id);
        Assert.Equal(equipoNombre, zona.Equipos[0].Nombre);
        Assert.Equal(clubNombre, zona.Equipos[0].Club);
        Assert.Equal(GeneradorDeHash.GenerarAlfanumerico7Digitos(equipoId), zona.Equipos[0].Codigo);
    }

    [Fact]
    public async Task ListarZonas_DevuelveEquiposCompletos()
    {
        var faseId = await CrearTorneoFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        int equipoId;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            equipoId = context.Equipos.First(e => e.ClubId == _club!.Id).Id;
        }

        var dtoCrear = new TorneoZonaDTO
        {
            Nombre = "Zona para Listar",
            Equipos = [new EquipoDeLaZonaDTO { Id = equipoId.ToString() }]
        };
        await client.PostAsJsonAsync($"/api/TorneoFase/{faseId}/zonas", dtoCrear);

        var response = await client.GetAsync($"/api/TorneoFase/{faseId}/zonas");
        response.EnsureSuccessStatusCode();

        var zonas = JsonConvert.DeserializeObject<List<TorneoZonaDTO>>(await response.Content.ReadAsStringAsync());
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
        var faseId = await CrearTorneoFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        int equipoId;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            equipoId = context.Equipos.First(e => e.ClubId == _club!.Id).Id;
        }

        var dtoCrear = new TorneoZonaDTO
        {
            Nombre = "Zona Original",
            Equipos = [new EquipoDeLaZonaDTO { Id = equipoId.ToString() }]
        };
        var postResponse = await client.PostAsJsonAsync($"/api/TorneoFase/{faseId}/zonas", dtoCrear);
        postResponse.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<TorneoZonaDTO>(await postResponse.Content.ReadAsStringAsync())!;

        var dtoModificar = new TorneoZonaDTO
        {
            Id = creado.Id,
            Nombre = "Zona Renombrada",
            TorneoFaseId = faseId
            // Equipos = null (no enviar, no modificar)
        };

        var putResponse = await client.PutAsJsonAsync($"/api/TorneoFase/{faseId}/zonas/{creado.Id}", dtoModificar);
        putResponse.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var equipo = context.Equipos.Find(equipoId);
            Assert.NotNull(equipo);
            Assert.Equal(creado.Id, equipo.ZonaActualId);
        }
    }

    [Fact]
    public async Task ModificarZona_EquiposVacio_BorraTodosLosEquipos()
    {
        var faseId = await CrearTorneoFaseDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        int equipoId;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            equipoId = context.Equipos.First(e => e.ClubId == _club!.Id).Id;
        }

        var dtoCrear = new TorneoZonaDTO
        {
            Nombre = "Zona para Vaciar",
            Equipos = [new EquipoDeLaZonaDTO { Id = equipoId.ToString() }]
        };
        var postResponse = await client.PostAsJsonAsync($"/api/TorneoFase/{faseId}/zonas", dtoCrear);
        postResponse.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<TorneoZonaDTO>(await postResponse.Content.ReadAsStringAsync())!;

        var dtoModificar = new TorneoZonaDTO
        {
            Id = creado.Id,
            Nombre = creado.Nombre,
            TorneoFaseId = faseId,
            Equipos = []
        };

        var putResponse = await client.PutAsJsonAsync($"/api/TorneoFase/{faseId}/zonas/{creado.Id}", dtoModificar);
        putResponse.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var equipo = context.Equipos.Find(equipoId);
            Assert.NotNull(equipo);
            Assert.Null(equipo.ZonaActualId);
        }
    }

    [Fact]
    public async Task ModificarZona_EquiposConItems_ReemplazaEquipos()
    {
        var faseId = await CrearTorneoFaseDePrueba(Factory);
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

        var dtoCrear = new TorneoZonaDTO
        {
            Nombre = "Zona para Reemplazar",
            Equipos = [new EquipoDeLaZonaDTO { Id = equipo1Id.ToString() }]
        };
        var postResponse = await client.PostAsJsonAsync($"/api/TorneoFase/{faseId}/zonas", dtoCrear);
        postResponse.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<TorneoZonaDTO>(await postResponse.Content.ReadAsStringAsync())!;

        var dtoModificar = new TorneoZonaDTO
        {
            Id = creado.Id,
            Nombre = creado.Nombre,
            TorneoFaseId = faseId,
            Equipos = [new EquipoDeLaZonaDTO { Id = equipo2Id.ToString() }]
        };

        var putResponse = await client.PutAsJsonAsync($"/api/TorneoFase/{faseId}/zonas/{creado.Id}", dtoModificar);
        putResponse.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var eq1 = context.Equipos.Find(equipo1Id);
            var eq2 = context.Equipos.Find(equipo2Id);
            Assert.NotNull(eq1);
            Assert.NotNull(eq2);
            Assert.Null(eq1.ZonaActualId);
            Assert.Equal(creado.Id, eq2.ZonaActualId);
        }
    }

    [Fact]
    public async Task CrearZonasMasivamente_DatosCorrectos_200()
    {
        Assert.NotNull(_club);
        var faseId = await CrearTorneoFaseDePrueba(Factory);
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

        var dtos = new List<TorneoZonaDTO>
        {
            new() { Nombre = "Zona A", Equipos = [new EquipoDeLaZonaDTO { Id = equipo1Id.ToString() }] },
            new() { Nombre = "Zona B" },
            new() { Nombre = "Zona C", Equipos = [new EquipoDeLaZonaDTO { Id = equipo2Id.ToString() }] }
        };

        var response = await client.PostAsJsonAsync($"/api/TorneoFase/{faseId}/zonas/crear-zonas-masivamente", dtos);
        response.EnsureSuccessStatusCode();

        var creados = JsonConvert.DeserializeObject<List<TorneoZonaDTO>>(await response.Content.ReadAsStringAsync());
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

        var getResponse = await client.GetAsync($"/api/TorneoFase/{faseId}/zonas/{zonaA.Id}");
        getResponse.EnsureSuccessStatusCode();
        var zonaAGet = JsonConvert.DeserializeObject<TorneoZonaDTO>(await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(zonaAGet);
        Assert.NotNull(zonaAGet.Equipos);
        Assert.Single(zonaAGet.Equipos);
        Assert.Equal(equipo1Id.ToString(), zonaAGet.Equipos[0].Id);
        Assert.Equal(equipo1Nombre, zonaAGet.Equipos[0].Nombre);
        Assert.Equal(clubNombre, zonaAGet.Equipos[0].Club);
        Assert.NotEmpty(zonaAGet.Equipos[0].Codigo);
    }

    [Fact]
    public async Task ModificarZonasMasivamente_DatosCorrectos_204()
    {
        Assert.NotNull(_club);
        var faseId = await CrearTorneoFaseDePrueba(Factory);
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

        var postResponse = await client.PostAsJsonAsync($"/api/TorneoFase/{faseId}/zonas/crear-zonas-masivamente",
            new List<TorneoZonaDTO>
            {
                new() { Nombre = "Zona 1" },
                new() { Nombre = "Zona 2" }
            });
        postResponse.EnsureSuccessStatusCode();
        var creados = JsonConvert.DeserializeObject<List<TorneoZonaDTO>>(await postResponse.Content.ReadAsStringAsync())!;

        var dtosModificar = new List<TorneoZonaDTO>
        {
            new()
            {
                Id = creados[0].Id,
                Nombre = "Zona Uno",
                TorneoFaseId = faseId,
                Equipos = [new EquipoDeLaZonaDTO { Id = equipo1Id.ToString() }]
            },
            new()
            {
                Id = creados[1].Id,
                Nombre = "Zona Dos",
                TorneoFaseId = faseId,
                Equipos = [new EquipoDeLaZonaDTO { Id = equipo2Id.ToString() }]
            }
        };

        var putResponse = await client.PutAsJsonAsync($"/api/TorneoFase/{faseId}/zonas/modificar-zonas-masivamente", dtosModificar);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, putResponse.StatusCode);

        var getZona1 = await client.GetAsync($"/api/TorneoFase/{faseId}/zonas/{creados[0].Id}");
        getZona1.EnsureSuccessStatusCode();
        var zona1 = JsonConvert.DeserializeObject<TorneoZonaDTO>(await getZona1.Content.ReadAsStringAsync());
        Assert.NotNull(zona1);
        Assert.Equal("Zona Uno", zona1.Nombre);
        Assert.NotNull(zona1.Equipos);
        Assert.Single(zona1.Equipos);
        Assert.Equal(equipo1Id.ToString(), zona1.Equipos[0].Id);
        Assert.Equal(equipo1Nombre, zona1.Equipos[0].Nombre);
        Assert.Equal(clubNombre, zona1.Equipos[0].Club);
        Assert.NotEmpty(zona1.Equipos[0].Codigo);

        var getZona2 = await client.GetAsync($"/api/TorneoFase/{faseId}/zonas/{creados[1].Id}");
        getZona2.EnsureSuccessStatusCode();
        var zona2 = JsonConvert.DeserializeObject<TorneoZonaDTO>(await getZona2.Content.ReadAsStringAsync());
        Assert.NotNull(zona2);
        Assert.Equal("Zona Dos", zona2.Nombre);
        Assert.NotNull(zona2.Equipos);
        Assert.Single(zona2.Equipos);
        Assert.Equal(equipo2Id.ToString(), zona2.Equipos[0].Id);
        Assert.Equal(equipo2Nombre, zona2.Equipos[0].Nombre);
        Assert.Equal(clubNombre, zona2.Equipos[0].Club);
        Assert.NotEmpty(zona2.Equipos[0].Codigo);
    }
}
