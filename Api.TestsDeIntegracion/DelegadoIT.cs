using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.DTOs.CambiosDeEstadoDelegado;
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

    public DelegadoIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        SeedData(context);
    }

    private void SeedData(AppDbContext context)
    {
        _utilidades = new Utilidades(context);
        _club = _utilidades.DadoQueExisteElClub();
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
            ClubId = _club!.Id,
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
        Assert.Equal(_club.Id, content.ClubId);

        // Aprobar delegado para crear el usuario
        var aprobarResponse = await client.PostAsJsonAsync("/api/delegado/aprobar-delegado", new AprobarDelegadoDTO { Id = content.Id });
        aprobarResponse.EnsureSuccessStatusCode();

        // Verificar el nombre de usuario generado
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var delegado = await context.Delegados.FindAsync(content.Id);
        var usuario = await context.Usuarios.FindAsync(delegado!.UsuarioId);
        
        Assert.NotNull(usuario);
        Assert.Equal("jperez", usuario.NombreUsuario);
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
            ClubId = _club!.Id,
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
        Assert.Equal(_club.Id, content.ClubId);
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
            ClubId = _club!.Id,
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
            ClubId = _club.Id,
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
        Assert.Equal(_club.Id, content.ClubId);
    }
} 