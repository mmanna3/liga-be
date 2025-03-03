using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Api.TestsUtilidades;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion;
public class JugadorIT : TestBase
{
    private Utilidades? _utilidades;
    private const string PuntoRojoBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";

    public JugadorIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        SeedData(context);
    }

    private void SeedData(AppDbContext context)
    {
        _utilidades = new Utilidades(context);
        var club = _utilidades.DadoQueExisteElClub();
        _utilidades.DadoQueExisteElEquipo(club);
        context.SaveChanges();
    }

    [Fact]
    public async Task CrearJugador_DatosCorrectos_200()
    {
        var client = Factory.CreateClient();

        var jugador = new JugadorDTO
        {
            DNI = "123456789",
            Nombre = "Diego",
            Apellido = "Maradona",
            FechaNacimiento = default,
            EquipoInicialId = 1,
            FotoCarnet = PuntoRojoBase64,
            FotoDNIFrente = PuntoRojoBase64,
            FotoDNIDorso = PuntoRojoBase64
        };
        
        var jugadorJson = JsonContent.Create(jugador);
        var response = await client.PostAsync("/api/jugador", jugadorJson);

        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<JugadorDTO>(stringResponse);
        
        response.EnsureSuccessStatusCode();
        
        Assert.Equal("Diego", content!.Nombre);
        Assert.Equal(1, content.Id);
    }
}