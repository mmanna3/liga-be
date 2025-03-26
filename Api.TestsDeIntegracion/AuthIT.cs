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
        var existingUsers = context.Usuarios.Where(u => u.NombreUsuario == "test" || u.NombreUsuario == "noAdmin").ToList();
        if (existingUsers.Any())
        {
            context.Usuarios.RemoveRange(existingUsers);
            context.SaveChanges();
        }
        
        // Obtener el rol de administrador (ya debe existir)
        var rolAdmin = context.Roles.First(r => r.Nombre == "Administrador");
        
        // Crear un usuario de prueba con contraseña hasheada
        var usuario = new Usuario
        {
            Id = 999, // ID único para tests
            NombreUsuario = "test",
            Password = AuthCore.HashPassword("test123"), // Usar BCrypt para hashear la contraseña
            RolId = rolAdmin.Id // Rol Administrador
        };
        
        context.Usuarios.Add(usuario);
        context.SaveChanges();
    }

    [Fact]
    public async Task Login_DatosCorrectos_200()
    {
        // Arrange
        var client = Factory.CreateClient();
        var loginRequest = new LoginDTO
        {
            Usuario = "test",
            Password = "test123"
        };
        
        // Act
        var response = await client.PostAsync("/api/auth/login", JsonContent.Create(loginRequest));
        
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        
        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<LoginResponseDTO>(stringResponse);
        
        Assert.NotNull(content);
        Assert.True(content.Exito);
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
    
    [Fact]
    public async Task Acceso_SinAutorizacion_401()
    {
        // Arrange
        var client = Factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/club"); // Intentar acceder a un endpoint protegido sin token
        
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task Acceso_ConAutorizacionCorrecta_200()
    {
        // Arrange
        var client = await GetAuthenticatedClient(); // Este método ya existe y configura el token
        
        // Act
        var response = await client.GetAsync("/api/club");
        
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task Acceso_ConRolIncorrecto_403()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Verificar que los roles existen
        var roles = context.Roles.ToList();
        Assert.Contains(roles, r => r.Nombre == "Usuario");
        
        // Obtener el rol de usuario (no administrador)
        var rolUsuario = context.Roles.First(r => r.Nombre == "Usuario");
        
        // Crear un usuario sin rol de administrador
        var usuarioNoAdmin = new Usuario
        {
            Id = 998,
            NombreUsuario = "noAdmin",
            Password = AuthCore.HashPassword("test123"),
            RolId = rolUsuario.Id
        };
        
        context.Usuarios.Add(usuarioNoAdmin);
        await context.SaveChangesAsync();
        
        var client = Factory.CreateClient();
        var loginRequest = new LoginDTO
        {
            Usuario = "noAdmin",
            Password = "test123"
        };
        
        var loginResponse = await client.PostAsync("/api/auth/login", JsonContent.Create(loginRequest));
        var loginContent = JsonConvert.DeserializeObject<LoginResponseDTO>(
            await loginResponse.Content.ReadAsStringAsync());
        
        Assert.NotNull(loginContent);
        Assert.True(loginContent.Exito);
        Assert.NotNull(loginContent.Token);
        
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginContent.Token);
        
        // Act
        var response = await client.GetAsync("/api/club");
        
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CambiarPassword_UsuarioNoEncontrado_400()
    {
        // Arrange
        var client = Factory.CreateClient();
        var request = new CambiarPasswordDTO
        {
            Usuario = "usuario_no_existente",
            PasswordActual = "test123",
            PasswordNuevo = "nueva123"
        };
        
        // Act
        var response = await client.PostAsync("/api/auth/cambiar-password", JsonContent.Create(request));
        
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = JsonConvert.DeserializeObject<LoginResponseDTO>(
            await response.Content.ReadAsStringAsync());
        
        Assert.NotNull(content);
        Assert.False(content.Exito);
        Assert.Equal("Usuario no encontrado", content.Error);
    }

    [Fact]
    public async Task CambiarPassword_ContraseñaActualIncorrecta_400()
    {
        // Arrange
        var client = Factory.CreateClient();
        var request = new CambiarPasswordDTO
        {
            Usuario = "test",
            PasswordActual = "contraseña_incorrecta",
            PasswordNuevo = "nueva123"
        };
        
        // Act
        var response = await client.PostAsync("/api/auth/cambiar-password", JsonContent.Create(request));
        
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = JsonConvert.DeserializeObject<LoginResponseDTO>(
            await response.Content.ReadAsStringAsync());
        
        Assert.NotNull(content);
        Assert.False(content.Exito);
        Assert.Equal("La contraseña actual es incorrecta", content.Error);
    }

    [Fact]
    public async Task CambiarPassword_DatosCorrectos_200()
    {
        // Arrange
        var client = Factory.CreateClient();
        var request = new CambiarPasswordDTO
        {
            Usuario = "test",
            PasswordActual = "test123",
            PasswordNuevo = "nueva123"
        };
        
        // Act
        var response = await client.PostAsync("/api/auth/cambiar-password", JsonContent.Create(request));
        
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        
        var content = JsonConvert.DeserializeObject<LoginResponseDTO>(
            await response.Content.ReadAsStringAsync());
        
        Assert.NotNull(content);
        Assert.True(content.Exito);
        Assert.NotNull(content.Token);
        
        // Verificar que se puede hacer login con la nueva contraseña
        var loginRequest = new LoginDTO
        {
            Usuario = "test",
            Password = "nueva123"
        };
        
        var loginResponse = await client.PostAsync("/api/auth/login", JsonContent.Create(loginRequest));
        Assert.Equal(System.Net.HttpStatusCode.OK, loginResponse.StatusCode);
    }
} 