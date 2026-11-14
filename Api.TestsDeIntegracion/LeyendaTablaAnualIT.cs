using System.Net;
using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.DTOs.AppCarnetDigital;
using Api.Core.Entidades;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Api.TestsUtilidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.TestsDeIntegracion;

/// <summary>
/// Leyendas propias de la tabla Anual (<see cref="LeyendaTablaPosiciones.EsDeTablaAnual"/>): se guardan en la zona de
/// Apertura, solo aparecen en la tabla Anual y la tabla Anual ya no muestra las leyendas de Apertura ni de Clausura.
/// </summary>
public class LeyendaTablaAnualIT : TestBase
{
    public LeyendaTablaAnualIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private sealed record Escenario(
        int TorneoId,
        int FaseAperturaId,
        int FaseClausuraId,
        int ZonaAperturaId,
        int ZonaClausuraId,
        int CatAperturaId,
        int CatClausuraId,
        int EquipoId);

    /// <summary>Torneo con Apertura y Clausura (una zona homónima y una categoría homónima en cada una).</summary>
    private async Task<Escenario> SeedAperturaYClausura()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var torneo = new Torneo
        {
            Id = 0,
            Nombre = "Torneo Anual Leyendas IT",
            Anio = 2026,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };
        context.Torneos.Add(torneo);
        await context.SaveChangesAsync();

        var faseA = new FaseTodosContraTodos
        {
            Id = 0, Nombre = "Apertura", Numero = 1, TorneoId = torneo.Id, EstadoFaseId = 100, EsVisibleEnApp = true
        };
        var faseC = new FaseTodosContraTodos
        {
            Id = 0, Nombre = "Clausura", Numero = 2, TorneoId = torneo.Id, EstadoFaseId = 100, EsVisibleEnApp = true
        };
        context.Fases.AddRange(faseA, faseC);
        await context.SaveChangesAsync();

        var catA = await CategoriasDePrueba.AgregarFaseCategoria(context, faseA.Id, "Primera", 1);
        var catC = await CategoriasDePrueba.AgregarFaseCategoria(context, faseC.Id, "Primera", 1);

        var zonaA = new ZonaTodosContraTodos { Id = 0, Nombre = "Zona A", FaseId = faseA.Id, Orden = 1 };
        var zonaC = new ZonaTodosContraTodos { Id = 0, Nombre = "Zona A", FaseId = faseC.Id, Orden = 1 };
        context.Zonas.AddRange(zonaA, zonaC);

        var club = new Club { Id = 0, Nombre = "Club Anual IT" };
        context.Clubs.Add(club);
        await context.SaveChangesAsync();

        var equipo = new Equipo
        {
            Id = 0, Nombre = "Equipo Anual", ClubId = club.Id, Club = club, Jugadores = new List<JugadorEquipo>()
        };
        context.Equipos.Add(equipo);
        await context.SaveChangesAsync();
        context.Set<EquipoZona>().AddRange(
            new EquipoZona { Id = 0, EquipoId = equipo.Id, ZonaId = zonaA.Id },
            new EquipoZona { Id = 0, EquipoId = equipo.Id, ZonaId = zonaC.Id });

        torneo.FaseAperturaId = faseA.Id;
        torneo.FaseClausuraId = faseC.Id;
        await context.SaveChangesAsync();

