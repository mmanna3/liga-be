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

public class JugadorIT : TestBase
{
    public JugadorIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        SeedData(context);
    }

    private void SeedData(AppDbContext context)
    {
        var club = new Club
        {
            Id = 1,
            Nombre = "Club de Prueba"
        };
        
        context.Clubs.Add(club);
        
        var equipo = new Equipo
        {
            Id = 1,
            Nombre = "Equipo de Prueba",
            ClubId = 1,
            Jugadores = new List<JugadorEquipo>()
        };
        
        context.Equipos.Add(equipo);
        context.SaveChanges();
    }
    
    [Fact]
    public async Task CrearJugador_CodigoQueNoExiste_LanzaError()
    {
        var client = await GetAuthenticatedClient();

        // Usar código válido en formato pero que apunta a un equipo que no existe (Id=2)
        var codigoAlfanumerico = GeneradorDeHash.GenerarAlfanumerico7Digitos(2);

        var jugadorDTO = new JugadorDTO
        {
            Nombre = "Maria",
            Apellido = "Lopez",
            DNI = "55555555",
            FechaNacimiento = DateTime.Now.AddYears(-22),
            CodigoAlfanumerico = codigoAlfanumerico,
            FotoCarnet = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==",
            FotoDNIFrente = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==",
            FotoDNIDorso = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="
        };

        var response = await client.PostAsJsonAsync("/api/jugador", jugadorDTO);

        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task CrearJugador_DniDuplicadoYActivo_LanzaError()
    {
        // Arrange: seed un jugador activo con DNI=12345678
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var jugador = new Jugador
        {
            Id = 10,
            DNI = "12345678",
            Nombre = "Jugador",
            Apellido = "Existente",
            FechaNacimiento = new DateTime(1990, 1, 1)
        };
        context.Jugadores.Add(jugador);
        context.SaveChanges();

        context.JugadorEquipo.Add(new JugadorEquipo
        {
            Id = 101,
            JugadorId = 10,
            EquipoId = 1,
            FechaFichaje = DateTime.Now,
            EstadoJugadorId = (int)EstadoJugadorEnum.Activo
        });
        context.SaveChanges();

        var client = await GetAuthenticatedClient();
        var codigoAlfanumerico = GeneradorDeHash.GenerarAlfanumerico7Digitos(1);

        var jugadorDTO = new JugadorDTO
        {
            Nombre = "Juan",
            Apellido = "Perez",
            DNI = "12345678",
            FechaNacimiento = DateTime.Now.AddYears(-20),
            CodigoAlfanumerico = codigoAlfanumerico,
            FotoCarnet = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==",
            FotoDNIFrente = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==",
            FotoDNIDorso = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="
        };

        var response = await client.PostAsJsonAsync("/api/jugador", jugadorDTO);

        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task CrearJugador_DatosCorrectos_200()
    {
        // Usar un cliente autenticado
        var client = await GetAuthenticatedClient();

        // Generar un código alfanumérico válido
        var codigoAlfanumerico = GeneradorDeHash.GenerarAlfanumerico7Digitos(1);

        // Crear un jugador con todos los campos requeridos
        var jugadorDTO = new JugadorDTO
        {
            Nombre = "Juan",
            Apellido = "Perez",
            DNI = "12345678",
            FechaNacimiento = DateTime.Now.AddYears(-20),
            CodigoAlfanumerico = codigoAlfanumerico,
            // Añadir campos adicionales que puedan ser requeridos
            FotoCarnet = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==",
            FotoDNIFrente = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==",
            FotoDNIDorso = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="
        };

        var response = await client.PostAsJsonAsync("/api/jugador", jugadorDTO);

        // Si hay un error, imprimir el contenido de la respuesta para depuración
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error al crear jugador: {errorContent}");
        }

        response.EnsureSuccessStatusCode();

        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<JugadorDTO>(stringResponse);

        Assert.NotNull(content);
        Assert.Equal("Juan", content.Nombre);
    }

    [Fact]
    public async Task EliminarJugador_JugadorExistente_Devuelve200()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var jugador = new Jugador
        {
            Id = 50,
            DNI = "99887766",
            Nombre = "Carlos",
            Apellido = "Ruiz",
            FechaNacimiento = new DateTime(1995, 6, 15)
        };
        context.Jugadores.Add(jugador);
        context.JugadorEquipo.Add(new JugadorEquipo
        {
            Id = 501,
            JugadorId = 50,
            EquipoId = 1,
            FechaFichaje = DateTime.Now,
            EstadoJugadorId = (int)EstadoJugadorEnum.Activo
        });
        context.SaveChanges();

        var client = await GetAuthenticatedClient();
        var response = await client.DeleteAsync("/api/Jugador/50");

        response.EnsureSuccessStatusCode();

        using var verifyScope = Factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jugadorEliminado = verifyContext.Jugadores.Find(50);
        Assert.Null(jugadorEliminado);
    }

    [Fact]
    public async Task EliminarJugador_JugadorInexistente_Devuelve404()
    {
        var client = await GetAuthenticatedClient();
        var response = await client.DeleteAsync("/api/Jugador/9999");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task EfectuarPases_JugadorYEquiposValidos_TransfiereCorrectamente()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var equipoDestino = new Equipo
        {
            Id = 2,
            Nombre = "Equipo Destino",
            ClubId = 1,
            Jugadores = new List<JugadorEquipo>()
        };
        context.Equipos.Add(equipoDestino);

        var jugador = new Jugador
        {
            Id = 51,
            DNI = "11223344",
            Nombre = "Luis",
            Apellido = "Gomez",
            FechaNacimiento = new DateTime(2000, 3, 20)
        };
        context.Jugadores.Add(jugador);
        context.JugadorEquipo.Add(new JugadorEquipo
        {
            Id = 511,
            JugadorId = 51,
            EquipoId = 1,
            FechaFichaje = DateTime.Now,
            EstadoJugadorId = (int)EstadoJugadorEnum.Activo
        });
        context.SaveChanges();

        var client = await GetAuthenticatedClient();
        var pases = new List<EfectuarPaseDTO>
        {
            new EfectuarPaseDTO { JugadorId = 51, EquipoOrigenId = 1, EquipoDestinoId = 2 }
        };
        var response = await client.PostAsJsonAsync("/api/Jugador/efectuar-pases", pases);

        response.EnsureSuccessStatusCode();

        using var verifyScope = Factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tieneEnOrigen = verifyContext.JugadorEquipo.Any(je => je.JugadorId == 51 && je.EquipoId == 1);
        var tieneEnDestino = verifyContext.JugadorEquipo.Any(je => je.JugadorId == 51 && je.EquipoId == 2);
        Assert.False(tieneEnOrigen);
        Assert.True(tieneEnDestino);
    }
}