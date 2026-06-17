using System.Net.Http.Headers;
using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Servicios;
using Api.Persistencia._Config;
using Api.TestsUtilidades;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion._Config;

public static class AuthTestHelper
{
    public static async Task<HttpClient> GetAuthenticatedClient(HttpClient client) =>
        await GetAuthenticatedClient(client, "test", "test123");

    public static async Task<HttpClient> GetAuthenticatedClient(HttpClient client, string usuario, string password)
    {
        var loginRequest = new LoginDTO
        {
            Usuario = usuario,
            Password = password
        };
        
        var loginJson = JsonContent.Create(loginRequest);
        var response = await client.PostAsync("/api/auth/login", loginJson);
        
        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<LoginResponseDTO>(stringResponse);
        
        if (!response.IsSuccessStatusCode || content?.Token == null)
        {
            throw new Exception("No se pudo autenticar para los tests. Asegúrate de que el usuario de prueba existe.");
        }
        
        var authenticatedClient = client;
        authenticatedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content.Token);
        
        return authenticatedClient;
    }

    public static async Task<HttpClient> CrearUsuarioYAutenticar(
        HttpClient client,
        AppDbContext context,
        string nombreUsuario,
        string password,
        RolEnum rol,
        IEnumerable<UsuarioAccesoModuloDTO>? accesos = null)
    {
        var rolEntidad = await context.Roles.FirstAsync(r => r.Id == (int)rol);
        var usuario = new Usuario
        {
            Id = 0,
            NombreUsuario = nombreUsuario,
            Password = AuthCore.HashPassword(password),
            RolId = rolEntidad.Id,
            DelegadoId = null
        };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        if (accesos != null)
            PermisosDePrueba.SembrarAccesos(context, usuario.Id, accesos);
        else if (rol == RolEnum.Administrador)
            PermisosDePrueba.SembrarAccesosControlTotal(context, usuario.Id);

        return await GetAuthenticatedClient(client, nombreUsuario, password);
    }
}
