using System.Net.Http.Headers;
using System.Net.Http.Json;
using Api.Core.DTOs;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion._Config;

public static class AuthTestHelper
{
    public static async Task<HttpClient> GetAuthenticatedClient(HttpClient client)
    {
        var loginRequest = new LoginDTO
        {
            Usuario = "test",
            Password = "test123"
        };
        
        var loginJson = JsonContent.Create(loginRequest);
        var response = await client.PostAsync("/api/auth/login", loginJson);
        
        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<LoginResponseDTO>(stringResponse);
        
        if (!response.IsSuccessStatusCode || content?.Token == null)
        {
            throw new Exception("No se pudo autenticar para los tests. Asegúrate de que el usuario de prueba existe.");
        }
        
        // Crear un nuevo cliente con el token de autenticación
        var authenticatedClient = client;
        authenticatedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content.Token);
        
        return authenticatedClient;
    }
} 