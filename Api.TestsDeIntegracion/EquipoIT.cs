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
    private Club? _club;

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
        var client = await GetAuthenticatedClient();
        
        var response = await client.GetAsync("/api/equipo");
        
        response.EnsureSuccessStatusCode();
        
        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<List<EquipoDTO>>(stringResponse);
        
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }
    
    [Fact]
    public async Task CrearEquipo_DatosCorrectos_200()
    {
        var client = await GetAuthenticatedClient();
        
        var equipoDTO = new EquipoDTO
        {
            Nombre = "Nuevo Equipo",
            ClubId = 1
        };
        
        var response = await client.PostAsJsonAsync("/api/equipo", equipoDTO);
        
        response.EnsureSuccessStatusCode();
        
        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<EquipoDTO>(stringResponse);
        
        Assert.NotNull(content);
        Assert.Equal("Nuevo Equipo", content.Nombre);
    }
}