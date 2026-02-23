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

        var equipo = new Equipo
        {
            Id = 1,
            Nombre = "Equipo de Prueba",
            ClubId = 1,
            Jugadores = new List<JugadorEquipo>()
        };
        context.Equipos.Add(equipo);

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
            JugadorId = 1,
            EquipoId = 1,
            FechaFichaje = DateTime.Now,
            EstadoJugadorId = (int)EstadoJugadorEnum.Activo
        });

        // Jugador sin equipo (para FicharEnOtroEquipo_JugadorActivoYCodigoValido_200)
        var jugadorSinEquipo = new Jugador
        {
            Id = 2,
            DNI = "22222222",
            Nombre = "Pedro",
            Apellido = "Garcia",
            FechaNacimiento = new DateTime(1995, 6, 15)
        };
        context.Jugadores.Add(jugadorSinEquipo);

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
        var codigo = GeneradorDeHash.GenerarAlfanumerico7Digitos(1);
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
}
