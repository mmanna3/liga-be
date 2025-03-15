using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion;

public class AuthIT : TestBase
{
    public AuthIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        SeedData(context);
    }

    private void SeedData(AppDbContext context)
    {
        // Crear un usuario de prueba
        var usuario = new Usuario
        {
            Id = 0,
            NombreUsuario = "test",
            Password = "test123",
            Rol = "Usuario"
        };
        
        context.Usuarios.Add(usuario);
        context.SaveChanges();
    }

    [Fact]
    public async Task Login_DatosCorrectos_200()
    {
        var client = Factory.CreateClient();

        var loginRequest = new LoginDTO
        {
            Usuario = "test",
            Password = "test123"
        };
        
        var loginJson = JsonContent.Create(loginRequest);
        var response = await client.PostAsync("/api/auth/login", loginJson);

        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<LoginResponseDTO>(stringResponse);
        
        response.EnsureSuccessStatusCode();
        
        Assert.True(content!.Exito);
        Assert.NotNull(content.Token);
        Assert.Null(content.Error);
    }
    
    [Fact]
    public async Task Login_DatosIncorrectos_400()
    {
        var client = Factory.CreateClient();

        var loginRequest = new LoginDTO
        {
            Usuario = "test",
            Password = "contraseña_incorrecta"
        };
        
        var loginJson = JsonContent.Create(loginRequest);
        var response = await client.PostAsync("/api/auth/login", loginJson);

        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<LoginResponseDTO>(stringResponse);
        
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(content!.Exito);
        Assert.Null(content.Token);
        Assert.NotNull(content.Error);
    }
} 