using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.TestsDeIntegracion;

public class EstructuraFasesIT : TestBase
{
    public EstructuraFasesIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    /// <summary>
    /// Caso canónico: Fase A, Grupo A (B,C, Grupo B(D,E)), Fase F
    /// </summary>
    private static async Task<(int TorneoId, Dictionary<string, int> Ids)> SeedEstructuraEjemplo(
        CustomWebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var torneo = new Torneo
        {
            Id = 0,
            Nombre = "Torneo Estructura",
            Anio = 2026,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };
        ctx.Torneos.Add(torneo);
        await ctx.SaveChangesAsync();

        var faseA = new FaseTodosContraTodos { Id = 0, Nombre = "Fase A", Numero = 1, TorneoId = torneo.Id, EstadoFaseId = 100, EsVisibleEnApp = true };
        var faseF = new FaseTodosContraTodos { Id = 0, Nombre = "Fase F", Numero = 3, TorneoId = torneo.Id, EstadoFaseId = 100, EsVisibleEnApp = true };
        ctx.Fases.AddRange(faseA, faseF);
        await ctx.SaveChangesAsync();

        var grupoA = new GrupoDeFases { Id = 0, Nombre = "Grupo A", Numero = 2, TorneoId = torneo.Id };
        ctx.Set<GrupoDeFases>().Add(grupoA);
        await ctx.SaveChangesAsync();

        var faseB = new FaseTodosContraTodos { Id = 0, Nombre = "Fase B", Numero = 1, TorneoId = torneo.Id, GrupoDeFasesId = grupoA.Id, EstadoFaseId = 100, EsVisibleEnApp = true };
        var faseC = new FaseTodosContraTodos { Id = 0, Nombre = "Fase C", Numero = 2, TorneoId = torneo.Id, GrupoDeFasesId = grupoA.Id, EstadoFaseId = 100, EsVisibleEnApp = true };
        ctx.Fases.AddRange(faseB, faseC);
        await ctx.SaveChangesAsync();

        var grupoB = new GrupoDeFases { Id = 0, Nombre = "Grupo B", Numero = 3, TorneoId = torneo.Id, GrupoDeFasesPadreId = grupoA.Id };
        ctx.Set<GrupoDeFases>().Add(grupoB);
        await ctx.SaveChangesAsync();

        var faseD = new FaseTodosContraTodos { Id = 0, Nombre = "Fase D", Numero = 1, TorneoId = torneo.Id, GrupoDeFasesId = grupoB.Id, EstadoFaseId = 100, EsVisibleEnApp = true };
        var faseE = new FaseTodosContraTodos { Id = 0, Nombre = "Fase E", Numero = 2, TorneoId = torneo.Id, GrupoDeFasesId = grupoB.Id, EstadoFaseId = 100, EsVisibleEnApp = true };
        ctx.Fases.AddRange(faseD, faseE);
        await ctx.SaveChangesAsync();

        return (torneo.Id, new Dictionary<string, int>
        {
            ["faseA"] = faseA.Id,
            ["grupoA"] = grupoA.Id,
            ["faseB"] = faseB.Id,
            ["faseC"] = faseC.Id,
            ["grupoB"] = grupoB.Id,
            ["faseD"] = faseD.Id,
            ["faseE"] = faseE.Id,
            ["faseF"] = faseF.Id
        });
    }

