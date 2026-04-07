using System.Net.Http.Json;
using Api.Core.DTOs.AppCarnetDigital;
using Api.Core.Entidades;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;

namespace Api.TestsDeIntegracion;

public class AppCarnetDigitalEliminacionDirectaIT : TestBase
{
    public AppCarnetDigitalEliminacionDirectaIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    /// <summary>
    /// Zona ED con cuatro fechas (octavos, cuartos, semifinal, final), una jornada normal y partido por fecha.
    /// </summary>
    private static async Task<int> SeedZonaEliminacionDirectaOctavosHastaFinal(
        CustomWebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var torneo = new Torneo
        {
            Id = 0,
            Nombre = "Torneo ED App Carnet",
            Anio = 2026,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };
        context.Torneos.Add(torneo);
        await context.SaveChangesAsync();

        var fase = new FaseEliminacionDirecta
        {
            Id = 0,
            Nombre = "Fase ED",
            Numero = 1,
            TorneoId = torneo.Id,
            EstadoFaseId = 100,
            EsVisibleEnApp = true
        };
        context.Fases.Add(fase);
        await context.SaveChangesAsync();

        var cat = new TorneoCategoria
        {
            Id = 0,
            Nombre = "Cat ED",
            AnioDesde = 2010,
            AnioHasta = 2020,
            TorneoId = torneo.Id
        };
        context.TorneoCategorias.Add(cat);
        await context.SaveChangesAsync();

        var zona = new ZonaEliminacionDirecta
        {
            Id = 0,
            Nombre = "Zona ED",
            FaseId = fase.Id,
            CategoriaId = cat.Id
        };
        context.Zonas.Add(zona);
        await context.SaveChangesAsync();
        var zonaId = zona.Id;

        var club = new Club
        {
            Id = 0,
            Nombre = "Club ED",
            Localidad = "Rosario",
            Direccion = "Calle 1",
            EsTechado = true
        };
        context.Clubs.Add(club);
        await context.SaveChangesAsync();

        var eq1 = new Equipo
        {
            Id = 0,
            Nombre = "Equipo Alpha",
            ClubId = club.Id,
            Jugadores = []
        };
        var eq2 = new Equipo
        {
            Id = 0,
            Nombre = "Equipo Beta",
            ClubId = club.Id,
            Jugadores = []
        };
        context.Equipos.AddRange(eq1, eq2);
        await context.SaveChangesAsync();

        context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = eq1.Id, ZonaId = zonaId });
        context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = eq2.Id, ZonaId = zonaId });
        await context.SaveChangesAsync();

        var instanciaIds = new[] { 16, 8, 4, 2 };
        var dia = 10;
        foreach (var instanciaId in instanciaIds)
        {
            var fecha = new FechaEliminacionDirecta
            {
                Id = 0,
                Dia = new DateOnly(2026, 7, dia),
                EsVisibleEnApp = true,
                ZonaId = zonaId,
                InstanciaId = instanciaId
            };
            context.Fechas.Add(fecha);
            await context.SaveChangesAsync();
            dia++;

            var jornada = new JornadaNormal
            {
                Id = 0,
                FechaId = fecha.Id,
                ResultadosVerificados = false,
                LocalEquipoId = eq1.Id,
                VisitanteEquipoId = eq2.Id,
                Partidos = []
            };
            context.Jornadas.Add(jornada);
            await context.SaveChangesAsync();

            var esFinal = instanciaId == 2;
            context.Partidos.Add(new Partido
            {
                Id = 0,
                CategoriaId = cat.Id,
                JornadaId = jornada.Id,
                ResultadoLocal = "2",
                ResultadoVisitante = "1",
                PenalesLocal = esFinal ? "4" : null,
                PenalesVisitante = esFinal ? "3" : null
            });
            await context.SaveChangesAsync();
        }

        return zonaId;
    }

    [Fact]
    public async Task EliminacionDirecta_CuatroInstanciasOctavosHastaFinal_DevuelveTitulosDiasPartidosYPenalesEnFinal()
    {
        var zonaId = await SeedZonaEliminacionDirectaOctavosHastaFinal(Factory);
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/carnet-digital/eliminacion-directa?zonaId={zonaId}");
        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<EliminacionDirectaDTO>();
        Assert.NotNull(dto?.Instancias);
        var bloques = dto.Instancias.ToList();
        Assert.Equal(4, bloques.Count);

        // Orden: InstanciaId descendente (16 → 8 → 4 → 2)
        Assert.Equal("Octavos de final", bloques[0].Titulo);
        Assert.Equal("Cuartos de final", bloques[1].Titulo);
        Assert.Equal("Semifinal", bloques[2].Titulo);
        Assert.Equal("Final", bloques[3].Titulo);

        Assert.Equal("10-07", bloques[0].Dia);
        Assert.Equal("11-07", bloques[1].Dia);
        Assert.Equal("12-07", bloques[2].Dia);
        Assert.Equal("13-07", bloques[3].Dia);

        foreach (var bloque in bloques)
        {
            var partidos = bloque.Partidos.ToList();
            Assert.Single(partidos);
            var p = partidos[0];
            Assert.Equal("Equipo Alpha", p.Local);
            Assert.Equal("Equipo Beta", p.Visitante);
            Assert.Equal("2", p.ResultadoLocal);
            Assert.Equal("1", p.ResultadoVisitante);
            Assert.Contains(".jpg", p.EscudoLocal, StringComparison.Ordinal);
            Assert.Contains(".jpg", p.EscudoVisitante, StringComparison.Ordinal);
        }

        Assert.Equal(string.Empty, bloques[0].Partidos.Single().PenalesLocal);
        Assert.Equal(string.Empty, bloques[0].Partidos.Single().PenalesVisitante);

        var final = bloques[3].Partidos.Single();
        Assert.Equal("4", final.PenalesLocal);
        Assert.Equal("3", final.PenalesVisitante);
    }
}
