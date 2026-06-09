using System.Net;
using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Servicios;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion;

public class UsuarioIT : TestBase
{
    public UsuarioIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Listar_ComoAdmin_200()
    {
        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync("/api/usuario");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Listar_ComoUsuario_403()
    {
        var client = await GetAuthenticatedClientConRol("Usuario");
        var response = await client.GetAsync("/api/usuario");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CrearUsuario_LoginFallaHastaCambiarPassword_200()
    {
        var client = await GetAuthenticatedClient();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var rolConsulta = context.Roles.First(r => r.Nombre == "Consulta");

        var dto = new UsuarioAdminDTO
        {
            NombreUsuario = "nuevo.usuario",
            RolId = rolConsulta.Id
        };

        var createResponse = await client.PostAsJsonAsync("/api/usuario", dto);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        var created = JsonConvert.DeserializeObject<UsuarioAdminDTO>(
            await createResponse.Content.ReadAsStringAsync());
        Assert.NotNull(created);
        Assert.True(created!.BlanqueoPendiente);

        var loginClient = Factory.CreateClient();
        var loginFail = await loginClient.PostAsJsonAsync("/api/auth/login", new LoginDTO
        {
            Usuario = "nuevo.usuario",
            Password = "cualquier"
        });
        var loginFailContent = JsonConvert.DeserializeObject<LoginResponseDTO>(
            await loginFail.Content.ReadAsStringAsync());
        Assert.False(loginFailContent!.Exito);
        Assert.Equal("El usuario debe cambiar la contraseña", loginFailContent.Error);

        var cambiarResponse = await loginClient.PostAsJsonAsync("/api/auth/cambiar-password", new CambiarPasswordDTO
        {
            Usuario = "nuevo.usuario",
            PasswordNuevo = "clave123"
        });
        var cambiarContent = JsonConvert.DeserializeObject<LoginResponseDTO>(
            await cambiarResponse.Content.ReadAsStringAsync());
        Assert.True(cambiarContent!.Exito);

        var loginOk = await loginClient.PostAsJsonAsync("/api/auth/login", new LoginDTO
        {
            Usuario = "nuevo.usuario",
            Password = "clave123"
        });
        Assert.Equal(HttpStatusCode.OK, loginOk.StatusCode);
    }

    [Fact]
    public async Task CrearUsuario_ConRolSuperAdministrador_400()
    {
        var client = await GetAuthenticatedClient();
        var dto = new UsuarioAdminDTO
        {
            NombreUsuario = "super.test",
            RolId = 0
        };

        var response = await client.PostAsJsonAsync("/api/usuario", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task BlanquearClave_DeOtroUsuario_200()
    {
        var client = await GetAuthenticatedClient();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var rolUsuario = context.Roles.First(r => r.Nombre == "Usuario");

        var usuario = new Usuario
        {
            Id = 0,
            NombreUsuario = "para.blanquear",
            Password = AuthCore.HashPassword("clave123"),
            RolId = rolUsuario.Id
        };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var response = await client.PostAsync($"/api/usuario/blanquear-clave?id={usuario.Id}", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var verifyScope = Factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var actualizado = await verifyContext.Usuarios.FindAsync(usuario.Id);
        Assert.Null(actualizado!.Password);
    }

    [Fact]
    public async Task BlanquearClave_PropioUsuario_400()
    {
        var client = await GetAuthenticatedClient();
        var response = await client.PostAsync("/api/usuario/blanquear-clave?id=999", null);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Eliminar_PropioUsuario_400()
    {
        var client = await GetAuthenticatedClient();
        var response = await client.DeleteAsync("/api/usuario/999");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RolesAsignables_ComoAdmin_NoIncluyeSuperAdministrador()
    {
        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync("/api/usuario/roles-asignables");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var roles = JsonConvert.DeserializeObject<List<RolDTO>>(
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(roles);
        Assert.DoesNotContain(roles!, r => r.Nombre == "SuperAdministrador");
        Assert.Contains(roles!, r => r.Nombre == "Administrador");
    }

    private async Task<HttpClient> GetAuthenticatedClientConRol(string rolNombre)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var rol = context.Roles.First(r => r.Nombre == rolNombre);

        var usuario = new Usuario
        {
            Id = 997,
            NombreUsuario = "usuario.rol",
            Password = AuthCore.HashPassword("test123"),
            RolId = rol.Id
        };

        var existente = context.Usuarios.FirstOrDefault(u => u.NombreUsuario == "usuario.rol");
        if (existente != null)
            context.Usuarios.Remove(existente);

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var client = Factory.CreateClient();
        return await AuthTestHelper.GetAuthenticatedClient(client, "usuario.rol", "test123");
    }
}
