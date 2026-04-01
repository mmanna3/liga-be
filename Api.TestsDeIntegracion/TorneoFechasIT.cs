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

public class TorneoFechasIT : TestBase
{
    private Utilidades? _utilidades;
    private Club? _club;

    public TorneoFechasIT(CustomWebApplicationFactory<Program> factory) : base(factory)
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

    private static async Task<int> CrearTorneoZonaDePrueba(CustomWebApplicationFactory<Program> factory)
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
        context.TorneoFases.Add(fase);
        await context.SaveChangesAsync();

        var zona = new ZonaTodosContraTodos { Id = 0, Nombre = "Zona única", TorneoFaseId = fase.Id };
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
            fecha = new FechaTodosContraTodos
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

            var fase2 = new FaseTodosContraTodos
            {
                Id = 0,
                Nombre = "",
                Numero = 1,
                TorneoId = torneo2.Id,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            context.TorneoFases.Add(fase2);
            await context.SaveChangesAsync();

            var zona2 = new ZonaTodosContraTodos { Id = 0, Nombre = "Zona B", TorneoFaseId = fase2.Id };
            context.TorneoZonas.Add(zona2);
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
            fecha = new FechaTodosContraTodos
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
            fecha = new FechaTodosContraTodos
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

            var fase2 = new FaseTodosContraTodos
            {
                Id = 0,
                Nombre = "",
                Numero = 1,
                TorneoId = torneo2.Id,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            context.TorneoFases.Add(fase2);
            await context.SaveChangesAsync();

            var zona2 = new ZonaTodosContraTodos { Id = 0, Nombre = "Zona B", TorneoFaseId = fase2.Id };
            context.TorneoZonas.Add(zona2);
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

    [Fact]
    public async Task CrearFechasMasivamente_DatosCorrectos_200()
    {
        var zonaId = await CrearTorneoZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var dtos = new List<TorneoFechaDTO>
        {
            new() { Dia = new DateOnly(2026, 3, 1), Numero = 1, EsVisibleEnApp = true },
            new() { Dia = new DateOnly(2026, 3, 8), Numero = 2, EsVisibleEnApp = true },
            new() { Dia = new DateOnly(2026, 3, 15), Numero = 3, EsVisibleEnApp = false }
        };

        var response = await client.PostAsJsonAsync($"/api/TorneoZona/{zonaId}/fechas/crear-fechas-masivamente", dtos);
        response.EnsureSuccessStatusCode();

        var creados = JsonConvert.DeserializeObject<List<TorneoFechaDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(creados);
        Assert.Equal(3, creados.Count);
        Assert.All(creados, f => Assert.True(f.Id > 0));
        Assert.Contains(creados, f => f.Numero == 1 && f.Dia == new DateOnly(2026, 3, 1));
        Assert.Contains(creados, f => f.Numero == 2 && f.Dia == new DateOnly(2026, 3, 8));
        Assert.Contains(creados, f => f.Numero == 3 && f.Dia == new DateOnly(2026, 3, 15));

        var listResponse = await client.GetAsync($"/api/TorneoZona/{zonaId}/fechas");
        listResponse.EnsureSuccessStatusCode();
        var list = JsonConvert.DeserializeObject<List<TorneoFechaDTO>>(await listResponse.Content.ReadAsStringAsync());
        Assert.NotNull(list);
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public async Task ModificarFechasMasivamente_FechasNoIncluidasEnArray_SeEliminan()
    {
        var zonaId = await CrearTorneoZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var postResponse = await client.PostAsJsonAsync($"/api/TorneoZona/{zonaId}/fechas/crear-fechas-masivamente",
            new List<TorneoFechaDTO>
            {
                new() { Dia = new DateOnly(2026, 3, 1), Numero = 1, EsVisibleEnApp = true },
                new() { Dia = new DateOnly(2026, 3, 8), Numero = 2, EsVisibleEnApp = true },
                new() { Dia = new DateOnly(2026, 3, 15), Numero = 3, EsVisibleEnApp = true }
            });
        postResponse.EnsureSuccessStatusCode();
        var creados = JsonConvert.DeserializeObject<List<TorneoFechaDTO>>(await postResponse.Content.ReadAsStringAsync())!;

        var dtosModificar = new List<TorneoFechaDTO>
        {
            new() { Id = creados[0].Id, Dia = new DateOnly(2026, 3, 5), Numero = 1, ZonaId = zonaId, EsVisibleEnApp = true },
            new() { Id = creados[2].Id, Dia = new DateOnly(2026, 3, 20), Numero = 3, ZonaId = zonaId, EsVisibleEnApp = true }
        };

        var putResponse = await client.PutAsJsonAsync($"/api/TorneoZona/{zonaId}/fechas/modificar-fechas-masivamente", dtosModificar);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, putResponse.StatusCode);

        var listResponse = await client.GetAsync($"/api/TorneoZona/{zonaId}/fechas");
        listResponse.EnsureSuccessStatusCode();
        var fechas = JsonConvert.DeserializeObject<List<TorneoFechaDTO>>(await listResponse.Content.ReadAsStringAsync());
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
        var zonaId = await CrearTorneoZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var postResponse = await client.PostAsJsonAsync($"/api/TorneoZona/{zonaId}/fechas/crear-fechas-masivamente",
            new List<TorneoFechaDTO>
            {
                new() { Dia = new DateOnly(2026, 3, 1), Numero = 1, EsVisibleEnApp = true },
                new() { Dia = new DateOnly(2026, 3, 8), Numero = 2, EsVisibleEnApp = true },
                new() { Dia = new DateOnly(2026, 3, 15), Numero = 3, EsVisibleEnApp = true },
                new() { Dia = new DateOnly(2026, 3, 22), Numero = 4, EsVisibleEnApp = true }
            });
        postResponse.EnsureSuccessStatusCode();
        var creados = JsonConvert.DeserializeObject<List<TorneoFechaDTO>>(await postResponse.Content.ReadAsStringAsync())!;
        var fechaAEliminarId = creados[1].Id;

        var deleteResponse = await client.DeleteAsync($"/api/TorneoZona/{zonaId}/fechas/{fechaAEliminarId}");
        deleteResponse.EnsureSuccessStatusCode();

        var listResponse = await client.GetAsync($"/api/TorneoZona/{zonaId}/fechas");
        listResponse.EnsureSuccessStatusCode();
        var fechas = JsonConvert.DeserializeObject<List<TorneoFechaDTO>>(await listResponse.Content.ReadAsStringAsync());
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
        var zonaId = await CrearTorneoZonaDePrueba(Factory);
        var client = await GetAuthenticatedClient();

        var postResponse = await client.PostAsJsonAsync($"/api/TorneoZona/{zonaId}/fechas/crear-fechas-masivamente",
            new List<TorneoFechaDTO>
            {
                new() { Dia = new DateOnly(2026, 3, 1), Numero = 1, EsVisibleEnApp = true }
            });
        postResponse.EnsureSuccessStatusCode();
        var creados = JsonConvert.DeserializeObject<List<TorneoFechaDTO>>(await postResponse.Content.ReadAsStringAsync())!;
        var fechaExistenteId = creados[0].Id;

        var dtosModificar = new List<TorneoFechaDTO>
        {
            new() { Id = fechaExistenteId, Dia = new DateOnly(2026, 3, 10), Numero = 1, ZonaId = zonaId, EsVisibleEnApp = false },
            new() { Dia = new DateOnly(2026, 3, 17), Numero = 2, EsVisibleEnApp = true }
        };

        var putResponse = await client.PutAsJsonAsync($"/api/TorneoZona/{zonaId}/fechas/modificar-fechas-masivamente", dtosModificar);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, putResponse.StatusCode);

        var listResponse = await client.GetAsync($"/api/TorneoZona/{zonaId}/fechas");
        listResponse.EnsureSuccessStatusCode();
        var fechas = JsonConvert.DeserializeObject<List<TorneoFechaDTO>>(await listResponse.Content.ReadAsStringAsync());
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
        var zonaId = await CrearTorneoZonaDePrueba(Factory);
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

        var dtos = new List<TorneoFechaDTO>
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
                        EquipoId = equipo1Id
                    }
                ]
            }
        };

        var response = await client.PostAsJsonAsync($"/api/TorneoZona/{zonaId}/fechas/crear-fechas-masivamente", dtos);
        response.EnsureSuccessStatusCode();

        var creados = JsonConvert.DeserializeObject<List<TorneoFechaDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(creados);
        Assert.Single(creados);
        Assert.NotNull(creados[0].Jornadas);
        Assert.Equal(2, creados[0].Jornadas!.Count);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var fecha = context.TorneoFechas.Include(f => f.Jornadas).First(f => f.Id == creados[0].Id);
            Assert.Equal(2, fecha.Jornadas.Count);
        }
    }

    [Fact]
    public async Task ModificarFechasMasivamente_ConJornadas_EliminaCreaModificaJornadas()
    {
        Assert.NotNull(_club);
        var zonaId = await CrearTorneoZonaDePrueba(Factory);
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

        var postResponse = await client.PostAsJsonAsync($"/api/TorneoZona/{zonaId}/fechas/crear-fechas-masivamente",
            new List<TorneoFechaDTO>
            {
                new()
                {
                    Dia = new DateOnly(2026, 3, 1),
                    Numero = 1,
                    EsVisibleEnApp = true,
                    Jornadas =
                    [
                        new JornadaDTO { Tipo = "Normal", ResultadosVerificados = false, LocalId = equipo1Id, VisitanteId = equipo2Id },
                        new JornadaDTO { Tipo = "Libre", ResultadosVerificados = false, EquipoId = equipo1Id }
                    ]
                }
            });
        postResponse.EnsureSuccessStatusCode();
        var creados = JsonConvert.DeserializeObject<List<TorneoFechaDTO>>(await postResponse.Content.ReadAsStringAsync())!;
        var fechaId = creados[0].Id;
        var jornadaNormalId = creados[0].Jornadas!.First(j => j.Tipo == "Normal").Id;

        var dtosModificar = new List<TorneoFechaDTO>
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
                    new JornadaDTO { Tipo = "Libre", ResultadosVerificados = false, EquipoId = equipo2Id }
                ]
            }
        };

        var putResponse = await client.PutAsJsonAsync($"/api/TorneoZona/{zonaId}/fechas/modificar-fechas-masivamente", dtosModificar);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, putResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/TorneoZona/{zonaId}/fechas/{fechaId}");
        getResponse.EnsureSuccessStatusCode();
        var fecha = JsonConvert.DeserializeObject<TorneoFechaDTO>(await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(fecha);
        Assert.NotNull(fecha.Jornadas);
        Assert.Equal(2, fecha.Jornadas.Count);

        var jornadaModificada = fecha.Jornadas.FirstOrDefault(j => j.Id == jornadaNormalId);
        Assert.NotNull(jornadaModificada);
        Assert.True(jornadaModificada.ResultadosVerificados);

        var jornadaLibre = fecha.Jornadas.FirstOrDefault(j => j.Tipo == "Libre");
        Assert.NotNull(jornadaLibre);
        Assert.Equal(equipo2Id, jornadaLibre.EquipoId);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var jornadas = context.Jornadas.Where(j => j.FechaId == fechaId).ToList();
            Assert.Equal(2, jornadas.Count);
        }
    }

    [Fact]
    public async Task CrearFechasMasivamente_FechaEnFormatoISO8601ConHora_DeserializaCorrectamente()
    {
        Assert.NotNull(_club);
        var zonaId = await CrearTorneoZonaDePrueba(Factory);
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
                        { "tipo": "Libre", "resultadosVerificados": false, "equipoId": "{{equipo3Id}}" }
                    ]
                },
                {
                    "numero": 2,
                    "dia": "2026-03-28T03:00:00.000Z",
                    "esVisibleEnApp": false,
                    "jornadas": [
                        { "tipo": "Normal", "resultadosVerificados": false, "localId": "{{equipo1Id}}", "visitanteId": "{{equipo3Id}}" },
                        { "tipo": "Libre", "resultadosVerificados": false, "equipoId": "{{equipo2Id}}" }
                    ]
                }
            ]
            """;

        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/api/TorneoZona/{zonaId}/fechas/crear-fechas-masivamente", content);
        response.EnsureSuccessStatusCode();

        var creados = JsonConvert.DeserializeObject<List<TorneoFechaDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(creados);
        Assert.Equal(2, creados.Count);
        Assert.Contains(creados, f => f.Numero == 1 && f.Dia == new DateOnly(2026, 3, 21));
        Assert.Contains(creados, f => f.Numero == 2 && f.Dia == new DateOnly(2026, 3, 28));
        Assert.All(creados, f => Assert.Equal(2, f.Jornadas!.Count));
    }
}
