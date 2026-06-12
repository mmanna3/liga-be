using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.TestsDeIntegracion;

public class FaseCategoriaAnualIT : TestBase
{
    public FaseCategoriaAnualIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedData(context);
    }

    private static void SeedData(AppDbContext context)
    {
    }

    [Fact]
    public async Task ModificarCategoriasFaseAnual_SetsDistintos_400()
    {
        var (torneoId, faseAperturaId, faseClausuraId) = await CrearTorneoConAperturaYClausura(Factory);
        var client = await GetAuthenticatedClient();

        var putResponse = await client.PutAsJsonAsync($"/api/Torneo/{torneoId}/fases/{faseClausuraId}", new FaseDTO
        {
            Id = faseClausuraId,
            Nombre = "Clausura",
            Numero = 2,
            TipoDeFase = TipoDeFaseEnum.TodosContraTodos,
            EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
            EsVisibleEnApp = true,
            TorneoId = torneoId,
            Categorias =
            [
                new FaseCategoriaDTO { Nombre = "Sub-15", AnioDesde = 2010, AnioHasta = 2015, Orden = 1 },
                new FaseCategoriaDTO { Nombre = "Otra", AnioDesde = 2007, AnioHasta = 2009, Orden = 2 }
            ]
        });

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, putResponse.StatusCode);
        var mensaje = await putResponse.Content.ReadAsStringAsync();
        Assert.Contains("apertura y clausura", mensaje.ToLowerInvariant());
    }

    [Fact]
    public async Task ModificarCategoriasFaseAnual_SetsIdenticos_200()
    {
        var (torneoId, faseAperturaId, faseClausuraId) = await CrearTorneoConAperturaYClausura(Factory);
        var client = await GetAuthenticatedClient();

        var putResponse = await client.PutAsJsonAsync($"/api/Torneo/{torneoId}/fases/{faseClausuraId}", new FaseDTO
        {
            Id = faseClausuraId,
            Nombre = "Clausura",
            Numero = 2,
            TipoDeFase = TipoDeFaseEnum.TodosContraTodos,
            EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
            EsVisibleEnApp = true,
            TorneoId = torneoId,
            Categorias =
            [
                new FaseCategoriaDTO { Nombre = "Sub-15", AnioDesde = 2010, AnioHasta = 2015, Orden = 1 },
                new FaseCategoriaDTO { Nombre = "Sub-18", AnioDesde = 2007, AnioHasta = 2009, Orden = 2 }
            ]
        });

        putResponse.EnsureSuccessStatusCode();
    }

    private static async Task<(int TorneoId, int FaseAperturaId, int FaseClausuraId)> CrearTorneoConAperturaYClausura(
        CustomWebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var torneo = new Torneo
        {
            Id = 0,
            Nombre = "Torneo Anual",
            Anio = 2026,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };
        context.Torneos.Add(torneo);
        await context.SaveChangesAsync();

        var faseApertura = new FaseTodosContraTodos
        {
            Id = 0, Nombre = "Apertura", Numero = 1, TorneoId = torneo.Id,
            EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente, EsVisibleEnApp = true
        };
        var faseClausura = new FaseTodosContraTodos
        {
            Id = 0, Nombre = "Clausura", Numero = 2, TorneoId = torneo.Id,
            EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente, EsVisibleEnApp = true
        };
        context.Fases.AddRange(faseApertura, faseClausura);
        await context.SaveChangesAsync();

        torneo.FaseAperturaId = faseApertura.Id;
        torneo.FaseClausuraId = faseClausura.Id;
        await context.SaveChangesAsync();

        foreach (var faseId in new[] { faseApertura.Id, faseClausura.Id })
        {
            context.FaseCategorias.AddRange(
                new FaseCategoria
                {
                    Id = 0, Nombre = "Sub-15", AnioDesde = 2010, AnioHasta = 2015, FaseId = faseId, Orden = 1
                },
                new FaseCategoria
                {
                    Id = 0, Nombre = "Sub-18", AnioDesde = 2007, AnioHasta = 2009, FaseId = faseId, Orden = 2
                });
        }

        await context.SaveChangesAsync();
        return (torneo.Id, faseApertura.Id, faseClausura.Id);
    }
}
