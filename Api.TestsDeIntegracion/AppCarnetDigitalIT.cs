using System.Net.Http.Json;
using Api.Core.DTOs.AppCarnetDigital;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Logica;
using Api.Core.Servicios;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;

namespace Api.TestsDeIntegracion;

public class AppCarnetDigitalIT : TestBase
{
    public AppCarnetDigitalIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var paths = scope.ServiceProvider.GetRequiredService<AppPaths>();
        
        SeedData(context, paths);
    }

    private void SeedData(AppDbContext context, AppPaths paths)
    {
        // Crear un club
        var club = new Club
        {
            Id = 1,
            Nombre = "Club de Prueba"
        };
        context.Clubs.Add(club);

        // Crear un torneo
        var torneo = new Torneo
        {
            Id = 1,
            Nombre = "Torneo 2024"
        };
        context.Torneos.Add(torneo);
        
        // Crear un equipo
        var equipo = new Equipo
        {
            Id = 1,
            Nombre = "Equipo de Prueba",
            ClubId = 1,
            TorneoId = 1,
            Jugadores = new List<JugadorEquipo>()
        };
        context.Equipos.Add(equipo);

        // Crear jugadores
        var jugador1 = new Jugador
        {
            Id = 1,
            Nombre = "Juan",
            Apellido = "Pérez",
            DNI = "12345678",
            FechaNacimiento = new DateTime(2000, 1, 1)
        };

        var jugador2 = new Jugador
        {
            Id = 2,
            Nombre = "Pedro",
            Apellido = "González",
            DNI = "87654321",
            FechaNacimiento = new DateTime(2001, 2, 2)
        };

        context.Jugadores.AddRange(jugador1, jugador2);
        
        // Guardar para obtener los IDs
        context.SaveChanges();

        // Asociar jugadores al equipo con diferentes estados
        var jugadorEquipo1 = new JugadorEquipo
        {
            Id = 1,
            JugadorId = 1,
            EquipoId = 1,
            Equipo = equipo,
            Jugador = jugador1,
            EstadoJugadorId = 3, // Activo
            FechaFichaje = DateTime.Now
        };

        var jugadorEquipo2 = new JugadorEquipo
        {
            Id = 2,
            JugadorId = 2,
            EquipoId = 1,
            Equipo = equipo,
            Jugador = jugador2,
            EstadoJugadorId = 1, // Fichaje pendiente de aprobación
            FechaFichaje = DateTime.Now
        };

        equipo.Jugadores.Add(jugadorEquipo1);
        equipo.Jugadores.Add(jugadorEquipo2);
        context.JugadorEquipo.AddRange(jugadorEquipo1, jugadorEquipo2);
        
        // Guardar los cambios
        context.SaveChanges();

        // Crear usuario y delegado para el endpoint equipos-del-delegado
        var rolDelegado = context.Roles.First(r => r.Nombre == "Delegado");
        var usuarioDelegado = new Usuario
        {
            Id = 100,
            NombreUsuario = "delegadoTest",
            Password = AuthCore.HashPassword("delegado123"),
            RolId = rolDelegado.Id
        };
        context.Usuarios.Add(usuarioDelegado);

        var estadoActivo = context.EstadoDelegado.First(e => e.Estado == "Activo");
        var delegado = new Delegado
        {
            Id = 1,
            DNI = "11111111",
            Nombre = "Delegado",
            Apellido = "Test",
            FechaNacimiento = new DateTime(1990, 1, 1),
            UsuarioId = usuarioDelegado.Id,
            EstadoDelegadoId = estadoActivo.Id,
            DelegadoClubs = new List<DelegadoClub>()
        };
        context.Delegados.Add(delegado);
        context.DelegadoClub.Add(new DelegadoClub { Id = 1, DelegadoId = delegado.Id, ClubId = club.Id });
        context.SaveChanges();

        // Crear directorio de imágenes y agregar una imagen de prueba
        Directory.CreateDirectory(paths.ImagenesJugadoresAbsolute);
        
        // Crear una imagen de prueba de 240x240 píxeles
        using var bitmap = new SKBitmap(240, 240);
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint();
        paint.Color = SKColors.Blue;
        canvas.DrawRect(0, 0, 240, 240, paint);
        
        // Guardar la imagen para el primer jugador
        using var stream1 = new FileStream($"{paths.ImagenesJugadoresAbsolute}/12345678.jpg", FileMode.Create);
        using var image1 = SKImage.FromBitmap(bitmap);
        using var data1 = image1.Encode(SKEncodedImageFormat.Jpeg, 75);
        data1.SaveTo(stream1);
        
        // Guardar la imagen para el segundo jugador
        using var stream2 = new FileStream($"{paths.ImagenesJugadoresAbsolute}/87654321.jpg", FileMode.Create);
        using var image2 = SKImage.FromBitmap(bitmap);
        using var data2 = image2.Encode(SKEncodedImageFormat.Jpeg, 75);
        data2.SaveTo(stream2);
    }
    
    [Fact]
    public async Task Carnets_EquipoExistente_DevuelveCarnetsCorrectos()
    {
        var client = await GetAuthenticatedClient();
        
        var response = await client.GetAsync("/api/carnet-digital/carnets?equipoId=1");
        
        response.EnsureSuccessStatusCode();
        
        var carnets = await response.Content.ReadFromJsonAsync<List<CarnetDigitalDTO>>();
        
        Assert.NotNull(carnets);
        Assert.Single(carnets);
        
        var primerCarnet = carnets.First();
        Assert.Equal("Juan", primerCarnet.Nombre);
        Assert.Equal("Pérez", primerCarnet.Apellido);
        Assert.Equal("12345678", primerCarnet.DNI);
        Assert.Equal((int)EstadoJugadorEnum.Activo, primerCarnet.Estado);
    }
    
    [Fact]
    public async Task JugadoresPendientes_EquipoExistente_DevuelveCarnetsCorrectos()
    {
        var client = await GetAuthenticatedClient();
        
        var response = await client.GetAsync("/api/carnet-digital/jugadores-pendientes?equipoId=1");
        
        response.EnsureSuccessStatusCode();
        
        var carnets = await response.Content.ReadFromJsonAsync<List<CarnetDigitalPendienteDTO>>();
        
        Assert.NotNull(carnets);
        Assert.Single(carnets);
        
        var segundoCarnet = carnets.First();
        Assert.Equal("Pedro", segundoCarnet.Nombre);
        Assert.Equal("González", segundoCarnet.Apellido);
        Assert.Equal("87654321", segundoCarnet.DNI);
        Assert.Equal(1, segundoCarnet.Estado); // EstadoJugador.Pendiente
    }

    [Fact]
    public async Task Carnets_EquipoInexistente_Devuelve404()
    {
        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync("/api/carnet-digital/carnets?equipoId=999");
        
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task EquiposDelDelegado_DelegadoConClubYEquipos_DevuelveEquipos()
    {
        var client = Factory.CreateClient();
        client = await AuthTestHelper.GetAuthenticatedClient(client, "delegadoTest", "delegado123");

        var response = await client.GetAsync("/api/carnet-digital/equipos-del-delegado");

        response.EnsureSuccessStatusCode();

        var resultado = await response.Content.ReadFromJsonAsync<EquiposDelDelegadoDTO>();

        Assert.NotNull(resultado);
        Assert.NotNull(resultado.ClubsConEquipos);
        Assert.NotEmpty(resultado.ClubsConEquipos);

        var clubConEquipos = resultado.ClubsConEquipos.First();
        Assert.Equal("Club de Prueba", clubConEquipos.Nombre);
        Assert.NotNull(clubConEquipos.Equipos);
        Assert.NotEmpty(clubConEquipos.Equipos);

        var equipo = clubConEquipos.Equipos.First();
        Assert.Equal(1, equipo.Id);
        Assert.Equal("Equipo de Prueba", equipo.Nombre);
        Assert.Equal("Torneo 2024", equipo.Torneo);
        Assert.NotNull(equipo.CodigoAlfanumerico);
    }
    
    // [Fact]
    // public async Task CarnetsPorCodigoAlfanumerico_EquipoExistente_DevuelveCarnets()
    // {
    //     var client = await GetAuthenticatedClient();
    //     
    //     var response = await client.GetAsync("/api/carnet-digital/carnets?carnets-por-codigo-alfanumerico=999");
    //     
    // }
} 