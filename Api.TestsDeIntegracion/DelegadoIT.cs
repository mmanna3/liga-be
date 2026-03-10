using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.DTOs.CambiosDeEstadoDelegado;
using Api.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Api.Core.Entidades;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Api.TestsUtilidades;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion;

public class DelegadoIT : TestBase
{
    private const string FotoBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";

    private Utilidades? _utilidades;
    private Club? _club;
    private Club? _club2;

    public DelegadoIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        SeedData(context);
    }

    private Equipo? _equipo;

    private void SeedData(AppDbContext context)
    {
        _utilidades = new Utilidades(context);
        _club = _utilidades.DadoQueExisteElClub();
        context.SaveChanges();
        _club2 = _utilidades.DadoQueExisteElClub();
        context.SaveChanges();
        _equipo = _utilidades.DadoQueExisteElEquipo(_club);
        context.SaveChanges();
    }

    [Fact]
    public async Task ListarDelegados_Funciona()
    {
        var client = await GetAuthenticatedClient();
        
        var response = await client.GetAsync("/api/delegado");
        
        response.EnsureSuccessStatusCode();
        
        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<List<DelegadoDTO>>(stringResponse);
        
        Assert.NotNull(content);
    }
    
    [Fact]
    public async Task CrearDelegado_DatosCorrectos_200()
    {
        var client = await GetAuthenticatedClient();
        
        var delegadoDTO = new DelegadoDTO
        {
            DNI = "12345678",
            Nombre = "Juan",
            Apellido = "Pérez",
            FechaNacimiento = new DateTime(1990, 5, 15),
            ClubIds = new List<int> { _club!.Id },
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };

        var response = await client.PostAsJsonAsync("/api/delegado", delegadoDTO);

        response.EnsureSuccessStatusCode();

        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<DelegadoDTO>(stringResponse);

        Assert.NotNull(content);
        Assert.Equal("Juan", content.Nombre);
        Assert.Equal("Pérez", content.Apellido);
        Assert.Contains(_club.Id, content.ClubIds);

        // Aprobar delegado para crear el usuario
        var delegadoClubId = content.DelegadoClubs.First().Id;
        var aprobarResponse = await client.PostAsJsonAsync("/api/delegado/aprobar-delegado-en-el-club", new AprobarDelegadoEnElClubDTO
        {
            DelegadoClubId = delegadoClubId,
            DNI = content.DNI,
            Nombre = content.Nombre,
            Apellido = content.Apellido,
            FechaNacimiento = content.FechaNacimiento,
            TelefonoCelular = content.TelefonoCelular,
            Email = content.Email
        });
        aprobarResponse.EnsureSuccessStatusCode();

        // Verificar el nombre de usuario generado
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.DelegadoId == content.Id);
        
        Assert.NotNull(usuario);
        Assert.Equal("jperez", usuario.NombreUsuario);
    }

    [Fact]
    public async Task EliminarDelegado_ConUsuarioAsociado_BorraTambienElUsuario()
    {
        var client = await GetAuthenticatedClient();

        var delegadoDTO = new DelegadoDTO
        {
            DNI = "99998888",
            Nombre = "Eliminar",
            Apellido = "Usuario",
            FechaNacimiento = new DateTime(1990, 1, 1),
            ClubIds = new List<int> { _club!.Id },
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };

        var createResponse = await client.PostAsJsonAsync("/api/delegado", delegadoDTO);
        createResponse.EnsureSuccessStatusCode();
        var delegadoCreado = JsonConvert.DeserializeObject<DelegadoDTO>(await createResponse.Content.ReadAsStringAsync())!;

        var aprobarResponse = await client.PostAsJsonAsync("/api/delegado/aprobar-delegado-en-el-club", new AprobarDelegadoEnElClubDTO
        {
            DelegadoClubId = delegadoCreado.DelegadoClubs.First().Id,
            DNI = delegadoCreado.DNI,
            Nombre = delegadoCreado.Nombre,
            Apellido = delegadoCreado.Apellido,
            FechaNacimiento = delegadoCreado.FechaNacimiento,
            TelefonoCelular = delegadoCreado.TelefonoCelular,
            Email = delegadoCreado.Email
        });
        aprobarResponse.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.DelegadoId == delegadoCreado.Id);
            Assert.NotNull(usuario);
        }

        var deleteResponse = await client.DeleteAsync($"/api/delegado/{delegadoCreado.Id}");
        deleteResponse.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var delegadoEliminado = context.Delegados.Find(delegadoCreado.Id);
            var usuarioEliminado = await context.Usuarios.FirstOrDefaultAsync(u => u.DelegadoId == delegadoCreado.Id);

            Assert.Null(delegadoEliminado);
            Assert.Null(usuarioEliminado);
        }
    }

    /// <summary>
    /// Al eliminar un delegado cuyo club tiene varios equipos en la misma TorneoZona,
    /// el Include profundo de DelegadoRepo carga la misma TorneoZona varias veces.
    /// EF falla con: "another instance with the same key value for {'Id'} is already being tracked".
    /// </summary>
    [Fact]
    public async Task EliminarDelegado_ClubConEquiposEnMismaZona_NoDebeFallarPorTorneoZonaDuplicado()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var torneo = new Torneo { Id = 0, Nombre = "Torneo Test", Anio = 2025, TorneoAgrupadorId = 1 };
        context.Torneos.Add(torneo);
        await context.SaveChangesAsync();

        var fase = new TorneoFase
        {
            Id = 0,
            TorneoId = torneo.Id,
            Numero = 1,
            FaseFormatoId = (int)FormatoDeLaFaseEnum.TodosContraTodos,
            FaseTipoDeVueltaId = (int)TipoVueltaDeLaFaseEnum.SoloIda,
            EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
            EsVisibleEnApp = true
        };
        context.TorneoFases.Add(fase);
        await context.SaveChangesAsync();

        var zona = new TorneoZona { Id = 0, TorneoFaseId = fase.Id, Nombre = "Zona única" };
        context.TorneoZonas.Add(zona);
        await context.SaveChangesAsync();

        var clubConZona = new Club { Id = 0, Nombre = "Club con equipos en zona" };
        context.Clubs.Add(clubConZona);
        await context.SaveChangesAsync();

        var equipo1 = new Equipo { Id = 0, Nombre = "Equipo A", ClubId = clubConZona.Id, ZonaActualId = zona.Id, Jugadores = new List<JugadorEquipo>() };
        var equipo2 = new Equipo { Id = 0, Nombre = "Equipo B", ClubId = clubConZona.Id, ZonaActualId = zona.Id, Jugadores = new List<JugadorEquipo>() };
        context.Equipos.Add(equipo1);
        context.Equipos.Add(equipo2);
        await context.SaveChangesAsync();

        var client = await GetAuthenticatedClient();
        var delegadoDTO = new DelegadoDTO
        {
            DNI = "11112222",
            Nombre = "Delegado",
            Apellido = "Zona",
            FechaNacimiento = new DateTime(1988, 4, 4),
            ClubIds = new List<int> { clubConZona.Id },
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };

        var createResponse = await client.PostAsJsonAsync("/api/delegado", delegadoDTO);
        createResponse.EnsureSuccessStatusCode();
        var delegadoCreado = JsonConvert.DeserializeObject<DelegadoDTO>(await createResponse.Content.ReadAsStringAsync())!;

        var aprobarResponse = await client.PostAsJsonAsync("/api/delegado/aprobar-delegado-en-el-club", new AprobarDelegadoEnElClubDTO
        {
            DelegadoClubId = delegadoCreado.DelegadoClubs!.First().Id,
            DNI = delegadoCreado.DNI,
            Nombre = delegadoCreado.Nombre,
            Apellido = delegadoCreado.Apellido,
            FechaNacimiento = delegadoCreado.FechaNacimiento,
            TelefonoCelular = delegadoCreado.TelefonoCelular,
            Email = delegadoCreado.Email
        });
        aprobarResponse.EnsureSuccessStatusCode();

        var deleteResponse = await client.DeleteAsync($"/api/delegado/{delegadoCreado.Id}");
        deleteResponse.EnsureSuccessStatusCode();

        var delegadoEliminado = context.Delegados.Find(delegadoCreado.Id);
        Assert.Null(delegadoEliminado);
    }

    [Fact]
    public async Task ObtenerDelegado_PorId_Funciona()
    {
        // Primero creamos un delegado
        var client = await GetAuthenticatedClient();
        
        var delegadoDTO = new DelegadoDTO
        {
            DNI = "23456789",
            Nombre = "Ana",
            Apellido = "García",
            FechaNacimiento = new DateTime(1985, 3, 20),
            ClubIds = new List<int> { _club!.Id },
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };

        var createResponse = await client.PostAsJsonAsync("/api/delegado", delegadoDTO);
        createResponse.EnsureSuccessStatusCode();
        var createContent = JsonConvert.DeserializeObject<DelegadoDTO>(await createResponse.Content.ReadAsStringAsync());

        // Luego obtenemos el delegado por su ID
        var response = await client.GetAsync($"/api/delegado/{createContent!.Id}");
        response.EnsureSuccessStatusCode();

        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<DelegadoDTO>(stringResponse);

        Assert.NotNull(content);
        Assert.Equal("Ana", content.Nombre);
        Assert.Equal("García", content.Apellido);
        Assert.Contains(_club.Id, content.ClubIds);
    }

    [Fact]
    public async Task ModificarDelegado_DatosCorrectos_200()
    {
        // Primero creamos un delegado
        var client = await GetAuthenticatedClient();
        
        var delegadoDTO = new DelegadoDTO
        {
            DNI = "34567890",
            Nombre = "Carlos",
            Apellido = "López",
            FechaNacimiento = new DateTime(1988, 11, 10),
            ClubIds = new List<int> { _club!.Id },
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };

        var createResponse = await client.PostAsJsonAsync("/api/delegado", delegadoDTO);
        createResponse.EnsureSuccessStatusCode();
        var createContent = JsonConvert.DeserializeObject<DelegadoDTO>(await createResponse.Content.ReadAsStringAsync());

        // Modificamos el delegado
        var delegadoModificadoDTO = new DelegadoDTO
        {
            Id = createContent!.Id,
            DNI = "34567890",
            Nombre = "CarlosModif",
            Apellido = "LópezModif",
            FechaNacimiento = new DateTime(1988, 11, 10),
            ClubIds = new List<int> { _club.Id },
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };

        var response = await client.PutAsJsonAsync($"/api/delegado/{createContent.Id}", delegadoModificadoDTO);
        response.EnsureSuccessStatusCode();

        // Verificamos que se haya modificado correctamente
        var getResponse = await client.GetAsync($"/api/delegado/{createContent.Id}");
        getResponse.EnsureSuccessStatusCode();

        var stringResponse = await getResponse.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<DelegadoDTO>(stringResponse);

        Assert.NotNull(content);
        Assert.Equal("CarlosModif", content.Nombre);
        Assert.Equal("LópezModif", content.Apellido);
        Assert.Contains(_club.Id, content.ClubIds);
    }

    [Fact]
    public async Task ListarDelegados_ConJugadorMismoDNI_DevuelveJugadorId()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var jugador = new Jugador
        {
            Id = 6001,
            DNI = "55556666",
            Nombre = "Pedro",
            Apellido = "Delegado",
            FechaNacimiento = new DateTime(1992, 1, 1)
        };
        context.Jugadores.Add(jugador);
        context.SaveChanges();

        context.JugadorEquipo.Add(new JugadorEquipo
        {
            Id = 6011,
            JugadorId = jugador.Id,
            EquipoId = _equipo!.Id,
            FechaFichaje = DateTime.Now,
            EstadoJugadorId = (int)EstadoJugadorEnum.Activo
        });

        var delegadoPedro = new Delegado
        {
            Id = 6021,
            DNI = "55556666",
            Nombre = "Pedro",
            Apellido = "Delegado",
            FechaNacimiento = new DateTime(1992, 1, 1)
        };
        context.Delegados.Add(delegadoPedro);
        context.SaveChanges();
        context.DelegadoClub.Add(new DelegadoClub { Id = 0, DelegadoId = delegadoPedro.Id, ClubId = _club!.Id, EstadoDelegadoId = (int)EstadoDelegadoEnum.Activo });
        context.SaveChanges();

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync("/api/Delegado");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<DelegadoDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);

        var delegadoConJugador = content.SingleOrDefault(d => d.DNI == "55556666");
        Assert.NotNull(delegadoConJugador);
        Assert.Equal(jugador.Id, delegadoConJugador.JugadorId);
    }

    [Fact]
    public async Task ObtenerDelegadoPorId_ConJugadorMismoDNI_DevuelveJugadorId()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var jugador = new Jugador
        {
            Id = 6031,
            DNI = "66667777",
            Nombre = "Laura",
            Apellido = "Delegada",
            FechaNacimiento = new DateTime(1995, 5, 10)
        };
        context.Jugadores.Add(jugador);
        context.SaveChanges();

        context.JugadorEquipo.Add(new JugadorEquipo
        {
            Id = 6041,
            JugadorId = jugador.Id,
            EquipoId = _equipo!.Id,
            FechaFichaje = DateTime.Now,
            EstadoJugadorId = (int)EstadoJugadorEnum.Activo
        });

        var delegado = new Delegado
        {
            Id = 6051,
            DNI = "66667777",
            Nombre = "Laura",
            Apellido = "Delegada",
            FechaNacimiento = new DateTime(1995, 5, 10)
        };
        context.Delegados.Add(delegado);
        context.SaveChanges();
        context.DelegadoClub.Add(new DelegadoClub { Id = 0, DelegadoId = delegado.Id, ClubId = _club!.Id, EstadoDelegadoId = (int)EstadoDelegadoEnum.Activo });
        context.SaveChanges();

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/Delegado/{delegado.Id}");
        response.EnsureSuccessStatusCode();

        var dto = JsonConvert.DeserializeObject<DelegadoDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(dto);
        Assert.Equal(jugador.Id, dto.JugadorId);
    }

    /// <summary>
    /// Aprobar un delegado en el segundo club cuando ya fue aprobado en el primero.
    /// El delegado comparte Usuario entre clubs; no debe intentar crear un segundo Usuario.
    /// </summary>
    [Fact]
    public async Task AprobarDelegadoEnElClub_DelegadoYaAprobadoEnOtroClub_200()
    {
        var client = await GetAuthenticatedClient();

        var delegadoDTO = new DelegadoDTO
        {
            DNI = "77778888",
            Nombre = "Multi",
            Apellido = "Club",
            FechaNacimiento = new DateTime(1985, 7, 20),
            ClubIds = new List<int> { _club!.Id, _club2!.Id },
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };

        var createResponse = await client.PostAsJsonAsync("/api/delegado", delegadoDTO);
        createResponse.EnsureSuccessStatusCode();
        var created = JsonConvert.DeserializeObject<DelegadoDTO>(await createResponse.Content.ReadAsStringAsync())!;

        var delegadoClubs = created.DelegadoClubs!.OrderBy(dc => dc.ClubId).ToList();
        var delegadoClub1 = delegadoClubs.First(dc => dc.ClubId == _club.Id);
        var delegadoClub2 = delegadoClubs.First(dc => dc.ClubId == _club2.Id);

        var aprobar1 = await client.PostAsJsonAsync("/api/delegado/aprobar-delegado-en-el-club", new AprobarDelegadoEnElClubDTO
        {
            DelegadoClubId = delegadoClub1.Id,
            DNI = created.DNI,
            Nombre = created.Nombre,
            Apellido = created.Apellido,
            FechaNacimiento = created.FechaNacimiento,
            TelefonoCelular = created.TelefonoCelular,
            Email = created.Email
        });
        aprobar1.EnsureSuccessStatusCode();

        var aprobar2 = await client.PostAsJsonAsync("/api/delegado/aprobar-delegado-en-el-club", new AprobarDelegadoEnElClubDTO
        {
            DelegadoClubId = delegadoClub2.Id,
            DNI = created.DNI,
            Nombre = created.Nombre,
            Apellido = created.Apellido,
            FechaNacimiento = created.FechaNacimiento,
            TelefonoCelular = created.TelefonoCelular,
            Email = created.Email
        });
        aprobar2.EnsureSuccessStatusCode();

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dc1 = await context.DelegadoClub.FindAsync(delegadoClub1.Id);
        var dc2 = await context.DelegadoClub.FindAsync(delegadoClub2.Id);
        Assert.NotNull(dc1);
        Assert.NotNull(dc2);
        Assert.Equal((int)EstadoDelegadoEnum.Activo, dc1.EstadoDelegadoId);
        Assert.Equal((int)EstadoDelegadoEnum.Activo, dc2.EstadoDelegadoId);
    }

    [Fact]
    public async Task ListarDelegados_SinJugadorMismoDNI_JugadorIdEsNull()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var delegadoEntidad = new Delegado
        {
            Id = 6061,
            DNI = "88889999",
            Nombre = "Solo",
            Apellido = "Delegado",
            FechaNacimiento = new DateTime(1988, 8, 8)
        };
        context.Delegados.Add(delegadoEntidad);
        context.SaveChanges();
        context.DelegadoClub.Add(new DelegadoClub { Id = 0, DelegadoId = delegadoEntidad.Id, ClubId = _club!.Id, EstadoDelegadoId = (int)EstadoDelegadoEnum.Activo });
        context.SaveChanges();

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync("/api/Delegado");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<DelegadoDTO>>(await response.Content.ReadAsStringAsync());
        var delegadoSolo = content?.SingleOrDefault(d => d.DNI == "88889999");
        Assert.NotNull(delegadoSolo);
        Assert.Null(delegadoSolo.JugadorId);
    }
} 