        return new Escenario(torneo.Id, faseA.Id, faseC.Id, zonaA.Id, zonaC.Id, catA.Id, catC.Id, equipo.Id);
    }

    private static LeyendaTablaPosicionesDTO Leyenda(int zonaId, int? categoriaId, string texto, bool anual) => new()
    {
        Id = 0,
        Leyenda = texto,
        CategoriaId = categoriaId,
        ZonaId = zonaId,
        EquipoId = null,
        QuitaDePuntos = 0,
        EsDeTablaAnual = anual
    };

    private async Task<PosicionesDTO> GetPosiciones(string endpoint, int zonaId)
    {
        var response = await Factory.CreateClient().GetAsync($"/api/carnet-digital/{endpoint}?zonaId={zonaId}");
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<PosicionesDTO>();
        Assert.NotNull(dto);
        return dto;
    }

    [Fact]
    public async Task TablaAnual_MuestraSoloSusLeyendas_YAperturaYClausuraNoLasMuestran()
    {
        var e = await SeedAperturaYClausura();
        var client = await GetAuthenticatedClient();

        async Task Post(int zonaId, LeyendaTablaPosicionesDTO dto)
        {
            var r = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", dto);
            Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        }

        await Post(e.ZonaAperturaId, Leyenda(e.ZonaAperturaId, e.CatAperturaId, "Cat apertura", false));
        await Post(e.ZonaAperturaId, Leyenda(e.ZonaAperturaId, null, "General apertura", false));
        await Post(e.ZonaClausuraId, Leyenda(e.ZonaClausuraId, e.CatClausuraId, "Cat clausura", false));
        await Post(e.ZonaClausuraId, Leyenda(e.ZonaClausuraId, null, "General clausura", false));
        // Misma zona y categoría que las de Apertura: no chocan con la unicidad porque son de la tabla Anual.
        await Post(e.ZonaAperturaId, Leyenda(e.ZonaAperturaId, e.CatAperturaId, "Cat anual", true));
        await Post(e.ZonaAperturaId, Leyenda(e.ZonaAperturaId, null, "General anual", true));

        // Se puede pedir la anual desde la zona de Apertura o desde la de Clausura.
        foreach (var zonaRef in new[] { e.ZonaAperturaId, e.ZonaClausuraId })
        {
            var anual = (await GetPosiciones("posiciones-anual", zonaRef)).Posiciones.ToList();
            Assert.Equal("Cat anual", anual[0].Leyenda);
            Assert.Equal("General", anual[1].Categoria);
            Assert.Equal("General anual", anual[1].Leyenda);
        }

        var apertura = (await GetPosiciones("posiciones-todos-contra-todos", e.ZonaAperturaId)).Posiciones.ToList();
        Assert.Equal("Cat apertura", apertura[0].Leyenda);
        Assert.Equal("General apertura", apertura[1].Leyenda);

        var clausura = (await GetPosiciones("posiciones-todos-contra-todos", e.ZonaClausuraId)).Posiciones.ToList();
        Assert.Equal("Cat clausura", clausura[0].Leyenda);
        Assert.Equal("General clausura", clausura[1].Leyenda);
    }

    [Fact]
    public async Task TablaAnual_SinLeyendasPropias_NoMuestraLasDeAperturaNiClausura()
    {
        var e = await SeedAperturaYClausura();
        var client = await GetAuthenticatedClient();
        await client.PostAsJsonAsync($"/api/Zona/{e.ZonaAperturaId}/leyendas",
            Leyenda(e.ZonaAperturaId, null, "General apertura", false));
        await client.PostAsJsonAsync($"/api/Zona/{e.ZonaClausuraId}/leyendas",
            Leyenda(e.ZonaClausuraId, null, "General clausura", false));

        var anual = (await GetPosiciones("posiciones-anual", e.ZonaAperturaId)).Posiciones.ToList();
        Assert.All(anual, b => Assert.Null(b.Leyenda));
    }

    [Fact]
    public async Task TablaAnual_SigueDescontandoLasQuitasDeAperturaYClausura()
    {
        var e = await SeedAperturaYClausura();
        var client = await GetAuthenticatedClient();
        var quita = Leyenda(e.ZonaAperturaId, e.CatAperturaId, "Quita apertura", false);
        quita.EquipoId = e.EquipoId;
        quita.QuitaDePuntos = 2;
        Assert.Equal(HttpStatusCode.OK,
            (await client.PostAsJsonAsync($"/api/Zona/{e.ZonaAperturaId}/leyendas", quita)).StatusCode);

        var anual = (await GetPosiciones("posiciones-anual", e.ZonaAperturaId)).Posiciones.ToList();
        Assert.Equal(-2, int.Parse(anual[0].Renglones.Single(r => r.Equipo == "Equipo Anual").Puntos));
        Assert.Null(anual[0].Leyenda);
    }

    [Fact]
    public async Task PostLeyendaAnual_EnZonaDeClausura_400()
    {
        var e = await SeedAperturaYClausura();
        var client = await GetAuthenticatedClient();
        var r = await client.PostAsJsonAsync($"/api/Zona/{e.ZonaClausuraId}/leyendas",
            Leyenda(e.ZonaClausuraId, null, "No va", true));
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task PostLeyendaAnual_EnTorneoSinTablaAnual_400()
    {
        var e = await SeedAperturaYClausura();
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var torneo = await ctx.Torneos.FirstAsync(t => t.Id == e.TorneoId);
            torneo.FaseAperturaId = null;
            torneo.FaseClausuraId = null;
            await ctx.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var r = await client.PostAsJsonAsync($"/api/Zona/{e.ZonaAperturaId}/leyendas",
            Leyenda(e.ZonaAperturaId, null, "No va", true));
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task PostLeyendaAnual_ConQuitaDePuntos_400()
    {
        var e = await SeedAperturaYClausura();
        var client = await GetAuthenticatedClient();
        var dto = Leyenda(e.ZonaAperturaId, e.CatAperturaId, "Con quita", true);
        dto.EquipoId = e.EquipoId;
        dto.QuitaDePuntos = 3;
        var r = await client.PostAsJsonAsync($"/api/Zona/{e.ZonaAperturaId}/leyendas", dto);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task PostLeyendaAnual_GeneralDuplicada_400()
    {
        var e = await SeedAperturaYClausura();
        var client = await GetAuthenticatedClient();
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync($"/api/Zona/{e.ZonaAperturaId}/leyendas",
            Leyenda(e.ZonaAperturaId, null, "Primera", true))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync($"/api/Zona/{e.ZonaAperturaId}/leyendas",
            Leyenda(e.ZonaAperturaId, null, "Segunda", true))).StatusCode);
    }

    [Fact]
    public async Task GetLeyendas_DevuelveLaMarcaDeTablaAnual()
    {
        var e = await SeedAperturaYClausura();
        var client = await GetAuthenticatedClient();
        await client.PostAsJsonAsync($"/api/Zona/{e.ZonaAperturaId}/leyendas",
            Leyenda(e.ZonaAperturaId, null, "De la fase", false));
        await client.PostAsJsonAsync($"/api/Zona/{e.ZonaAperturaId}/leyendas",
            Leyenda(e.ZonaAperturaId, null, "De la anual", true));

        var lista = await client.GetFromJsonAsync<List<LeyendaTablaPosicionesDTO>>(
            $"/api/Zona/{e.ZonaAperturaId}/leyendas");
        Assert.NotNull(lista);
        Assert.False(lista.Single(l => l.Leyenda == "De la fase").EsDeTablaAnual);
        Assert.True(lista.Single(l => l.Leyenda == "De la anual").EsDeTablaAnual);
    }

    [Fact]
    public async Task CambiarFaseApertura_BorraLasLeyendasAnuales_YConservaLasDeLaFase()
    {
        var e = await SeedAperturaYClausura();
        var client = await GetAuthenticatedClient();
        await client.PostAsJsonAsync($"/api/Zona/{e.ZonaAperturaId}/leyendas",
            Leyenda(e.ZonaAperturaId, null, "De la fase", false));
        await client.PostAsJsonAsync($"/api/Zona/{e.ZonaAperturaId}/leyendas",
            Leyenda(e.ZonaAperturaId, null, "De la anual", true));

        // Mismas fases: no borra nada.
        var mismas = await client.PutAsJsonAsync($"/api/Torneo/{e.TorneoId}/fases-para-tabla-anual",
            new EditarFasesParaTablaAnualDTO { FaseAperturaId = e.FaseAperturaId, FaseClausuraId = e.FaseClausuraId });
        mismas.EnsureSuccessStatusCode();
        Assert.Equal(2, await ContarLeyendas(e.ZonaAperturaId));

        // Sin tabla anual: se borran las leyendas anuales.
        var sinAnual = await client.PutAsJsonAsync($"/api/Torneo/{e.TorneoId}/fases-para-tabla-anual",
            new EditarFasesParaTablaAnualDTO { FaseAperturaId = null, FaseClausuraId = null });
        sinAnual.EnsureSuccessStatusCode();

        await using var scope = Factory.Services.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var restantes = await ctx.LeyendaTablaPosiciones.Where(l => l.ZonaId == e.ZonaAperturaId).ToListAsync();
        Assert.Equal("De la fase", Assert.Single(restantes).Leyenda);
    }

    private async Task<int> ContarLeyendas(int zonaId)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await ctx.LeyendaTablaPosiciones.CountAsync(l => l.ZonaId == zonaId);
    }
}
