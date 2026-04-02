using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Api.TestsUtilidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace Api.TestsDeIntegracion;

public class FechasIT : TestBase
{
    private Utilidades? _utilidades;
    private Club? _club;

    public FechasIT(CustomWebApplicationFactory<Program> factory) : base(factory)
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

    private static async Task<int> CrearZonaDePrueba(CustomWebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var torneo = new Torneo { Id = 0, Nombre = "Torneo Test Fechas", Anio = 2026, TorneoAgrupadorId = 1 };
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

        var content = JsonConvert.DeserializeObject<List<FechaDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Empty(content);
    }

    [Fact]
    public async Task CrearFecha_DatosCorrectos_200()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var dto = new FechaDTO
        {
            Dia = new DateOnly(2026, 3, 15),
            Numero = 1,
            EsVisibleEnApp = true
        };

        var response = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas", dto);

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<FechaDTO>(await response.Content.ReadAsStringAsync());
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

        var dto = new FechaDTO
        {
            Dia = new DateOnly(2026, 3, 15),
            Numero = 1,
            EsVisibleEnApp = true
        };

        var response = await client.PostAsJsonAsync("/api/Zona/99999/fechas", dto);

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

        var content = JsonConvert.DeserializeObject<FechaDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(fecha.Id, content.Id);
        Assert.Equal(new DateOnly(2026, 4, 20), content.Dia);
        Assert.Equal(2, content.Numero);
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
            var torneo2 = new Torneo { Id = 0, Nombre = "Otro Torneo", Anio = 2026, TorneoAgrupadorId = 1 };
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
        var dto = new FechaDTO
        {
            Id = fecha.Id,
            Dia = new DateOnly(2026, 3, 10),
            Numero = 1,
            ZonaId = zonaId,
            EsVisibleEnApp = false
        };

        var response = await client.PutAsJsonAsync($"/api/Zona/{zonaId}/fechas/{fecha.Id}", dto);

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
            var torneo2 = new Torneo { Id = 0, Nombre = "Otro Torneo", Anio = 2026, TorneoAgrupadorId = 1 };
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

