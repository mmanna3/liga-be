using System.Net;
using Api.Core.DTOs;
using Api.Core.Enums;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Api.TestsUtilidades;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion;

public class PermisosIT : TestBase
{
    public PermisosIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetTorneos_SinPermisos_Devuelve403()
    {
        var client = Factory.CreateClient();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var autenticado = await AuthTestHelper.CrearUsuarioYAutenticar(
            client, context, "sinperm", "clave123", RolEnum.Usuario, accesos: []);

        var response = await autenticado.GetAsync("/api/torneo");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetTorneos_ConEdicionEnTorneos_Devuelve200()
    {
        var client = Factory.CreateClient();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var autenticado = await AuthTestHelper.CrearUsuarioYAutenticar(
            client,
            context,
            "edittor",
            "clave123",
            RolEnum.Usuario,
            accesos: PermisosDePrueba.AccesoSolo(ModuloSistema.Torneos, NivelAcceso.Edicion));

        var response = await autenticado.GetAsync("/api/torneo");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteClub_ConEdicionEnClubes_Devuelve403()
    {
        var client = Factory.CreateClient();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var autenticado = await AuthTestHelper.CrearUsuarioYAutenticar(
            client,
            context,
            "editclub",
            "clave123",
            RolEnum.Usuario,
            accesos: PermisosDePrueba.AccesoSolo(ModuloSistema.Clubes, NivelAcceso.Edicion));

        var response = await autenticado.DeleteAsync("/api/club/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetClub_ConEdicionEnTorneos_Devuelve403()
    {
        var client = Factory.CreateClient();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var autenticado = await AuthTestHelper.CrearUsuarioYAutenticar(
            client,
            context,
            "solo-tor",
            "clave123",
            RolEnum.Usuario,
            accesos: PermisosDePrueba.AccesoSolo(ModuloSistema.Torneos, NivelAcceso.Edicion));

        var response = await autenticado.GetAsync("/api/club");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Login_IncluyePermisosEnResponse()
    {
        var client = Factory.CreateClient();
        var loginJson = new StringContent(
            JsonConvert.SerializeObject(new LoginDTO { Usuario = "test", Password = "test123" }),
            System.Text.Encoding.UTF8,
            "application/json");
        var response = await client.PostAsync("/api/auth/login", loginJson);
        var body = JsonConvert.DeserializeObject<LoginResponseDTO>(await response.Content.ReadAsStringAsync());

        Assert.True(body!.Exito);
        Assert.Equal(8, body.Permisos.Count);
    }

    [Fact]
    public async Task GetUsuarios_SinConfiguracion_Devuelve403()
    {
        var client = Factory.CreateClient();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var autenticado = await AuthTestHelper.CrearUsuarioYAutenticar(
            client,
            context,
            "no-config",
            "clave123",
            RolEnum.Usuario,
            accesos: PermisosDePrueba.AccesoSolo(ModuloSistema.Torneos, NivelAcceso.ControlTotal));

        var response = await autenticado.GetAsync("/api/usuario");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
