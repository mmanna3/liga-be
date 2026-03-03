using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Logica;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace Api.TestsDeIntegracion;
public class ClubIT : TestBase
{
    private Club? _club;

    public ClubIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        SeedData(context);
    }

    private void SeedData(AppDbContext context)
    {
        _club = new Club
        {
            Id = 0,
            Nombre = "Club de Prueba"
        };
        
        context.Clubs.Add(_club);
        context.SaveChanges();
    }

    [Fact]
    public async Task ListarClubes_Funciona()
    {
        var client = await GetAuthenticatedClient();
        
        var response = await client.GetAsync("/api/club");
        
        response.EnsureSuccessStatusCode();
        
        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<List<ClubDTO>>(stringResponse);
        
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task ObtenerClub_PorId_DevuelveEscudoEnBase64()
    {
        const string fotoBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";
        var client = await GetAuthenticatedClient();
        await client.PostAsJsonAsync($"/api/club/{_club!.Id}/cambiar-escudo", new CambiarEscudoDTO { ImagenBase64 = fotoBase64 });

        var response = await client.GetAsync($"/api/club/{_club.Id}");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<ClubDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.NotNull(content.Escudo);
        Assert.StartsWith("data:image/", content.Escudo);
        Assert.Contains("base64,", content.Escudo);
    }

    [Fact]
    public async Task CambiarEscudo_ConImagenValida_GuardaEscudo()
    {
        const string fotoBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";
        var client = await GetAuthenticatedClient();

        var response = await client.PostAsJsonAsync($"/api/club/{_club!.Id}/cambiar-escudo", new CambiarEscudoDTO { ImagenBase64 = fotoBase64 });
        response.EnsureSuccessStatusCode();

        var getResponse = await client.GetAsync($"/api/club/{_club.Id}");
        getResponse.EnsureSuccessStatusCode();
        var club = JsonConvert.DeserializeObject<ClubDTO>(await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(club);
        Assert.StartsWith("data:image/", club.Escudo);
        Assert.Contains("base64,", club.Escudo);
    }

    [Fact]
    public async Task EliminarClub_EliminaClubYEscudo()
    {
        const string fotoBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";
        var client = await GetAuthenticatedClient();

        var clubParaEliminar = new Club { Id = 0, Nombre = "Club a Eliminar" };
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Clubs.Add(clubParaEliminar);
            context.SaveChanges();
        }

        await client.PostAsJsonAsync($"/api/club/{clubParaEliminar.Id}/cambiar-escudo", new CambiarEscudoDTO { ImagenBase64 = fotoBase64 });

        var deleteResponse = await client.DeleteAsync($"/api/club/{clubParaEliminar.Id}");
        deleteResponse.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var clubEliminado = context.Clubs.Find(clubParaEliminar.Id);
            Assert.Null(clubEliminado);

            var paths = scope.ServiceProvider.GetRequiredService<AppPaths>();
            var pathEscudo = Path.Combine(paths.ImagenesEscudosAbsolute, $"{clubParaEliminar.Id}.jpg");
            Assert.False(File.Exists(pathEscudo));
        }
    }
}