    [Fact]
    public async Task PersistirEstructura_EjemploUsuario_MantieneJerarquia()
    {
        var (torneoId, ids) = await SeedEstructuraEjemplo(Factory);
        var client = await GetAuthenticatedClient();

        var dto = new EstructuraFasesDTO
        {
            Items =
            [
                new EstructuraFasesItemDTO { Tipo = EstructuraFasesTreeBuilder.TipoFase, FaseId = ids["faseA"] },
                new EstructuraFasesItemDTO
                {
                    Tipo = EstructuraFasesTreeBuilder.TipoGrupo,
                    GrupoId = ids["grupoA"],
                    Items =
                    [
                        new EstructuraFasesItemDTO { Tipo = EstructuraFasesTreeBuilder.TipoFase, FaseId = ids["faseC"] },
                        new EstructuraFasesItemDTO { Tipo = EstructuraFasesTreeBuilder.TipoFase, FaseId = ids["faseB"] },
                        new EstructuraFasesItemDTO
                        {
                            Tipo = EstructuraFasesTreeBuilder.TipoGrupo,
                            GrupoId = ids["grupoB"],
                            Items =
                            [
                                new EstructuraFasesItemDTO { Tipo = EstructuraFasesTreeBuilder.TipoFase, FaseId = ids["faseE"] },
                                new EstructuraFasesItemDTO { Tipo = EstructuraFasesTreeBuilder.TipoFase, FaseId = ids["faseD"] }
                            ]
                        }
                    ]
                },
                new EstructuraFasesItemDTO { Tipo = EstructuraFasesTreeBuilder.TipoFase, FaseId = ids["faseF"] }
            ]
        };

        var response = await client.PutAsJsonAsync($"/api/Torneo/{torneoId}/estructura-fases", dto);
        response.EnsureSuccessStatusCode();

        await using var scope = Factory.Services.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var faseB = await ctx.Fases.FindAsync(ids["faseB"]);
        Assert.NotNull(faseB);
        Assert.Equal(2, faseB.Numero);
        Assert.Equal(ids["grupoA"], faseB.GrupoDeFasesId);

        var faseC = await ctx.Fases.FindAsync(ids["faseC"]);
        Assert.NotNull(faseC);
        Assert.Equal(1, faseC.Numero);

        var faseD = await ctx.Fases.FindAsync(ids["faseD"]);
        Assert.NotNull(faseD);
        Assert.Equal(2, faseD.Numero);
        Assert.Equal(ids["grupoB"], faseD.GrupoDeFasesId);
    }

    [Fact]
    public async Task RechazarEstructura_TresNivelesDeGrupos_400()
    {
        var torneoId = await CrearTorneoVacio();
        int g1, g2, g3;
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var grupo1 = new GrupoDeFases { Id = 0, Nombre = "G1", Numero = 1, TorneoId = torneoId };
            ctx.Set<GrupoDeFases>().Add(grupo1);
            await ctx.SaveChangesAsync();
            g1 = grupo1.Id;

            var grupo2 = new GrupoDeFases { Id = 0, Nombre = "G2", Numero = 1, TorneoId = torneoId, GrupoDeFasesPadreId = g1 };
            ctx.Set<GrupoDeFases>().Add(grupo2);
            await ctx.SaveChangesAsync();
            g2 = grupo2.Id;

            var grupo3 = new GrupoDeFases { Id = 0, Nombre = "G3", Numero = 1, TorneoId = torneoId, GrupoDeFasesPadreId = g2 };
            ctx.Set<GrupoDeFases>().Add(grupo3);
            await ctx.SaveChangesAsync();
            g3 = grupo3.Id;
        }

        var client = await GetAuthenticatedClient();
        var dto = new EstructuraFasesDTO
        {
            Items =
            [
                new EstructuraFasesItemDTO
                {
                    Tipo = EstructuraFasesTreeBuilder.TipoGrupo,
                    GrupoId = g1,
                    Items =
                    [
                        new EstructuraFasesItemDTO
                        {
                            Tipo = EstructuraFasesTreeBuilder.TipoGrupo,
                            GrupoId = g2,
                            Items =
                            [
                                new EstructuraFasesItemDTO
                                {
                                    Tipo = EstructuraFasesTreeBuilder.TipoGrupo,
                                    GrupoId = g3,
                                    Items = []
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        var response = await client.PutAsJsonAsync($"/api/Torneo/{torneoId}/estructura-fases", dto);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<int> CrearTorneoVacio()
    {
        using var scope = Factory.Services.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var torneo = new Torneo
        {
            Id = 0,
            Nombre = "Vacío",
            Anio = 2026,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };
        ctx.Torneos.Add(torneo);
        await ctx.SaveChangesAsync();
        return torneo.Id;
    }
}
