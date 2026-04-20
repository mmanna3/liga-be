using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Logica;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion;

public class PublicoIT : TestBase
{
    public PublicoIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        SeedData(context);
    }

    private void SeedData(AppDbContext context)
    {
        var club = new Club { Id = 1, Nombre = "Club de Prueba" };
        context.Clubs.Add(club);

        var equipo1 = new Equipo
        {
            Id = 1,
            Nombre = "Equipo de Prueba",
            ClubId = 1,
            Jugadores = new List<JugadorEquipo>()
        };
        var equipo2 = new Equipo
        {
            Id = 2,
            Nombre = "Equipo Destino",
            ClubId = 1,
            Jugadores = new List<JugadorEquipo>()
        };
        context.Equipos.Add(equipo1);
        context.Equipos.Add(equipo2);

        // Jugador con equipo activo (para ElDniEstaFichado_JugadorActivo_RetornaTrue)
        var jugadorActivo = new Jugador
        {
            Id = 1,
            DNI = "11111111",
            Nombre = "Juan",
            Apellido = "Perez",
            FechaNacimiento = new DateTime(2000, 1, 1)
        };
        context.Jugadores.Add(jugadorActivo);
        context.SaveChanges();

        context.JugadorEquipo.Add(new JugadorEquipo
        {
            Id = 1,
            JugadorId = 1,
            EquipoId = 1,
            FechaFichaje = DateTime.Now,
            EstadoJugadorId = (int)EstadoJugadorEnum.Activo
        });

        // Jugador aprobado en otro equipo (para FicharEnOtroEquipo - debe tener estado "existente": Activo, etc.)
        var jugadorParaFicharEnOtroEquipo = new Jugador
        {
            Id = 2,
            DNI = "22222222",
            Nombre = "Pedro",
            Apellido = "Garcia",
            FechaNacimiento = new DateTime(1995, 6, 15)
        };
        context.Jugadores.Add(jugadorParaFicharEnOtroEquipo);
        context.SaveChanges();

        context.JugadorEquipo.Add(new JugadorEquipo
        {
            Id = 2,
            JugadorId = 2,
            EquipoId = 1,
            FechaFichaje = DateTime.Now,
            EstadoJugadorId = (int)EstadoJugadorEnum.Activo
        });

        context.SaveChanges();
    }

    [Fact]
    public async Task ElDniEstaFichado_JugadorNoExiste_RetornaFalse()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/publico/el-dni-esta-fichado?dni=99999999");

        response.EnsureSuccessStatusCode();
        var result = JsonConvert.DeserializeObject<bool>(await response.Content.ReadAsStringAsync());
        Assert.False(result);
    }

    [Fact]
    public async Task ElDniEstaFichado_JugadorActivo_RetornaTrue()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/publico/el-dni-esta-fichado?dni=11111111");

        response.EnsureSuccessStatusCode();
        var result = JsonConvert.DeserializeObject<bool>(await response.Content.ReadAsStringAsync());
        Assert.True(result);
    }

    [Fact]
    public async Task ObtenerNombreEquipo_CodigoValido_RetornaRespuesta()
    {
        var client = Factory.CreateClient();
        var codigo = GeneradorDeHash.GenerarAlfanumerico7Digitos(1);

        var response = await client.GetAsync($"/api/publico/obtener-nombre-equipo?codigoAlfanumerico={codigo}");

        response.EnsureSuccessStatusCode();
        var result = JsonConvert.DeserializeObject<ObtenerNombreEquipoDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(result);
        Assert.False(result.HayError);
        Assert.Equal("Equipo de Prueba", result.Respuesta);
    }

    [Fact]
    public async Task ObtenerNombreEquipo_CodigoInvalido_RetornaHayError()
    {
        var client = Factory.CreateClient();

        // XXX0001 tiene formato válido pero checksum incorrecto → error controlado
        var response = await client.GetAsync("/api/publico/obtener-nombre-equipo?codigoAlfanumerico=XXX0001");

        response.EnsureSuccessStatusCode();
        var result = JsonConvert.DeserializeObject<ObtenerNombreEquipoDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(result);
        Assert.True(result.HayError);
    }

    [Fact]
    public async Task FicharEnOtroEquipo_JugadorExistenteYCodigoValido_RetornaJugadorId()
    {
        var client = Factory.CreateClient();
        // Jugador 22222222 está en equipo 1; lo fichamos en equipo 2
        var codigo = GeneradorDeHash.GenerarAlfanumerico7Digitos(2);
        var dto = new FicharEnOtroEquipoDTO { DNI = "22222222", CodigoAlfanumerico = codigo };

        var response = await client.PostAsJsonAsync("/api/publico/fichar-en-otro-equipo", dto);

        response.EnsureSuccessStatusCode();
        var jugadorId = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
        Assert.Equal(2, jugadorId);
    }

    [Fact]
    public async Task FicharEnOtroEquipo_JugadorNoExiste_LanzaError()
    {
        var client = Factory.CreateClient();
        var codigo = GeneradorDeHash.GenerarAlfanumerico7Digitos(1);
        var dto = new FicharEnOtroEquipoDTO { DNI = "99999999", CodigoAlfanumerico = codigo };

        var response = await client.PostAsJsonAsync("/api/publico/fichar-en-otro-equipo", dto);

        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task FicharEnOtroEquipo_JugadorYaJuegaEnEquipoDelMismoTorneo_LanzaError()
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var torneo = new Torneo
            {
                Id = 0,
                Nombre = "Torneo Unico",
                Anio = DateTime.Today.Year,
                EsVisibleEnApp = true,
                SeVenLosGolesEnTablaDePosiciones = true,
                TorneoAgrupadorId = 1
            };
            context.Torneos.Add(torneo);
            context.SaveChanges();

            var fase = new FaseTodosContraTodos
            {
                Id = 0,
                Nombre = "Fase",
                TorneoId = torneo.Id,
                Numero = 1,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            context.Fases.Add(fase);
            context.SaveChanges();

            var zona = new ZonaTodosContraTodos
            {
                Id = 0,
                FaseId = fase.Id,
                Nombre = "Zona A"
            };
            context.Zonas.Add(zona);
            context.SaveChanges();

            context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = 1, ZonaId = zona.Id });
            context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = 2, ZonaId = zona.Id });
            context.SaveChanges();
        }

        var client = Factory.CreateClient();
        var codigo = GeneradorDeHash.GenerarAlfanumerico7Digitos(2);
        var dto = new FicharEnOtroEquipoDTO { DNI = "22222222", CodigoAlfanumerico = codigo };

        var response = await client.PostAsJsonAsync("/api/publico/fichar-en-otro-equipo", dto);

        Assert.False(response.IsSuccessStatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("mismo torneo", errorContent);
    }

    [Fact]
    public async Task FicharEnOtroEquipo_JugadorJuegaEnEquipoDeOtroTorneo_PermiteFichaje()
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var torneo1 = new Torneo
            {
                Id = 0,
                Nombre = "Torneo 1",
                Anio = DateTime.Today.Year,
                EsVisibleEnApp = true,
                SeVenLosGolesEnTablaDePosiciones = true,
                TorneoAgrupadorId = 1
            };
            var torneo2 = new Torneo
            {
                Id = 0,
                Nombre = "Torneo 2",
                Anio = DateTime.Today.Year,
                EsVisibleEnApp = true,
                SeVenLosGolesEnTablaDePosiciones = true,
                TorneoAgrupadorId = 1
            };
            context.Torneos.AddRange(torneo1, torneo2);
            context.SaveChanges();

            var fase1 = new FaseTodosContraTodos
            {
                Id = 0,
                Nombre = "Fase 1",
                TorneoId = torneo1.Id,
                Numero = 1,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            var fase2 = new FaseTodosContraTodos
            {
                Id = 0,
                Nombre = "Fase 2",
                TorneoId = torneo2.Id,
                Numero = 1,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            context.Fases.AddRange(fase1, fase2);
            context.SaveChanges();

            var zona1 = new ZonaTodosContraTodos
            {
                Id = 0,
                FaseId = fase1.Id,
                Nombre = "Zona Torneo 1"
            };
            var zona2 = new ZonaTodosContraTodos
            {
                Id = 0,
                FaseId = fase2.Id,
                Nombre = "Zona Torneo 2"
            };
            context.Zonas.AddRange(zona1, zona2);
            context.SaveChanges();

            context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = 1, ZonaId = zona1.Id });
            context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = 2, ZonaId = zona2.Id });
            context.SaveChanges();
        }

        var client = Factory.CreateClient();
        var codigo = GeneradorDeHash.GenerarAlfanumerico7Digitos(2);
        var dto = new FicharEnOtroEquipoDTO { DNI = "22222222", CodigoAlfanumerico = codigo };

        var response = await client.PostAsJsonAsync("/api/publico/fichar-en-otro-equipo", dto);

        response.EnsureSuccessStatusCode();
        var jugadorId = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
        Assert.Equal(2, jugadorId);
    }
}