        await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas", new FechaDTO
        {
            Dia = new DateOnly(2026, 3, 1),
            Numero = 1,
            EsVisibleEnApp = true
        });
        await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas", new FechaDTO
        {
            Dia = new DateOnly(2026, 3, 8),
            Numero = 2,
            EsVisibleEnApp = true
        });

        var response = await client.GetAsync($"/api/Zona/{zonaId}/fechas");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<FechaDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(2, content.Count);
        Assert.Contains(content, f => f.Numero == 1 && f.Dia == new DateOnly(2026, 3, 1));
        Assert.Contains(content, f => f.Numero == 2 && f.Dia == new DateOnly(2026, 3, 8));
    }

    [Fact]
    public async Task CrearFechasMasivamente_DatosCorrectos_200()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var dtos = new List<FechaDTO>
        {
            new() { Dia = new DateOnly(2026, 3, 1), Numero = 1, EsVisibleEnApp = true },
            new() { Dia = new DateOnly(2026, 3, 8), Numero = 2, EsVisibleEnApp = true },
            new() { Dia = new DateOnly(2026, 3, 15), Numero = 3, EsVisibleEnApp = false }
        };

        var response = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas/crear-fechas-masivamente", dtos);
        response.EnsureSuccessStatusCode();

        var creados = JsonConvert.DeserializeObject<List<FechaDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(creados);
        Assert.Equal(3, creados.Count);
        Assert.All(creados, f => Assert.True(f.Id > 0));
        Assert.Contains(creados, f => f.Numero == 1 && f.Dia == new DateOnly(2026, 3, 1));
        Assert.Contains(creados, f => f.Numero == 2 && f.Dia == new DateOnly(2026, 3, 8));
        Assert.Contains(creados, f => f.Numero == 3 && f.Dia == new DateOnly(2026, 3, 15));

        var listResponse = await client.GetAsync($"/api/Zona/{zonaId}/fechas");
        listResponse.EnsureSuccessStatusCode();
        var list = JsonConvert.DeserializeObject<List<FechaDTO>>(await listResponse.Content.ReadAsStringAsync());
        Assert.NotNull(list);
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public async Task ModificarFechasMasivamente_FechasNoIncluidasEnArray_SeEliminan()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var postResponse = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas/crear-fechas-masivamente",
            new List<FechaDTO>
            {
                new() { Dia = new DateOnly(2026, 3, 1), Numero = 1, EsVisibleEnApp = true },
                new() { Dia = new DateOnly(2026, 3, 8), Numero = 2, EsVisibleEnApp = true },
                new() { Dia = new DateOnly(2026, 3, 15), Numero = 3, EsVisibleEnApp = true }
            });
        postResponse.EnsureSuccessStatusCode();
        var creados = JsonConvert.DeserializeObject<List<FechaDTO>>(await postResponse.Content.ReadAsStringAsync())!;

        var dtosModificar = new List<FechaDTO>
        {
            new() { Id = creados[0].Id, Dia = new DateOnly(2026, 3, 5), Numero = 1, ZonaId = zonaId, EsVisibleEnApp = true },
            new() { Id = creados[2].Id, Dia = new DateOnly(2026, 3, 20), Numero = 3, ZonaId = zonaId, EsVisibleEnApp = true }
        };

        var putResponse = await client.PutAsJsonAsync($"/api/Zona/{zonaId}/fechas/modificar-fechas-masivamente", dtosModificar);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, putResponse.StatusCode);

        var listResponse = await client.GetAsync($"/api/Zona/{zonaId}/fechas");
        listResponse.EnsureSuccessStatusCode();
        var fechas = JsonConvert.DeserializeObject<List<FechaDTO>>(await listResponse.Content.ReadAsStringAsync());
        Assert.NotNull(fechas);
        Assert.Equal(2, fechas.Count);
        Assert.Contains(fechas, f => f.Dia == new DateOnly(2026, 3, 5));
        Assert.Contains(fechas, f => f.Dia == new DateOnly(2026, 3, 20));
        Assert.Contains(fechas, f => f.Numero == 1);
        Assert.Contains(fechas, f => f.Numero == 2);
    }

    [Fact]
    public async Task EliminarFecha_RenumeraLasSiguientesConsecutivamente()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var postResponse = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas/crear-fechas-masivamente",
            new List<FechaDTO>
            {
                new() { Dia = new DateOnly(2026, 3, 1), Numero = 1, EsVisibleEnApp = true },
                new() { Dia = new DateOnly(2026, 3, 8), Numero = 2, EsVisibleEnApp = true },
                new() { Dia = new DateOnly(2026, 3, 15), Numero = 3, EsVisibleEnApp = true },
                new() { Dia = new DateOnly(2026, 3, 22), Numero = 4, EsVisibleEnApp = true }
            });
        postResponse.EnsureSuccessStatusCode();
        var creados = JsonConvert.DeserializeObject<List<FechaDTO>>(await postResponse.Content.ReadAsStringAsync())!;
        var fechaAEliminarId = creados[1].Id;

        var deleteResponse = await client.DeleteAsync($"/api/Zona/{zonaId}/fechas/{fechaAEliminarId}");
        deleteResponse.EnsureSuccessStatusCode();

        var listResponse = await client.GetAsync($"/api/Zona/{zonaId}/fechas");
        listResponse.EnsureSuccessStatusCode();
        var fechas = JsonConvert.DeserializeObject<List<FechaDTO>>(await listResponse.Content.ReadAsStringAsync());
        Assert.NotNull(fechas);
        Assert.Equal(3, fechas.Count);
        Assert.Contains(fechas, f => f.Numero == 1);
        Assert.Contains(fechas, f => f.Numero == 2);
        Assert.Contains(fechas, f => f.Numero == 3);
        Assert.DoesNotContain(fechas, f => f.Numero == 4);
    }

    [Fact]
    public async Task ModificarFechasMasivamente_ConFechaNuevaSinId_CreaLaFechaNueva()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var postResponse = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas/crear-fechas-masivamente",
            new List<FechaDTO>
            {
                new() { Dia = new DateOnly(2026, 3, 1), Numero = 1, EsVisibleEnApp = true }
            });
        postResponse.EnsureSuccessStatusCode();
        var creados = JsonConvert.DeserializeObject<List<FechaDTO>>(await postResponse.Content.ReadAsStringAsync())!;
        var fechaExistenteId = creados[0].Id;

        var dtosModificar = new List<FechaDTO>
        {
            new() { Id = fechaExistenteId, Dia = new DateOnly(2026, 3, 10), Numero = 1, ZonaId = zonaId, EsVisibleEnApp = false },
            new() { Dia = new DateOnly(2026, 3, 17), Numero = 2, EsVisibleEnApp = true }
        };

        var putResponse = await client.PutAsJsonAsync($"/api/Zona/{zonaId}/fechas/modificar-fechas-masivamente", dtosModificar);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, putResponse.StatusCode);

        var listResponse = await client.GetAsync($"/api/Zona/{zonaId}/fechas");
        listResponse.EnsureSuccessStatusCode();
        var fechas = JsonConvert.DeserializeObject<List<FechaDTO>>(await listResponse.Content.ReadAsStringAsync());
        Assert.NotNull(fechas);
        Assert.Equal(2, fechas.Count);

        var fechaModificada = fechas.FirstOrDefault(f => f.Id == fechaExistenteId);
        Assert.NotNull(fechaModificada);
        Assert.Equal(new DateOnly(2026, 3, 10), fechaModificada.Dia);
        Assert.False(fechaModificada.EsVisibleEnApp);

        var fechaNueva = fechas.FirstOrDefault(f => f.Dia == new DateOnly(2026, 3, 17));
        Assert.NotNull(fechaNueva);
        Assert.True(fechaNueva.Id > 0);
        Assert.Equal(2, fechaNueva.Numero);
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

        var dtos = new List<FechaDTO>
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

        var response = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas/crear-fechas-masivamente", dtos);
        response.EnsureSuccessStatusCode();

        var creados = JsonConvert.DeserializeObject<List<FechaDTO>>(await response.Content.ReadAsStringAsync());
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

        var dtos = new List<FechaDTO>
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

        var response = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas/crear-fechas-masivamente", dtos);
        response.EnsureSuccessStatusCode();
        var creados = JsonConvert.DeserializeObject<List<FechaDTO>>(await response.Content.ReadAsStringAsync());
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

        var postResponse = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas/crear-fechas-masivamente",
            new List<FechaDTO>
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
            });
        postResponse.EnsureSuccessStatusCode();
        var creados = JsonConvert.DeserializeObject<List<FechaDTO>>(await postResponse.Content.ReadAsStringAsync())!;
        var fechaId = creados[0].Id;
        var jornadaNormalId = creados[0].Jornadas!.First(j => j.Tipo == "Normal").Id;

        var dtosModificar = new List<FechaDTO>
        {
            new()
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

        var putResponse = await client.PutAsJsonAsync($"/api/Zona/{zonaId}/fechas/modificar-fechas-masivamente", dtosModificar);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, putResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/Zona/{zonaId}/fechas/{fechaId}");
        getResponse.EnsureSuccessStatusCode();
        var fecha = JsonConvert.DeserializeObject<FechaDTO>(await getResponse.Content.ReadAsStringAsync());
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

        var postResponse = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/fechas/crear-fechas-masivamente",
            new List<FechaDTO>
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
            });
        postResponse.EnsureSuccessStatusCode();
        var creados = JsonConvert.DeserializeObject<List<FechaDTO>>(await postResponse.Content.ReadAsStringAsync())!;
        var fechaId = creados[0].Id;
        var jornadaNormalId = creados[0].Jornadas!.First(j => j.Tipo == "Normal").Id;
        var jornadaLibreId = creados[0].Jornadas!.First(j => j.Tipo == "Libre").Id;

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(2, await context.Partidos.CountAsync(p => p.JornadaId == jornadaLibreId));
        }

        var dtoModificar = new FechaDTO
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

        var putResponse = await client.PutAsJsonAsync($"/api/Zona/{zonaId}/fechas/{fechaId}", dtoModificar);
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
        var response = await client.PostAsync($"/api/Zona/{zonaId}/fechas/crear-fechas-masivamente", content);
        response.EnsureSuccessStatusCode();

        var creados = JsonConvert.DeserializeObject<List<FechaDTO>>(await response.Content.ReadAsStringAsync());
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
}
