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

    /// <summary>
    /// Crea el escenario base para tests de pase:
    /// - TorneoOro y TorneoPl distintos
    /// - EquipoAzul y EquipoRojo en TorneoOro, EquipoVerde en TorneoPl
    /// - Jugador inscripto en EquipoRojo y EquipoVerde
    /// Los IDs usan <paramref name="baseId"/> como prefijo para evitar colisiones entre tests.
    /// </summary>
    private void SeedEscenarioPases(AppDbContext context, int baseId,
        out int azulId, out int rojoId, out int verdeId, out int jugadorId)
    {
        var torneoOroId = baseId;
        var torneoPlataId = baseId + 1;
        azulId  = baseId + 10;
        rojoId  = baseId + 11;
        verdeId = baseId + 12;
        jugadorId = baseId + 20;

        context.Torneos.AddRange(
            new Torneo { Id = torneoOroId,   Nombre = $"Oro {baseId}"  },
            new Torneo { Id = torneoPlataId, Nombre = $"Plata {baseId}" }
        );
        context.Equipos.AddRange(
            new Equipo { Id = azulId,  Nombre = $"Azul {baseId}",  ClubId = 1, TorneoId = torneoOroId,   Jugadores = new List<JugadorEquipo>() },
            new Equipo { Id = rojoId,  Nombre = $"Rojo {baseId}",  ClubId = 1, TorneoId = torneoOroId,   Jugadores = new List<JugadorEquipo>() },
            new Equipo { Id = verdeId, Nombre = $"Verde {baseId}", ClubId = 1, TorneoId = torneoPlataId, Jugadores = new List<JugadorEquipo>() }
        );
        context.Jugadores.Add(new Jugador
        {
            Id = jugadorId,
            DNI = $"{baseId}12",
            Nombre = "Test",
            Apellido = "Jugador",
            FechaNacimiento = new DateTime(2000, 1, 1)
        });
        context.JugadorEquipo.AddRange(
            new JugadorEquipo { Id = baseId + 30, JugadorId = jugadorId, EquipoId = rojoId,  FechaFichaje = DateTime.Now, EstadoJugadorId = (int)EstadoJugadorEnum.Activo },
            new JugadorEquipo { Id = baseId + 31, JugadorId = jugadorId, EquipoId = verdeId, FechaFichaje = DateTime.Now, EstadoJugadorId = (int)EstadoJugadorEnum.Activo }
        );
        context.SaveChanges();
    }

    [Fact]
    public async Task EfectuarPases_VerdeAzul_FallaConflictoTorneoOro()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedEscenarioPases(context, 1000, out _, out _, out var verdeId, out var jugadorId);
        // azulId = 1010, rojoId = 1011, verdeId = 1012
        var azulId = 1010;

        var client = await GetAuthenticatedClient();
        var pases = new List<EfectuarPaseDTO>
        {
            new EfectuarPaseDTO { JugadorId = jugadorId, EquipoOrigenId = verdeId, EquipoDestinoId = azulId }
        };
        var response = await client.PostAsJsonAsync("/api/Jugador/efectuar-pases", pases);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EfectuarPases_VerdeRojo_FallaJugadorYaEnDestino()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedEscenarioPases(context, 2000, out _, out var rojoId, out var verdeId, out var jugadorId);

        var client = await GetAuthenticatedClient();
        var pases = new List<EfectuarPaseDTO>
        {
            new EfectuarPaseDTO { JugadorId = jugadorId, EquipoOrigenId = verdeId, EquipoDestinoId = rojoId }
        };
        var response = await client.PostAsJsonAsync("/api/Jugador/efectuar-pases", pases);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EfectuarPases_RojoVerde_FallaJugadorYaEnDestino()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedEscenarioPases(context, 3000, out _, out var rojoId, out var verdeId, out var jugadorId);

        var client = await GetAuthenticatedClient();
        var pases = new List<EfectuarPaseDTO>
        {
            new EfectuarPaseDTO { JugadorId = jugadorId, EquipoOrigenId = rojoId, EquipoDestinoId = verdeId }
        };
        var response = await client.PostAsJsonAsync("/api/Jugador/efectuar-pases", pases);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EfectuarPases_RojoAzul_TransfiereYSetaAprobadoPendienteDePago()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedEscenarioPases(context, 4000, out var azulId, out var rojoId, out _, out var jugadorId);

        var client = await GetAuthenticatedClient();
        var pases = new List<EfectuarPaseDTO>
        {
            new EfectuarPaseDTO { JugadorId = jugadorId, EquipoOrigenId = rojoId, EquipoDestinoId = azulId }
        };
        var response = await client.PostAsJsonAsync("/api/Jugador/efectuar-pases", pases);

        response.EnsureSuccessStatusCode();

        using var verifyScope = Factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.False(verifyContext.JugadorEquipo.Any(je => je.JugadorId == jugadorId && je.EquipoId == rojoId));
        Assert.True(verifyContext.JugadorEquipo.Any(je => je.JugadorId == jugadorId && je.EquipoId == azulId));

        var jeAzul = verifyContext.JugadorEquipo.Single(je => je.JugadorId == jugadorId && je.EquipoId == azulId);
        Assert.Equal((int)EstadoJugadorEnum.AprobadoPendienteDePago, jeAzul.EstadoJugadorId);
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

    [Fact]
    public async Task ListarConFiltro_ConDelegadoMismoDNI_DevuelveDelegadoId()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var club = context.Clubs.First();
        var delegado = new Delegado
        {
            Id = 7001,
            DNI = "33334444",
            Nombre = "Jugador",
            Apellido = "Delegado",
            FechaNacimiento = new DateTime(1990, 1, 1)
        };
        context.Delegados.Add(delegado);
        context.SaveChanges();
        context.DelegadoClub.Add(new DelegadoClub { Id = 0, DelegadoId = delegado.Id, ClubId = club.Id, EstadoDelegadoId = (int)EstadoDelegadoEnum.Activo });
        context.SaveChanges();

        var jugador = new Jugador
        {
            Id = 7011,
            DNI = "33334444",
            Nombre = "Jugador",
            Apellido = "Delegado",
            FechaNacimiento = new DateTime(1990, 1, 1)
        };
        context.Jugadores.Add(jugador);
        context.SaveChanges();

        context.JugadorEquipo.Add(new JugadorEquipo
        {
            Id = 7021,
            JugadorId = jugador.Id,
            EquipoId = 1,
            FechaFichaje = DateTime.Now,
            EstadoJugadorId = (int)EstadoJugadorEnum.Activo
        });
        context.SaveChanges();

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync("/api/Jugador/listar-con-filtro?estados=Activo");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<JugadorDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);

        var jugadorConDelegado = content.SingleOrDefault(j => j.DNI == "33334444");
        Assert.NotNull(jugadorConDelegado);
        Assert.Equal(delegado.Id, jugadorConDelegado.DelegadoId);
    }

    [Fact]
    public async Task ObtenerJugadorPorId_ConDelegadoMismoDNI_DevuelveDelegadoId()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var club = context.Clubs.First();
        var delegado = new Delegado
        {
            Id = 7031,
            DNI = "44445555",
            Nombre = "Maria",
            Apellido = "Delegada",
            FechaNacimiento = new DateTime(1993, 4, 4)
        };
        context.Delegados.Add(delegado);
        context.SaveChanges();
        context.DelegadoClub.Add(new DelegadoClub { Id = 0, DelegadoId = delegado.Id, ClubId = club.Id, EstadoDelegadoId = (int)EstadoDelegadoEnum.Activo });
        context.SaveChanges();

        var jugador = new Jugador
        {
            Id = 7041,
            DNI = "44445555",
            Nombre = "Maria",
            Apellido = "Delegada",
            FechaNacimiento = new DateTime(1993, 4, 4)
        };
        context.Jugadores.Add(jugador);
        context.SaveChanges();

        context.JugadorEquipo.Add(new JugadorEquipo
        {
            Id = 7051,
            JugadorId = jugador.Id,
            EquipoId = 1,
            FechaFichaje = DateTime.Now,
            EstadoJugadorId = (int)EstadoJugadorEnum.Activo
        });
        context.SaveChanges();

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/Jugador/{jugador.Id}");
        response.EnsureSuccessStatusCode();

        var dto = JsonConvert.DeserializeObject<JugadorDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(dto);
        Assert.Equal(delegado.Id, dto.DelegadoId);
    }

    [Fact]
    public async Task ListarConFiltro_SinDelegadoMismoDNI_DelegadoIdEsNull()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var jugador = new Jugador
        {
            Id = 7061,
            DNI = "99998888",
            Nombre = "Solo",
            Apellido = "Jugador",
            FechaNacimiento = new DateTime(2000, 9, 9)
        };
        context.Jugadores.Add(jugador);
        context.SaveChanges();

        context.JugadorEquipo.Add(new JugadorEquipo
        {
            Id = 7071,
            JugadorId = jugador.Id,
            EquipoId = 1,
            FechaFichaje = DateTime.Now,
            EstadoJugadorId = (int)EstadoJugadorEnum.Activo
        });
        context.SaveChanges();

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync("/api/Jugador/listar-con-filtro?estados=Activo");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<JugadorDTO>>(await response.Content.ReadAsStringAsync());
        var jugadorSolo = content?.SingleOrDefault(j => j.DNI == "99998888");
        Assert.NotNull(jugadorSolo);
        Assert.Null(jugadorSolo.DelegadoId);
    }
}