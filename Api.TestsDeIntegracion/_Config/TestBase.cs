using Api.Core.Entidades;
using Api.Core.Servicios;
using Api.Persistencia._Config;
using Microsoft.Extensions.DependencyInjection;

namespace Api.TestsDeIntegracion._Config;

public abstract class TestBase 
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    protected readonly CustomWebApplicationFactory<Program> Factory;

    protected TestBase(CustomWebApplicationFactory<Program> factory)
    {
        Factory = factory;

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        // Crear usuario de prueba para autenticación
        CreateTestUser(context);
    }
    
    private void CreateTestUser(AppDbContext context)
    {
        // Limpiar usuarios existentes para evitar conflictos
        var existingUsers = context.Usuarios.Where(u => u.NombreUsuario == "test").ToList();
        if (existingUsers.Any())
        {
            context.Usuarios.RemoveRange(existingUsers);
            context.SaveChanges();
        }
        
        // Obtener el rol de administrador (ya debe existir)
        var rolAdmin = context.Roles.First(r => r.Nombre == "Administrador");
        
        // Crear un usuario de prueba con contraseña hasheada y rol de administrador
        var usuario = new Usuario
        {
            Id = 999, // ID único para tests
            NombreUsuario = "test",
            Password = AuthCore.HashPassword("test123"), // Usar BCrypt para hashear la contraseña
            RolId = rolAdmin.Id
        };
        
        context.Usuarios.Add(usuario);
        context.SaveChanges();
    }
    
    protected async Task<HttpClient> GetAuthenticatedClient()
    {
        var client = Factory.CreateClient();
        return await AuthTestHelper.GetAuthenticatedClient(client);
    }
}