using System.Net;
using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion;

public class ArbitroIT : TestBase
{
    public ArbitroIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task ListarArbitros_ListaVacia_Funciona()
    {
        var client = await GetAuthenticatedClient();

        var response = await client.GetAsync("/api/arbitro");

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<ArbitroDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Empty(content);
    }

    [Fact]
    public async Task CrearArbitro_DatosCorrectos_200()
    {
        var client = await GetAuthenticatedClient();

        var dto = new ArbitroDTO
        {
            DNI = "30123456",
            Nombre = "Juan",
            Apellido = "Pérez",
            TelefonoCelular = "1122334455"
        };

        var response = await client.PostAsJsonAsync("/api/arbitro", dto);

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<ArbitroDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.True(content.Id > 0);
        Assert.Equal("30123456", content.DNI);
        Assert.Equal("Juan", content.Nombre);
        Assert.Equal("Pérez", content.Apellido);
        Assert.Equal("1122334455", content.TelefonoCelular);
    }

    [Fact]
    public async Task ObtenerArbitro_PorId_DevuelveCorrecto()
    {
        var client = await GetAuthenticatedClient();

        int arbitroId;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var arbitro = new Arbitro
            {
                Id = 0,
                DNI = "28765432",
                Nombre = "Carlos",
                Apellido = "Gómez"
            };
            context.Arbitros.Add(arbitro);
            context.SaveChanges();
            arbitroId = arbitro.Id;
        }

