using Api.Core.DTOs;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Api.TestsUtilidades;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace Api.TestsDeIntegracion;
public class ClubIT : TestBase
{
    private Utilidades? _utilidades;

    public ClubIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        SeedData(context);
    }

    private void SeedData(AppDbContext context)
    {
        _utilidades = new Utilidades(context);
        _utilidades.DadoQueExisteElClub();
        context.SaveChanges();
    }

    [Fact]
    public async Task ListarClubes_Funciona()
    {
        var client = Factory.CreateClient();
        
        var response = await client.GetAsync($"/api/club");

        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<List<ClubDTO>>(stringResponse);
        
        response.EnsureSuccessStatusCode();
        
        Assert.Single(content!);
        Assert.Equal("un club", content!.First().Nombre);
        Assert.Equal(1, content!.First().Id);
    }
}