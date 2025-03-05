using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Api.TestsUtilidades;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace Api.TestsDeIntegracion;
public class EquipoIT : TestBase
{
    private Utilidades? _utilidades;
    private Club _club;

    public EquipoIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        SeedData(context);
    }

    private void SeedData(AppDbContext context)
    {
        _utilidades = new Utilidades(context);
        _club = _utilidades.DadoQueExisteElClub();
        _utilidades.DadoQueExisteElEquipo(_club);
        context.SaveChanges();
    }

    [Fact]
    public async Task ListarEquipos_Funciona()
    {
        var client = Factory.CreateClient();
        
        var response = await client.GetAsync("/api/equipo");

        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<List<EquipoDTO>>(stringResponse);
        
        response.EnsureSuccessStatusCode();
        
        Assert.Single(content!);
        Assert.Equal("un equipo", content!.First().Nombre);
        Assert.Equal(1, content!.First().Id);
    }
    
    [Fact]
    public async Task CrearEquipo_DatosCorrectos_200()
    {
        var client = Factory.CreateClient();

        var equipo = new EquipoDTO
        {
            Nombre = "Equipo de prueba",
            ClubId = 1
        };
        
        var equipoJson = JsonContent.Create(equipo);
        var response = await client.PostAsync("/api/equipo", equipoJson);

        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<EquipoDTO>(stringResponse);
        
        response.EnsureSuccessStatusCode();
        
        Assert.Equal("Equipo de prueba", content!.Nombre);
        Assert.IsType<int>(content.Id);
        Assert.True(content.Id > 0);
    }
}