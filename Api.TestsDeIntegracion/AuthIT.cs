using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Servicios;
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
        // Limpiar usuarios existentes para evitar conflictos
        var existingUsers = context.Usuarios.Where(u => u.NombreUsuario == "test").ToList();
        if (existingUsers.Any())
        {
            context.Usuarios.RemoveRange(existingUsers);
            context.SaveChanges();
        }
        
        // Crear un usuario de prueba con contraseña hasheada
        var usuario = new Usuario
        {
            Id = 999, // ID único para tests
            NombreUsuario = "test",
            Password = AuthService.HashPassword("test123") // Usar BCrypt para hashear la contraseña
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