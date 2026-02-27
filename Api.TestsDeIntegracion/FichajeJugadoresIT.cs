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
/// Tests de integración para el flujo de fichaje de jugadores:
/// - POST Jugador (crear con fotos completas)
/// - POST publico/fichar-en-otro-equipo (fichar cuando los datos ya existen)
/// </summary>
public class FichajeJugadoresIT : TestBase
{
    private const string FotoBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";

    private Utilidades? _utilidades;
    private Club? _club1;
    private Club? _club2;
    private Equipo? _equipo1;
    private Equipo? _equipo2;

    public FichajeJugadoresIT(CustomWebApplicationFactory<Program> factory) : base(factory)
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
        _equipo2 = _utilidades.DadoQueExisteElEquipo(_club2);
        context.SaveChanges();
    }

    /// <summary>
    /// Un jugador, cuyos datos no existen en el sistema, intenta ficharse con "POST Jugador" con éxito.
    /// </summary>
    [Fact]
    public async Task Jugador_DatosNoExisten_POST_Jugador_Exito()
    {
        var client = Factory.CreateClient();
        var codigo = GeneradorDeHash.GenerarAlfanumerico7Digitos(_equipo1!.Id);
        var jugadorDTO = new JugadorDTO
        {
            DNI = "11111111",
            Nombre = "Nuevo",
            Apellido = "Jugador",
            FechaNacimiento = new DateTime(2000, 1, 15),
            CodigoAlfanumerico = codigo,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };

        var response = await client.PostAsJsonAsync("/api/jugador", jugadorDTO);

        response.EnsureSuccessStatusCode();
        var content = JsonConvert.DeserializeObject<JugadorDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal("Nuevo", content.Nombre);
    }

    /// <summary>
    /// Un jugador con mismo DNI de delegado PENDIENTE - POST Jugador arroja error.
    /// </summary>
    [Fact]
    public async Task Jugador_DatosExistenComoDelegadoPendiente_POST_Jugador_ArrojaError()
    {
        // Crear un delegado SIN aprobar
        var client = Factory.CreateClient();
        var delegadoDTO = new DelegadoDTO
        {
            DNI = "21111111",
            Nombre = "Pendiente",
            Apellido = "Delegado",
            FechaNacimiento = new DateTime(1990, 5, 10),
            ClubId = _club1!.Id,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };
        var createDelegadoResponse = await client.PostAsJsonAsync("/api/delegado", delegadoDTO);
        createDelegadoResponse.EnsureSuccessStatusCode();

        // Intentar crear jugador con el mismo DNI vía POST normal → debe fallar
        var codigo = GeneradorDeHash.GenerarAlfanumerico7Digitos(_equipo1!.Id);
        var jugadorDTO = new JugadorDTO
        {
            DNI = "21111111",
            Nombre = "Jugador",
            Apellido = "Intent",
            FechaNacimiento = new DateTime(1990, 5, 10),
            CodigoAlfanumerico = codigo,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };

        var response = await client.PostAsJsonAsync("/api/jugador", jugadorDTO);

        Assert.False(response.IsSuccessStatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("pendiente de aprobación como delegado", errorContent);
        Assert.Contains("fichar como jugador", errorContent);
    }

    /// <summary>
    /// Un jugador con mismo DNI de jugador PENDIENTE - POST Jugador arroja error porque DNI es único en Jugadores.
    /// </summary>
    [Fact]
    public async Task Jugador_DatosExistenComoJugadorPendiente_POST_Jugador_ArrojaError()
    {
        // Crear un jugador SIN aprobar en equipo1
        var client = Factory.CreateClient();
        var codigo1 = GeneradorDeHash.GenerarAlfanumerico7Digitos(_equipo1!.Id);
        var jugadorDTO = new JugadorDTO
        {
            DNI = "31111111",
            Nombre = "Pendiente",
            Apellido = "Jugador",
            FechaNacimiento = new DateTime(1995, 3, 20),
            CodigoAlfanumerico = codigo1,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };
        var createResponse = await client.PostAsJsonAsync("/api/jugador", jugadorDTO);
        createResponse.EnsureSuccessStatusCode();

        // Intentar crear otro jugador con el mismo DNI en equipo2 vía POST normal → debe fallar (DNI único)
        var codigo2 = GeneradorDeHash.GenerarAlfanumerico7Digitos(_equipo2!.Id);
        var duplicadoDTO = new JugadorDTO
        {
            DNI = "31111111",
            Nombre = "Segundo",
            Apellido = "Jugador",
            FechaNacimiento = new DateTime(1995, 3, 20),
            CodigoAlfanumerico = codigo2,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };

        var response = await client.PostAsJsonAsync("/api/jugador", duplicadoDTO);

        Assert.False(response.IsSuccessStatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("pendiente de aprobación como jugador", errorContent);
    }

    /// <summary>
    /// Un jugador, cuyos datos ya existen en el sistema (es delegado de algún club, APROBADOS), intenta ficharse con "POST Jugador" y arroja error.
    /// </summary>
    [Fact]
    public async Task Jugador_DatosExistenComoDelegado_POST_Jugador_ArrojaError()
    {
        // Crear y aprobar un delegado (solo los aprobados "existen")
        var client = await GetAuthenticatedClient();
        var delegadoDTO = new DelegadoDTO
        {
            DNI = "22222222",
            Nombre = "Delegado",
            Apellido = "Existente",
            FechaNacimiento = new DateTime(1990, 5, 10),
            ClubId = _club1!.Id,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };
        var createDelegadoResponse = await client.PostAsJsonAsync("/api/delegado", delegadoDTO);
        createDelegadoResponse.EnsureSuccessStatusCode();
        var delegadoCreado = JsonConvert.DeserializeObject<DelegadoDTO>(await createDelegadoResponse.Content.ReadAsStringAsync())!;
        var aprobarResponse = await client.PostAsJsonAsync("/api/delegado/aprobar", new AprobarDelegadoDTO { Id = delegadoCreado.Id });
        aprobarResponse.EnsureSuccessStatusCode();

        // Intentar crear jugador con el mismo DNI vía POST normal → debe fallar
        var clientAnon = Factory.CreateClient();
        var codigo = GeneradorDeHash.GenerarAlfanumerico7Digitos(_equipo1!.Id);
        var jugadorDTO = new JugadorDTO
        {
            DNI = "22222222",
            Nombre = "Jugador",
            Apellido = "Intent",
            FechaNacimiento = new DateTime(1990, 5, 10),
            CodigoAlfanumerico = codigo,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };

        var response = await clientAnon.PostAsJsonAsync("/api/jugador", jugadorDTO);

        Assert.False(response.IsSuccessStatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("DNI ya existente en el sistema", errorContent);
    }

    /// <summary>
    /// Un jugador, cuyos datos ya existen en el sistema (es jugador de otro equipo, APROBADOS), intenta ficharse con "POST Jugador" y arroja error.
    /// </summary>
    [Fact]
    public async Task Jugador_DatosExistenComoJugadorOtroEquipo_POST_Jugador_ArrojaError()
    {
        // Crear y aprobar un jugador en equipo1 (solo los aprobados "existen")
        var client = await GetAuthenticatedClient();
        var codigo1 = GeneradorDeHash.GenerarAlfanumerico7Digitos(_equipo1!.Id);
        var jugadorDTO = new JugadorDTO
        {
            DNI = "33333333",
            Nombre = "Jugador",
            Apellido = "Existente",
            FechaNacimiento = new DateTime(1995, 3, 20),
            CodigoAlfanumerico = codigo1,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };
        var createResponse = await client.PostAsJsonAsync("/api/jugador", jugadorDTO);
        createResponse.EnsureSuccessStatusCode();
        var jugadorCreado = JsonConvert.DeserializeObject<JugadorDTO>(await createResponse.Content.ReadAsStringAsync())!;

        int jugadorEquipoId;
        using (var scope = Factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            jugadorEquipoId = ctx.JugadorEquipo.First(x => x.JugadorId == jugadorCreado.Id).Id;
        }
        await client.PostAsJsonAsync("/api/jugador/aprobar-jugador", new AprobarJugadorDTO
        {
            Id = jugadorCreado.Id,
            DNI = jugadorCreado.DNI,
            Nombre = jugadorCreado.Nombre,
            Apellido = jugadorCreado.Apellido,
            FechaNacimiento = jugadorCreado.FechaNacimiento,
            JugadorEquipoId = jugadorEquipoId
        });

        // Intentar crear otro jugador con el mismo DNI en equipo2 vía POST normal → debe fallar
        var codigo2 = GeneradorDeHash.GenerarAlfanumerico7Digitos(_equipo2!.Id);
        var duplicadoDTO = new JugadorDTO
        {
            DNI = "33333333",
            Nombre = "Duplicado",
            Apellido = "Intent",
            FechaNacimiento = new DateTime(1995, 3, 20),
            CodigoAlfanumerico = codigo2,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };

        var response = await client.PostAsJsonAsync("/api/jugador", duplicadoDTO);

        Assert.False(response.IsSuccessStatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("DNI ya existente en el sistema", errorContent);
    }

    /// <summary>
    /// Un jugador, cuyos datos ya existen en el sistema (es delegado de algún club), intenta ficharse con POST a "publico/fichar-en-otro-equipo" y tiene éxito.
    /// </summary>
    [Fact]
    public async Task Jugador_DatosExistenComoDelegado_POST_FicharEnOtroEquipo_Exito()
    {
        // Crear y aprobar un delegado para que tenga fotos en definitivas
        var client = await GetAuthenticatedClient();
        var delegadoDTO = new DelegadoDTO
        {
            DNI = "44444444",
            Nombre = "Delegado",
            Apellido = "Jugador",
            FechaNacimiento = new DateTime(1988, 8, 8),
            ClubId = _club1!.Id,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };
        var createDelegadoResponse = await client.PostAsJsonAsync("/api/delegado", delegadoDTO);
        createDelegadoResponse.EnsureSuccessStatusCode();
        var delegadoCreado = JsonConvert.DeserializeObject<DelegadoDTO>(await createDelegadoResponse.Content.ReadAsStringAsync())!;

        var aprobarResponse = await client.PostAsJsonAsync("/api/delegado/aprobar", new AprobarDelegadoDTO { Id = delegadoCreado.Id });
        aprobarResponse.EnsureSuccessStatusCode();

        // Fichar como jugador en equipo2 vía fichar-en-otro-equipo → debe tener éxito
        var clientAnon = Factory.CreateClient();
        var codigo = GeneradorDeHash.GenerarAlfanumerico7Digitos(_equipo2!.Id);
        var ficharDTO = new FicharEnOtroEquipoDTO { DNI = "44444444", CodigoAlfanumerico = codigo };

        var response = await clientAnon.PostAsJsonAsync("/api/publico/fichar-en-otro-equipo", ficharDTO);

        response.EnsureSuccessStatusCode();
        var jugadorId = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
        Assert.True(jugadorId > 0);

        using var verifyScope = Factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jugador = await verifyContext.Jugadores.FindAsync(jugadorId);
        Assert.NotNull(jugador);
        Assert.True(verifyContext.JugadorEquipo.Any(je => je.JugadorId == jugadorId && je.EquipoId == _equipo2.Id));
    }

    /// <summary>
    /// Un jugador, cuyos datos ya existen en el sistema (es jugador de otro equipo, APROBADOS), intenta ficharse con POST a "publico/fichar-en-otro-equipo" y tiene éxito.
    /// </summary>
    [Fact]
    public async Task Jugador_DatosExistenComoJugadorOtroEquipo_POST_FicharEnOtroEquipo_Exito()
    {
        // Crear y aprobar un jugador en equipo1 (solo los aprobados "existen" para fichar-en-otro-equipo)
        var client = await GetAuthenticatedClient();
        var codigo1 = GeneradorDeHash.GenerarAlfanumerico7Digitos(_equipo1!.Id);
        var jugadorDTO = new JugadorDTO
        {
            DNI = "55555555",
            Nombre = "Jugador",
            Apellido = "MultiEquipo",
            FechaNacimiento = new DateTime(1992, 6, 15),
            CodigoAlfanumerico = codigo1,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };
        var createResponse = await client.PostAsJsonAsync("/api/jugador", jugadorDTO);
        createResponse.EnsureSuccessStatusCode();
        var jugadorCreado = JsonConvert.DeserializeObject<JugadorDTO>(await createResponse.Content.ReadAsStringAsync())!;

        int jugadorEquipoId;
        using (var scope = Factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            jugadorEquipoId = ctx.JugadorEquipo.First(x => x.JugadorId == jugadorCreado.Id).Id;
        }
        await client.PostAsJsonAsync("/api/jugador/aprobar-jugador", new AprobarJugadorDTO
        {
            Id = jugadorCreado.Id,
            DNI = jugadorCreado.DNI,
            Nombre = jugadorCreado.Nombre,
            Apellido = jugadorCreado.Apellido,
            FechaNacimiento = jugadorCreado.FechaNacimiento,
            JugadorEquipoId = jugadorEquipoId
        });

        // Fichar en equipo2 vía fichar-en-otro-equipo → debe tener éxito
        var codigo2 = GeneradorDeHash.GenerarAlfanumerico7Digitos(_equipo2!.Id);
        var ficharDTO = new FicharEnOtroEquipoDTO { DNI = "55555555", CodigoAlfanumerico = codigo2 };

        var response = await client.PostAsJsonAsync("/api/publico/fichar-en-otro-equipo", ficharDTO);

        response.EnsureSuccessStatusCode();
        var jugadorId = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
        Assert.Equal(jugadorCreado.Id, jugadorId);

        using var verifyScope = Factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.True(verifyContext.JugadorEquipo.Count(je => je.JugadorId == jugadorId) == 2);
        Assert.True(verifyContext.JugadorEquipo.Any(je => je.JugadorId == jugadorId && je.EquipoId == _equipo2.Id));
    }

    /// <summary>
    /// Un jugador PENDIENTE (no aprobado) intenta ficharse con POST a "publico/fichar-en-otro-equipo" y arroja error explícito.
    /// La administración debe aprobarlo antes de poder fichar en otro equipo.
    /// </summary>
    [Fact]
    public async Task Jugador_DatosExistenComoJugadorPendiente_POST_FicharEnOtroEquipo_ArrojaErrorPendienteDeAprobacion()
    {
        // Crear un jugador SIN aprobar
        var client = Factory.CreateClient();
        var codigo1 = GeneradorDeHash.GenerarAlfanumerico7Digitos(_equipo1!.Id);
        var jugadorDTO = new JugadorDTO
        {
            DNI = "66666666",
            Nombre = "Pendiente",
            Apellido = "Jugador",
            FechaNacimiento = new DateTime(1993, 2, 2),
            CodigoAlfanumerico = codigo1,
            FotoCarnet = FotoBase64,
            FotoDNIFrente = FotoBase64,
            FotoDNIDorso = FotoBase64
        };
        var createResponse = await client.PostAsJsonAsync("/api/jugador", jugadorDTO);
        createResponse.EnsureSuccessStatusCode();

        // Intentar fichar-en-otro-equipo con ese DNI → debe fallar con mensaje explícito de pendiente
        var codigo2 = GeneradorDeHash.GenerarAlfanumerico7Digitos(_equipo2!.Id);
        var ficharDTO = new FicharEnOtroEquipoDTO { DNI = "66666666", CodigoAlfanumerico = codigo2 };

        var response = await client.PostAsJsonAsync("/api/publico/fichar-en-otro-equipo", ficharDTO);

        Assert.False(response.IsSuccessStatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("pendiente de aprobación como jugador", errorContent);
        Assert.Contains("administración debe aprobarlo", errorContent);
    }

    /// <summary>
    /// Un delegado PENDIENTE intenta ficharse como jugador con POST a "fichar-en-otro-equipo" y arroja error explícito.
    /// </summary>
    [Fact]
    public async Task Delegado_DatosExistenComoDelegadoPendiente_POST_FicharEnOtroEquipo_ArrojaErrorPendienteDeAprobacion()
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

        // Intentar fichar-en-otro-equipo con ese DNI (crearía jugador desde delegado) → debe fallar con mensaje explícito
        var codigo = GeneradorDeHash.GenerarAlfanumerico7Digitos(_equipo2!.Id);
        var ficharDTO = new FicharEnOtroEquipoDTO { DNI = "77777777", CodigoAlfanumerico = codigo };

        var response = await client.PostAsJsonAsync("/api/publico/fichar-en-otro-equipo", ficharDTO);

        Assert.False(response.IsSuccessStatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("pendiente de aprobación como delegado", errorContent);
        Assert.Contains("administración debe aprobarlo", errorContent);
    }

    /// <summary>
    /// Un jugador, cuyos datos no existen en el sistema, intenta ficharse con POST a "publico/fichar-en-otro-equipo" y arroja error.
    /// </summary>
    [Fact]
    public async Task Jugador_DatosNoExisten_POST_FicharEnOtroEquipo_ArrojaError()
    {
        var client = Factory.CreateClient();
        var codigo = GeneradorDeHash.GenerarAlfanumerico7Digitos(_equipo1!.Id);
        var ficharDTO = new FicharEnOtroEquipoDTO { DNI = "99999999", CodigoAlfanumerico = codigo };

        var response = await client.PostAsJsonAsync("/api/publico/fichar-en-otro-equipo", ficharDTO);

        Assert.False(response.IsSuccessStatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("No existe ni un jugador ni un delegado", errorContent);
    }
}
