using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Api.TestsUtilidades;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion;

public class DelegadoIT : TestBase
{
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
            Nombre = "Juan",
            Apellido = "Pérez",
            ClubId = _club!.Id
        };
        
        var response = await client.PostAsJsonAsync("/api/delegado", delegadoDTO);
        
        response.EnsureSuccessStatusCode();
        
        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<DelegadoDTO>(stringResponse);
        
        Assert.NotNull(content);
        Assert.Equal("Juan", content.Nombre);
        Assert.Equal("Pérez", content.Apellido);
        Assert.Equal(_club.Id, content.ClubId);

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
            Nombre = "Ana",
            Apellido = "García",
            ClubId = _club!.Id
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
            Nombre = "Carlos",
            Apellido = "López",
            ClubId = _club!.Id
        };
        
        var createResponse = await client.PostAsJsonAsync("/api/delegado", delegadoDTO);
        createResponse.EnsureSuccessStatusCode();
        var createContent = JsonConvert.DeserializeObject<DelegadoDTO>(await createResponse.Content.ReadAsStringAsync());
        
        // Modificamos el delegado
        var delegadoModificadoDTO = new DelegadoDTO
        {
            Id = createContent!.Id,
            Nombre = "CarlosModif",
            Apellido = "LópezModif",
            ClubId = _club.Id
        };
        
        var response = await client.PutAsJsonAsync($"/api/delegado/{createContent.Id}", delegadoModificadoDTO);
        response.EnsureSuccessStatusCode();
        
        // Verificamos que se haya modificado correctamente
        var getResponse = await client.GetAsync($"/api/delegado/{createContent.Id}");
        getResponse.EnsureSuccessStatusCode();
        
        var stringResponse = await getResponse.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<DelegadoDTO>(stringResponse);
        
        Assert.NotNull(content);
        Assert.Equal("Carlos Modificado", content.Nombre);
        Assert.Equal("López Modificado", content.Apellido);
        Assert.Equal(_club.Id, content.ClubId);
    }
} 