using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.DTOs.CambiosDeEstadoDelegado;
using Api.Core.DTOs.CambiosDeEstadoJugador;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Logica;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Api.TestsUtilidades;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion;

/// <summary>
/// Tests de integración para el flujo de fichaje de delegados:
/// - POST Delegados (crear con fotos completas)
/// - POST delegados/fichar-delegado-solo-con-dni-y-club (crear cuando los datos ya existen)
/// </summary>
public class FichajeDelegadosIT : TestBase
{
    private const string FotoBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";

    private Utilidades? _utilidades;
    private Club? _club1;
    private Club? _club2;
    private Equipo? _equipo1;

    public FichajeDelegadosIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedData(context);
    }

    private void SeedData(AppDbContext context)
    {
        _utilidades = new Utilidades(context);
        _club1 = _utilidades.DadoQueExisteElClub();
        context.SaveChanges();
        _club2 = _utilidades.DadoQueExisteElClub();
        context.SaveChanges();
        _equipo1 = _utilidades.DadoQueExisteElEquipo(_club1);
        context.SaveChanges();
    }

    /// <summary>
    /// Un delegado, cuyos datos no existen en el sistema, intenta ficharse con "POST Delegados" con éxito.
    /// </summary>
    [Fact]
    public async Task Delegado_DatosNoExisten_POST_Delegados_Exito()
    {
        var client = Factory.CreateClient();
        var delegadoDTO = new DelegadoDTO
        {
            DNI = "11111111",
            Nombre = "Nuevo",
            Apellido = "Delegado",
            FechaNacimiento = new DateTime(1990, 5, 15),
            ClubId = _club1!.Id,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };

        var response = await client.PostAsJsonAsync("/api/delegado", delegadoDTO);

        response.EnsureSuccessStatusCode();
        var content = JsonConvert.DeserializeObject<DelegadoDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal("Nuevo", content.Nombre);
        Assert.Equal(_club1.Id, content.ClubId);
    }

    /// <summary>
    /// Un delegado con mismo DNI pero PENDIENTE - POST Delegados arroja error porque DNI es único en Delegados.
    /// </summary>
    [Fact]
    public async Task Delegado_DatosExistenComoDelegadoPendiente_POST_Delegados_ArrojaError()
    {
        // Crear un delegado SIN aprobar
        var client = Factory.CreateClient();
        var delegadoDTO = new DelegadoDTO
        {
            DNI = "21111111",
            Nombre = "Pendiente",
            Apellido = "Delegado",
            FechaNacimiento = new DateTime(1988, 3, 10),
            ClubId = _club1!.Id,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };
        var createResponse = await client.PostAsJsonAsync("/api/delegado", delegadoDTO);
        createResponse.EnsureSuccessStatusCode();

        // Intentar crear otro delegado con el mismo DNI en club2 vía POST normal → debe fallar (DNI único)
        var duplicadoDTO = new DelegadoDTO
        {
            DNI = "21111111",
            Nombre = "Segundo",
            Apellido = "Delegado",
            FechaNacimiento = new DateTime(1988, 3, 10),
            ClubId = _club2!.Id,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };

        var response = await client.PostAsJsonAsync("/api/delegado", duplicadoDTO);

        Assert.False(response.IsSuccessStatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("pendiente de aprobación como delegado", errorContent);
    }

    /// <summary>
    /// Un delegado con mismo DNI de jugador PENDIENTE - POST Delegados tiene éxito porque los pendientes no "existen".
    /// </summary>
    [Fact]
    public async Task Delegado_DatosExistenComoJugadorPendiente_POST_Delegados_Exito()
    {
        // Crear un jugador SIN aprobar (pendiente no "existe")
        var client = Factory.CreateClient();
        var codigo = GeneradorDeHash.GenerarAlfanumerico7Digitos(_equipo1!.Id);
        var jugadorDTO = new JugadorDTO
        {
            DNI = "31111111",
            Nombre = "Pendiente",
            Apellido = "Jugador",
            FechaNacimiento = new DateTime(1995, 1, 1),
            CodigoAlfanumerico = codigo,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };
        var createJugadorResponse = await client.PostAsJsonAsync("/api/jugador", jugadorDTO);
        createJugadorResponse.EnsureSuccessStatusCode();

        // Crear delegado con el mismo DNI vía POST normal → debe tener éxito (jugador pendiente no bloquea)
        var delegadoDTO = new DelegadoDTO
        {
            DNI = "31111111",
            Nombre = "Delegado",
            Apellido = "Intent",
            FechaNacimiento = new DateTime(1995, 1, 1),
            ClubId = _club1!.Id,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };

        var response = await client.PostAsJsonAsync("/api/delegado", delegadoDTO);

        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Juan Pérez y José Pérez aprobados generan usuarios distintos: jperez y joperez (evita colisión).
    /// </summary>
    [Fact]
    public async Task Delegado_JuanPerezYJosePerez_Aprobados_GeneranUsuariosDistintos()
    {
        var client = await GetAuthenticatedClient();

        // Crear y aprobar Juan Pérez → usuario "jperez"
        var juanDTO = new DelegadoDTO
        {
            DNI = "60111111",
            Nombre = "Juan",
            Apellido = "Pérez",
            FechaNacimiento = new DateTime(1985, 1, 1),
            ClubId = _club1!.Id,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };
        var createJuan = await client.PostAsJsonAsync("/api/delegado", juanDTO);
        createJuan.EnsureSuccessStatusCode();
        var juanCreado = JsonConvert.DeserializeObject<DelegadoDTO>(await createJuan.Content.ReadAsStringAsync())!;
        var aprobarJuan = await client.PostAsJsonAsync("/api/delegado/aprobar", new AprobarDelegadoDTO { Id = juanCreado.Id });
        aprobarJuan.EnsureSuccessStatusCode();

        // Crear y aprobar José Pérez → usuario "joperez" (jperez ya existe)
        var joseDTO = new DelegadoDTO
        {
            DNI = "60222222",
            Nombre = "José",
            Apellido = "Pérez",
            FechaNacimiento = new DateTime(1986, 2, 2),
            ClubId = _club1.Id,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };
        var createJose = await client.PostAsJsonAsync("/api/delegado", joseDTO);
        createJose.EnsureSuccessStatusCode();
        var joseCreado = JsonConvert.DeserializeObject<DelegadoDTO>(await createJose.Content.ReadAsStringAsync())!;
        var aprobarJose = await client.PostAsJsonAsync("/api/delegado/aprobar", new AprobarDelegadoDTO { Id = joseCreado.Id });
        aprobarJose.EnsureSuccessStatusCode();

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var usuarios = context.Usuarios
            .Where(u => u.NombreUsuario == "jperez" || u.NombreUsuario == "joperez")
            .Select(u => u.NombreUsuario)
            .ToList();
        Assert.Equal(2, usuarios.Count);
        Assert.Contains("jperez", usuarios);
        Assert.Contains("joperez", usuarios);
    }

    /// <summary>
    /// Un delegado, cuyos datos ya existen en el sistema (es delegado de otro club, APROBADOS), intenta ficharse con "POST Delegados" y arroja error.
    /// </summary>
    [Fact]
    public async Task Delegado_DatosExistenComoDelegadoOtroClub_POST_Delegados_ArrojaError()
    {
        // Crear y aprobar un delegado en club1 para que tenga fotos en definitivas
        var client = await GetAuthenticatedClient();
        var delegadoDTO = new DelegadoDTO
        {
            DNI = "22222222",
            Nombre = "Existente",
            Apellido = "Delegado",
            FechaNacimiento = new DateTime(1988, 3, 10),
            ClubId = _club1!.Id,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };
        var createResponse = await client.PostAsJsonAsync("/api/delegado", delegadoDTO);
        createResponse.EnsureSuccessStatusCode();
        var created = JsonConvert.DeserializeObject<DelegadoDTO>(await createResponse.Content.ReadAsStringAsync())!;

        var aprobarResponse = await client.PostAsJsonAsync("/api/delegado/aprobar", new AprobarDelegadoDTO { Id = created.Id });
        aprobarResponse.EnsureSuccessStatusCode();

        // Intentar crear otro delegado con el mismo DNI en club2 vía POST normal → debe fallar
        var clientAnon = Factory.CreateClient();
        var duplicadoDTO = new DelegadoDTO
        {
            DNI = "22222222",
            Nombre = "Duplicado",
            Apellido = "Intent",
            FechaNacimiento = new DateTime(1988, 3, 10),
            ClubId = _club2!.Id,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };

        var response = await clientAnon.PostAsJsonAsync("/api/delegado", duplicadoDTO);

        Assert.False(response.IsSuccessStatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("DNI ya existente en el sistema", errorContent);
    }

    /// <summary>
    /// Un delegado, cuyos datos ya existen en el sistema (es jugador APROBADOS), intenta ficharse con "POST Delegados" y arroja error.
    /// </summary>
    [Fact]
    public async Task Delegado_DatosExistenComoJugador_POST_Delegados_ArrojaError()
    {
        // Crear un jugador con DNI 33333333
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jugador = new Jugador
        {
            Id = 9001,
            DNI = "33333333",
            Nombre = "Jugador",
            Apellido = "Existente",
            FechaNacimiento = new DateTime(1995, 1, 1)
        };
        context.Jugadores.Add(jugador);
        context.JugadorEquipo.Add(new JugadorEquipo
        {
            Id = 9011,
            JugadorId = jugador.Id,
            EquipoId = _equipo1!.Id,
            FechaFichaje = DateTime.Now,
            EstadoJugadorId = (int)EstadoJugadorEnum.Activo
        });
        context.SaveChanges();

        // Intentar crear delegado con el mismo DNI vía POST normal → debe fallar
        var client = Factory.CreateClient();
        var delegadoDTO = new DelegadoDTO
        {
            DNI = "33333333",
            Nombre = "Delegado",
            Apellido = "Intent",
            FechaNacimiento = new DateTime(1995, 1, 1),
            ClubId = _club1!.Id,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };

        var response = await client.PostAsJsonAsync("/api/delegado", delegadoDTO);

        Assert.False(response.IsSuccessStatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("DNI ya existente en el sistema", errorContent);
    }

    /// <summary>
    /// Un delegado, cuyos datos ya existen en el sistema (es delegado de otro club), intenta ficharse con POST a "delegados/fichar-delegado-solo-con-dni-y-club" y tiene éxito.
    /// </summary>
    [Fact]
    public async Task Delegado_DatosExistenComoDelegadoOtroClub_POST_FicharSoloConDniYClub_Exito()
    {
        // Crear y aprobar un delegado en club1
        var client = await GetAuthenticatedClient();
        var delegadoDTO = new DelegadoDTO
        {
            DNI = "44444444",
            Nombre = "Delegado",
            Apellido = "MultiClub",
            FechaNacimiento = new DateTime(1985, 7, 20),
            ClubId = _club1!.Id,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };
        var createResponse = await client.PostAsJsonAsync("/api/delegado", delegadoDTO);
        createResponse.EnsureSuccessStatusCode();
        var created = JsonConvert.DeserializeObject<DelegadoDTO>(await createResponse.Content.ReadAsStringAsync())!;

        var aprobarResponse = await client.PostAsJsonAsync("/api/delegado/aprobar", new AprobarDelegadoDTO { Id = created.Id });
        aprobarResponse.EnsureSuccessStatusCode();

        // Fichar en club2 vía flujo "solo con DNI" → debe tener éxito
        var ficharDTO = new FicharDelegadoSoloConDniYClubDTO { DNI = "44444444", ClubId = _club2!.Id };
        var response = await client.PostAsJsonAsync("/api/delegado/fichar-delegado-solo-con-dni-y-club", ficharDTO);

        response.EnsureSuccessStatusCode();
        var nuevoId = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
        Assert.True(nuevoId > 0);

        using var verifyScope = Factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var nuevoDelegado = await verifyContext.Delegados.FindAsync(nuevoId);
        Assert.NotNull(nuevoDelegado);
        Assert.Equal(_club2.Id, nuevoDelegado.ClubId);
        Assert.Equal((int)EstadoDelegadoEnum.PendienteDeAprobacion, nuevoDelegado.EstadoDelegadoId);
    }

    /// <summary>
    /// Un delegado, cuyos datos ya existen en el sistema (es jugador), intenta ficharse con POST a "delegados/fichar-delegado-solo-con-dni-y-club" y tiene éxito.
    /// </summary>
    [Fact]
    public async Task Delegado_DatosExistenComoJugador_POST_FicharSoloConDniYClub_Exito()
    {
        // Crear y aprobar un jugador para que tenga fotos en definitivas
        var client = await GetAuthenticatedClient();
        var codigo = GeneradorDeHash.GenerarAlfanumerico7Digitos(_equipo1!.Id);
        var jugadorDTO = new JugadorDTO
        {
            DNI = "55555555",
            Nombre = "Jugador",
            Apellido = "Delegado",
            FechaNacimiento = new DateTime(1992, 4, 12),
            CodigoAlfanumerico = codigo,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };
        var createJugadorResponse = await client.PostAsJsonAsync("/api/jugador", jugadorDTO);
        createJugadorResponse.EnsureSuccessStatusCode();
        var jugadorCreado = JsonConvert.DeserializeObject<JugadorDTO>(await createJugadorResponse.Content.ReadAsStringAsync())!;

        int jugadorEquipoId;
        using (var scope = Factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var je = ctx.JugadorEquipo.First(x => x.JugadorId == jugadorCreado.Id);
            jugadorEquipoId = je.Id;
        }

        var aprobarResponse = await client.PostAsJsonAsync("/api/jugador/aprobar-jugador", new AprobarJugadorDTO
        {
            Id = jugadorCreado.Id,
            DNI = jugadorCreado.DNI,
            Nombre = jugadorCreado.Nombre,
            Apellido = jugadorCreado.Apellido,
            FechaNacimiento = jugadorCreado.FechaNacimiento,
            JugadorEquipoId = jugadorEquipoId
        });
        aprobarResponse.EnsureSuccessStatusCode();

        // Fichar como delegado vía flujo "solo con DNI" (jugador no tiene email/telefono, los pasamos)
        var ficharDTO = new FicharDelegadoSoloConDniYClubDTO
        {
            DNI = "55555555",
            ClubId = _club1!.Id,
            Email = "jugador@test.com",
            TelefonoCelular = "1234567890"
        };
        var response = await client.PostAsJsonAsync("/api/delegado/fichar-delegado-solo-con-dni-y-club", ficharDTO);

        response.EnsureSuccessStatusCode();
        var nuevoId = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
        Assert.True(nuevoId > 0);

        using var verifyScope = Factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var nuevoDelegado = await verifyContext.Delegados.FindAsync(nuevoId);
        Assert.NotNull(nuevoDelegado);
        Assert.Equal("jugador@test.com", nuevoDelegado.Email);
        Assert.Equal("1234567890", nuevoDelegado.TelefonoCelular);
    }

    /// <summary>
    /// Un delegado PENDIENTE (no aprobado) intenta ficharse con POST a "fichar-delegado-solo-con-dni-y-club" y arroja error explícito.
    /// La administración debe aprobarlo antes de poder fichar.
    /// </summary>
    [Fact]
    public async Task Delegado_DatosExistenComoDelegadoPendiente_POST_FicharSoloConDniYClub_ArrojaErrorPendienteDeAprobacion()
    {
        // Crear un delegado SIN aprobar
        var client = Factory.CreateClient();
        var delegadoDTO = new DelegadoDTO
        {
            DNI = "77777777",
            Nombre = "Pendiente",
            Apellido = "Delegado",
            FechaNacimiento = new DateTime(1985, 1, 1),
            ClubId = _club1!.Id,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };
        var createResponse = await client.PostAsJsonAsync("/api/delegado", delegadoDTO);
        createResponse.EnsureSuccessStatusCode();

        // Intentar fichar-delegado-solo-con-dni-y-club con ese DNI → debe fallar con mensaje explícito de pendiente
        var authClient = await GetAuthenticatedClient();
        var ficharDTO = new FicharDelegadoSoloConDniYClubDTO { DNI = "77777777", ClubId = _club2!.Id };

        var response = await authClient.PostAsJsonAsync("/api/delegado/fichar-delegado-solo-con-dni-y-club", ficharDTO);

        Assert.False(response.IsSuccessStatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("pendiente de aprobación como delegado", errorContent);
        Assert.Contains("administración debe aprobarlo", errorContent);
    }

    /// <summary>
    /// Un jugador PENDIENTE intenta ficharse como delegado con POST a "fichar-delegado-solo-con-dni-y-club" y arroja error explícito.
    /// </summary>
    [Fact]
    public async Task Jugador_DatosExistenComoJugadorPendiente_POST_FicharSoloConDniYClub_ArrojaErrorPendienteDeAprobacion()
    {
        // Crear un jugador SIN aprobar
        var client = Factory.CreateClient();
        var codigo = GeneradorDeHash.GenerarAlfanumerico7Digitos(_equipo1!.Id);
        var jugadorDTO = new JugadorDTO
        {
            DNI = "88888888",
            Nombre = "Pendiente",
            Apellido = "Jugador",
            FechaNacimiento = new DateTime(1991, 1, 1),
            CodigoAlfanumerico = codigo,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };
        var createResponse = await client.PostAsJsonAsync("/api/jugador", jugadorDTO);
        createResponse.EnsureSuccessStatusCode();

        // Intentar fichar como delegado con ese DNI → debe fallar con mensaje explícito de jugador pendiente
        var authClient = await GetAuthenticatedClient();
        var ficharDTO = new FicharDelegadoSoloConDniYClubDTO { DNI = "88888888", ClubId = _club1!.Id, Email = "test@test.com", TelefonoCelular = "123" };

        var response = await authClient.PostAsJsonAsync("/api/delegado/fichar-delegado-solo-con-dni-y-club", ficharDTO);

        Assert.False(response.IsSuccessStatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("pendiente de aprobación como jugador", errorContent);
        Assert.Contains("administración debe aprobarlo", errorContent);
    }

    /// <summary>
    /// Un delegado, cuyos datos no existen en el sistema, intenta ficharse con POST a "delegados/fichar-delegado-solo-con-dni-y-club" y arroja error.
    /// </summary>
    [Fact]
    public async Task Delegado_DatosNoExisten_POST_FicharSoloConDniYClub_ArrojaError()
    {
        var client = await GetAuthenticatedClient();
        var ficharDTO = new FicharDelegadoSoloConDniYClubDTO { DNI = "99999999", ClubId = _club1!.Id };

        var response = await client.PostAsJsonAsync("/api/delegado/fichar-delegado-solo-con-dni-y-club", ficharDTO);

        Assert.False(response.IsSuccessStatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("No existe ni un delegado ni un jugador", errorContent);
    }
}