        var response = await client.GetAsync($"/api/arbitro/{arbitroId}");

        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<ArbitroDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(arbitroId, content.Id);
        Assert.Equal("28765432", content.DNI);
        Assert.Equal("Carlos", content.Nombre);
        Assert.Equal("Gómez", content.Apellido);
    }

    [Fact]
    public async Task ModificarArbitro_DatosCorrectos_204()
    {
        var client = await GetAuthenticatedClient();

        Arbitro arbitro;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            arbitro = new Arbitro
            {
                Id = 0,
                DNI = "33445566",
                Nombre = "Pedro",
                Apellido = "López"
            };
            context.Arbitros.Add(arbitro);
            context.SaveChanges();
        }

        var dto = new ArbitroDTO
        {
            Id = arbitro.Id,
            DNI = "33445566",
            Nombre = "Pedro Modif.",
            Apellido = "López Modif.",
            TelefonoCelular = "1199887766"
        };

        var response = await client.PutAsJsonAsync($"/api/arbitro/{arbitro.Id}", dto);

        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var actualizado = context.Arbitros.Find(arbitro.Id);
            Assert.NotNull(actualizado);
            Assert.Equal("Pedro Modif.", actualizado.Nombre);
            Assert.Equal("López Modif.", actualizado.Apellido);
            Assert.Equal("1199887766", actualizado.TelefonoCelular);
        }
    }

    [Fact]
    public async Task EliminarArbitro_SinReferencias_200()
    {
        var client = await GetAuthenticatedClient();

        Arbitro arbitro;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            arbitro = new Arbitro
            {
                Id = 0,
                DNI = "44556677",
                Nombre = "Para",
                Apellido = "Eliminar"
            };
            context.Arbitros.Add(arbitro);
            context.SaveChanges();
        }

        var response = await client.DeleteAsync($"/api/arbitro/{arbitro.Id}");

        response.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Null(context.Arbitros.Find(arbitro.Id));
        }
    }

    [Fact]
    public async Task ObtenerArbitros_PorIds_DevuelveSolicitados()
    {
        var client = await GetAuthenticatedClient();

        int id1, id2;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var arbitro1 = new Arbitro { Id = 0, DNI = "11111111", Nombre = "Primero", Apellido = "Árbitro" };
            var arbitro2 = new Arbitro { Id = 0, DNI = "22222222", Nombre = "Segundo", Apellido = "Árbitro" };
            context.Arbitros.AddRange(arbitro1, arbitro2);
            context.SaveChanges();
            id1 = arbitro1.Id;
            id2 = arbitro2.Id;
        }

        var response = await client.GetAsync($"/api/arbitro/por-ids?ids={id1}&ids={id2}");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<ArbitroDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(2, content.Count);
        Assert.Contains(content, a => a.Id == id1 && a.Nombre == "Primero");
        Assert.Contains(content, a => a.Id == id2 && a.Nombre == "Segundo");
    }

    [Fact]
    public async Task FlujoCompleto_CrearListarModificarYEliminar()
    {
        var client = await GetAuthenticatedClient();

        var crearDto = new ArbitroDTO
        {
            DNI = "55667788",
            Nombre = "Flujo",
            Apellido = "Completo",
            TelefonoCelular = "1155667788"
        };
        var crearResponse = await client.PostAsJsonAsync("/api/arbitro", crearDto);
        crearResponse.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<ArbitroDTO>(await crearResponse.Content.ReadAsStringAsync());
        Assert.NotNull(creado);

        var lista = await client.GetFromJsonAsync<List<ArbitroDTO>>("/api/arbitro");
        Assert.NotNull(lista);
        Assert.Single(lista);
        Assert.Equal(creado.Id, lista[0].Id);

        var modificarDto = new ArbitroDTO
        {
            Id = creado.Id,
            DNI = "55667788",
            Nombre = "Flujo Mod.",
            Apellido = "Completo M.",
            TelefonoCelular = "1199001122"
        };
        var modificarResponse = await client.PutAsJsonAsync($"/api/arbitro/{creado.Id}", modificarDto);
        modificarResponse.EnsureSuccessStatusCode();

        var obtenido = await client.GetFromJsonAsync<ArbitroDTO>($"/api/arbitro/{creado.Id}");
        Assert.NotNull(obtenido);
        Assert.Equal("Flujo Mod.", obtenido.Nombre);
        Assert.Equal("1199001122", obtenido.TelefonoCelular);

        var eliminarResponse = await client.DeleteAsync($"/api/arbitro/{creado.Id}");
        eliminarResponse.EnsureSuccessStatusCode();

        var listaFinal = await client.GetFromJsonAsync<List<ArbitroDTO>>("/api/arbitro");
        Assert.NotNull(listaFinal);
        Assert.Empty(listaFinal);
    }

    [Fact]
    public async Task CrearArbitro_SinAgrupadores_ListaVacia()
    {
        var client = await GetAuthenticatedClient();

        var dto = new ArbitroDTO
        {
            DNI = "40123456",
            Nombre = "Sin",
            Apellido = "Agrupadores"
        };

        var crearResponse = await client.PostAsJsonAsync("/api/arbitro", dto);
        crearResponse.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<ArbitroDTO>(await crearResponse.Content.ReadAsStringAsync());
        Assert.NotNull(creado);

        var obtenido = await client.GetFromJsonAsync<ArbitroDTO>($"/api/arbitro/{creado.Id}");
        Assert.NotNull(obtenido);
        Assert.Empty(obtenido.TorneoAgrupadorIds);
        Assert.Empty(obtenido.TorneoAgrupadores);
    }

    [Fact]
    public async Task CrearArbitro_ConDosAgrupadores_DevuelveIdsYNombres()
    {
        var client = await GetAuthenticatedClient();
        int agrupadorId2;

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var agrupador2 = new TorneoAgrupador
            {
                Id = 0,
                Nombre = "Futsal",
                EsVisibleEnApp = true,
                ColorId = (int)ColorEnum.Negro
            };
            context.TorneoAgrupadores.Add(agrupador2);
            context.SaveChanges();
            agrupadorId2 = agrupador2.Id;
        }

        var dto = new ArbitroDTO
        {
            DNI = "40234567",
            Nombre = "Con",
            Apellido = "Agrupadores",
            TorneoAgrupadorIds = [1, agrupadorId2]
        };

        var crearResponse = await client.PostAsJsonAsync("/api/arbitro", dto);
        crearResponse.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<ArbitroDTO>(await crearResponse.Content.ReadAsStringAsync());
        Assert.NotNull(creado);

        var obtenido = await client.GetFromJsonAsync<ArbitroDTO>($"/api/arbitro/{creado.Id}");
        Assert.NotNull(obtenido);
        Assert.Equal(2, obtenido.TorneoAgrupadorIds.Count);
        Assert.Contains(1, obtenido.TorneoAgrupadorIds);
        Assert.Contains(agrupadorId2, obtenido.TorneoAgrupadorIds);
        Assert.Equal(2, obtenido.TorneoAgrupadores.Count);
        Assert.Contains(obtenido.TorneoAgrupadores, a => a.TorneoAgrupadorId == 1 && a.TorneoAgrupadorNombre == "General");
        Assert.Contains(obtenido.TorneoAgrupadores, a => a.TorneoAgrupadorId == agrupadorId2 && a.TorneoAgrupadorNombre == "Futsal");
    }

    [Fact]
    public async Task ModificarArbitro_CambiaAgrupadoresAsignados()
    {
        var client = await GetAuthenticatedClient();
        int agrupadorId2;
        int arbitroId;

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var agrupador2 = new TorneoAgrupador
            {
                Id = 0,
                Nombre = "Beach",
                EsVisibleEnApp = true,
                ColorId = (int)ColorEnum.Negro
            };
            context.TorneoAgrupadores.Add(agrupador2);
            context.SaveChanges();
            agrupadorId2 = agrupador2.Id;

            var arbitro = new Arbitro
            {
                Id = 0,
                DNI = "40345678",
                Nombre = "Modif",
                Apellido = "Agrupadores",
                ArbitroTorneoAgrupadores =
                [
                    new ArbitroTorneoAgrupador { Id = 0, TorneoAgrupadorId = 1 }
                ]
            };
            context.Arbitros.Add(arbitro);
            context.SaveChanges();
            arbitroId = arbitro.Id;
        }

        var dto = new ArbitroDTO
        {
            Id = arbitroId,
            DNI = "40345678",
            Nombre = "Modif",
            Apellido = "Agrupadores",
            TorneoAgrupadorIds = [agrupadorId2]
        };

        var response = await client.PutAsJsonAsync($"/api/arbitro/{arbitroId}", dto);
        response.EnsureSuccessStatusCode();

        var obtenido = await client.GetFromJsonAsync<ArbitroDTO>($"/api/arbitro/{arbitroId}");
        Assert.NotNull(obtenido);
        Assert.Single(obtenido.TorneoAgrupadorIds);
        Assert.Equal(agrupadorId2, obtenido.TorneoAgrupadorIds[0]);
        Assert.Single(obtenido.TorneoAgrupadores);
        Assert.Equal("Beach", obtenido.TorneoAgrupadores[0].TorneoAgrupadorNombre);
    }

    [Fact]
    public async Task ModificarArbitro_AgrupadorInexistente_400()
    {
        var client = await GetAuthenticatedClient();
        int arbitroId;

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var arbitro = new Arbitro
            {
                Id = 0,
                DNI = "40456789",
                Nombre = "Error",
                Apellido = "Agrupador"
            };
            context.Arbitros.Add(arbitro);
            context.SaveChanges();
            arbitroId = arbitro.Id;
        }

        var dto = new ArbitroDTO
        {
            Id = arbitroId,
            DNI = "40456789",
            Nombre = "Error",
            Apellido = "Agrupador",
            TorneoAgrupadorIds = [99999]
        };

        var response = await client.PutAsJsonAsync($"/api/arbitro/{arbitroId}", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EliminarArbitro_ConAgrupadores_EliminaEnlaces()
    {
        var client = await GetAuthenticatedClient();
        int arbitroId;

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var arbitro = new Arbitro
            {
                Id = 0,
                DNI = "40567890",
                Nombre = "Eliminar",
                Apellido = "ConAgrupador",
                ArbitroTorneoAgrupadores =
                [
                    new ArbitroTorneoAgrupador { Id = 0, TorneoAgrupadorId = 1 }
                ]
            };
            context.Arbitros.Add(arbitro);
            context.SaveChanges();
            arbitroId = arbitro.Id;
        }

        var response = await client.DeleteAsync($"/api/arbitro/{arbitroId}");
        response.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Null(context.Arbitros.Find(arbitroId));
            Assert.Empty(context.ArbitroTorneoAgrupador.Where(a => a.ArbitroId == arbitroId).ToList());
        }
    }
}
