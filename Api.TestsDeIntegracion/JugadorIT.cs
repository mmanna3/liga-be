using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Logica;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion;

public class JugadorIT : TestBase
{
    public JugadorIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        SeedData(context);
    }

    private void SeedData(AppDbContext context)
    {
        var club = new Club
        {
            Id = 1,
            Nombre = "Club de Prueba"
        };
        
        context.Clubs.Add(club);
        
        var equipo = new Equipo
        {
            Id = 1,
            Nombre = "Equipo de Prueba",
            ClubId = 1,
            Jugadores = new List<JugadorEquipo>()
        };
        
        context.Equipos.Add(equipo);
        context.SaveChanges();
    }
    
    [Fact]
    public async Task CrearJugador_DatosCorrectos_200()
    {
        // Usar un cliente autenticado
        var client = await GetAuthenticatedClient();
        
        // Generar un código alfanumérico válido
        var codigoAlfanumerico = GeneradorDeHash.GenerarAlfanumerico7Digitos(1);
        
        // Crear un jugador con todos los campos requeridos
        var jugadorDTO = new JugadorDTO
        {
            Nombre = "Juan",
            Apellido = "Perez",
            DNI = "12345678",
            FechaNacimiento = DateTime.Now.AddYears(-20),
            CodigoAlfanumerico = codigoAlfanumerico,
            // Añadir campos adicionales que puedan ser requeridos
            FotoCarnet = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==",
            FotoDNIFrente = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==",
            FotoDNIDorso = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="
        };
        
        var response = await client.PostAsJsonAsync("/api/jugador", jugadorDTO);
        
        // Si hay un error, imprimir el contenido de la respuesta para depuración
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error al crear jugador: {errorContent}");
        }
        
        response.EnsureSuccessStatusCode();
        
        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<JugadorDTO>(stringResponse);
        
        Assert.NotNull(content);
        Assert.Equal("Juan", content.Nombre);
    }
}