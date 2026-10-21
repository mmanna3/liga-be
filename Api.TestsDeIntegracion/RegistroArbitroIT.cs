using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion;

public class RegistroArbitroIT : TestBase
{
    public RegistroArbitroIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedData(context);
    }

    private static void SeedData(AppDbContext context)
    {
        // El seed "General" (Id=1, no visible en la app) ya existe por HasData en AppDbContext
        context.TorneoAgrupadores.AddRange(
            new TorneoAgrupador { Id = 0, Nombre = "Futsal", EsVisibleEnApp = true, ColorId = (int)ColorEnum.Azul },
            new TorneoAgrupador { Id = 0, Nombre = "Baby", EsVisibleEnApp = true, ColorId = (int)ColorEnum.Rojo },
            new TorneoAgrupador { Id = 0, Nombre = "Oculto", EsVisibleEnApp = false, ColorId = (int)ColorEnum.Verde });
        context.SaveChanges();
    }

    [Fact]
    public async Task TorneoAgrupadoresParaArbitros_SinLogin_DevuelveSoloVisiblesOrdenadosPorNombre()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/publico/torneo-agrupadores-para-arbitros");

        response.EnsureSuccessStatusCode();
        var content = JsonConvert.DeserializeObject<List<TorneoAgrupadorParaArbitroDTO>>(
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(new[] { "Baby", "Futsal" }, content.Select(a => a.Nombre));
        Assert.All(content, a => Assert.True(a.Id > 0));
    }
}
