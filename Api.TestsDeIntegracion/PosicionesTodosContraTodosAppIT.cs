using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api._Config;
using Api.Core.DTOs;
using Api.Core.DTOs.AppCarnetDigital;
using Api.Core.Entidades;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.TestsDeIntegracion;

/// <summary>
/// Integración de <c>GET /api/carnet-digital/posiciones-todos-contra-todos</c>; el bloque General va al final de la lista.
/// </summary>
public class PosicionesTodosContraTodosAppIT : TestBase
{
    private static readonly JsonSerializerOptions FechaJsonOptions = CreateFechaJsonOptions();

    private Club? _club;

    public PosicionesTodosContraTodosAppIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var util = new global::Api.TestsUtilidades.Utilidades(context);
        _club = util.DadoQueExisteElClub();
        util.DadoQueExisteElEquipo(_club);
        context.SaveChanges();
    }

    private static JsonSerializerOptions CreateFechaJsonOptions()
    {
        var o = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        o.Converters.Add(new DateOnlyJsonConverter());
        o.Converters.Add(new FechaJsonConverter());
        o.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        return o;
    }

    private static async Task<int> CrearZonaDePrueba(CustomWebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var torneo = new Torneo
        {
            Id = 0,
            Nombre = "Torneo Posiciones App",
            Anio = 2026,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };
        context.Torneos.Add(torneo);
        await context.SaveChangesAsync();

        var fase = new FaseTodosContraTodos
        {
            Id = 0,
            Nombre = "",
            Numero = 1,
            TorneoId = torneo.Id,
            EstadoFaseId = 100,
            EsVisibleEnApp = true
        };
        context.Fases.Add(fase);
        await context.SaveChangesAsync();

        var zona = new ZonaTodosContraTodos { Id = 0, Nombre = "Zona posiciones app", FaseId = fase.Id };
        context.Zonas.Add(zona);
        await context.SaveChangesAsync();
        return zona.Id;
    }

    private static async Task<int> ObtenerTorneoIdDeZona(CustomWebApplicationFactory<Program> factory, int zonaId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await (from z in context.Zonas
            join f in context.Fases on z.FaseId equals f.Id
            where z.Id == zonaId
            select f.TorneoId).FirstAsync();
    }

    private static async Task<(int CatId1, int CatId2)> SeedDosCategorias(CustomWebApplicationFactory<Program> factory,
        int torneoId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var c1 = new TorneoCategoria
        {
            Id = 0,
            Nombre = "Cat A",
            AnioDesde = 2010,
            AnioHasta = 2020,
            TorneoId = torneoId
        };
        var c2 = new TorneoCategoria
        {
            Id = 0,
            Nombre = "Cat B",
            AnioDesde = 2010,
            AnioHasta = 2020,
            TorneoId = torneoId
        };
        context.TorneoCategorias.AddRange(c1, c2);
        await context.SaveChangesAsync();
        return (c1.Id, c2.Id);
    }

    private static async Task<int> SeedUnaCategoria(CustomWebApplicationFactory<Program> factory, int torneoId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var c = new TorneoCategoria
        {
            Id = 0,
            Nombre = "Única",
            AnioDesde = 2010,
            AnioHasta = 2020,
            TorneoId = torneoId
        };
        context.TorneoCategorias.Add(c);
        await context.SaveChangesAsync();
        return c.Id;
    }

    [Fact]
    public async Task PosicionesTodosContraTodos_SinLeyendas_BloqueGeneralUltimoYLeyendasVacias()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        var torneoId = await ObtenerTorneoIdDeZona(Factory, zonaId);
        await SeedDosCategorias(Factory, torneoId);

        var client = Factory.CreateClient();
        var response = await client.GetAsync($"/api/carnet-digital/posiciones-todos-contra-todos?zonaId={zonaId}");
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<PosicionesDTO>();

        Assert.NotNull(dto);
        Assert.NotNull(dto.Posiciones);
        var bloques = dto.Posiciones.ToList();
        Assert.Equal(3, bloques.Count);
        Assert.Equal("Cat A", bloques[0].Categoria);
        Assert.Equal("Cat B", bloques[1].Categoria);
        Assert.Equal("General", bloques[2].Categoria);
        foreach (var b in bloques)
            Assert.True(string.IsNullOrEmpty(b.Leyenda));
    }

    [Fact]
    public async Task PosicionesTodosContraTodos_ConLeyendaSinCategoriaYporCategoria_MapeaEnBloqueGeneralYCategorias()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        var torneoId = await ObtenerTorneoIdDeZona(Factory, zonaId);
        var (catId1, catId2) = await SeedDosCategorias(Factory, torneoId);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.LeyendaTablaPosiciones.AddRange(
                new LeyendaTablaPosiciones
                {
                    Id = 0,
                    ZonaId = zonaId,
                    CategoriaId = null,
                    Leyenda = "Nota general"
                },
                new LeyendaTablaPosiciones
                {
                    Id = 0,
                    ZonaId = zonaId,
                    CategoriaId = catId1,
                    Leyenda = "Nota cat A"
                },
                new LeyendaTablaPosiciones
                {
                    Id = 0,
                    ZonaId = zonaId,
                    CategoriaId = catId2,
                    Leyenda = "Nota cat B"
                });
            await context.SaveChangesAsync();
        }

        var client = Factory.CreateClient();
        var response = await client.GetAsync($"/api/carnet-digital/posiciones-todos-contra-todos?zonaId={zonaId}");
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<PosicionesDTO>();

        Assert.NotNull(dto);
        var bloques = dto.Posiciones!.ToList();
        Assert.Equal(3, bloques.Count);
        Assert.Equal("Nota cat A", bloques[0].Leyenda);
        Assert.Equal("Nota cat B", bloques[1].Leyenda);
        Assert.Equal("General", bloques[2].Categoria);
        Assert.Equal("Nota general", bloques[2].Leyenda);
    }

    [Fact]
    public async Task PosicionesTodosContraTodos_UnaCategoria_GeneralMismosValoresQueLaCategoria()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        var torneoId = await ObtenerTorneoIdDeZona(Factory, zonaId);
        await SeedUnaCategoria(Factory, torneoId);

        var client = Factory.CreateClient();
        var response = await client.GetAsync($"/api/carnet-digital/posiciones-todos-contra-todos?zonaId={zonaId}");
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<PosicionesDTO>();

        Assert.NotNull(dto?.Posiciones);
        var bloques = dto.Posiciones.ToList();
        Assert.Equal(2, bloques.Count);
        var unica = bloques[0];
        var general = bloques[1];
        Assert.Equal("Única", unica.Categoria);
        Assert.Equal("General", general.Categoria);
        Assert.Equal(general.Renglones.Count, unica.Renglones.Count);
        var porEqGen = general.Renglones.ToDictionary(r => r.Equipo);
        foreach (var fila in unica.Renglones)
        {
            var g = porEqGen[fila.Equipo];
            Assert.Equal(fila.Puntos, g.Puntos);
            Assert.Equal(fila.PartidosJugados, g.PartidosJugados);
            Assert.Equal(fila.PartidosGanados, g.PartidosGanados);
            Assert.Equal(fila.PartidosEmpatados, g.PartidosEmpatados);
            Assert.Equal(fila.PartidosPerdidos, g.PartidosPerdidos);
            Assert.Equal(fila.GolesAFavor, g.GolesAFavor);
            Assert.Equal(fila.GolesEnContra, g.GolesEnContra);
            Assert.Equal(fila.GolesDiferencia, g.GolesDiferencia);
            Assert.Equal(fila.PartidosNoPresento, g.PartidosNoPresento);
        }
    }

    [Fact]
    public async Task PosicionesTodosContraTodos_DosCategorias_SumaPuntosYEstadisticasYDesempataPorNombreEquipo()
    {
        Assert.NotNull(_club);
        var zonaId = await CrearZonaDePrueba(Factory);
        var torneoId = await ObtenerTorneoIdDeZona(Factory, zonaId);
        await SeedDosCategorias(Factory, torneoId);

        int equipoLocalId;
        int equipoAlphaId;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            equipoLocalId = context.Equipos.First(e => e.ClubId == _club!.Id).Id;
            var eqAlpha = new Equipo
            {
                Id = 0,
                Nombre = "Alpha",
                ClubId = _club.Id,
                Jugadores = []
            };
            context.Equipos.Add(eqAlpha);
            context.SaveChanges();
            equipoAlphaId = eqAlpha.Id;
            context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = equipoLocalId, ZonaId = zonaId });
            context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = equipoAlphaId, ZonaId = zonaId });
            context.SaveChanges();
        }

        var client = await GetAuthenticatedClient();
        var postFechas = await client.PostAsJsonAsync(
            $"/api/Zona/{zonaId}/fechas/crear-fechas-todoscontratodos-masivamente",
            new List<FechaTodosContraTodosDTO>
            {
                new()
                {
                    Dia = new DateOnly(2026, 9, 1),
                    Numero = 1,
                    EsVisibleEnApp = true,
                    Jornadas =
                    [
                        new JornadaDTO
                        {
                            Tipo = "Normal",
                            ResultadosVerificados = false,
                            LocalId = equipoLocalId,
                            VisitanteId = equipoAlphaId
                        }
                    ]
                }
            }, FechaJsonOptions);
        postFechas.EnsureSuccessStatusCode();

        int jornadaNormalId;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var jornadaNormal = await context.Jornadas.OfType<JornadaNormal>()
                .FirstAsync(j => j.LocalEquipoId == equipoLocalId && j.VisitanteEquipoId == equipoAlphaId);
            jornadaNormalId = jornadaNormal.Id;
        }

        List<PartidoDTO> partidosCargar;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var partidosDb = await context.Partidos
                .Where(p => p.JornadaId == jornadaNormalId)
                .OrderBy(p => p.CategoriaId)
                .ToListAsync();
            Assert.Equal(2, partidosDb.Count);
            partidosCargar =
            [
                new PartidoDTO
                {
                    Id = partidosDb[0].Id,
                    CategoriaId = partidosDb[0].CategoriaId,
                    ResultadoLocal = "2",
                    ResultadoVisitante = "1"
                },
                new PartidoDTO
                {
                    Id = partidosDb[1].Id,
                    CategoriaId = partidosDb[1].CategoriaId,
                    ResultadoLocal = "1",
                    ResultadoVisitante = "2"
                }
            ];
        }

        var cargar = new CargarResultadosDTO
        {
            JornadaId = jornadaNormalId,
            ResultadosVerificados = true,
            Partidos = partidosCargar
        };
        var postRes = await client.PostAsJsonAsync(
            $"/api/Zona/{zonaId}/fechas/cargar-resultados/{jornadaNormalId}", cargar, FechaJsonOptions);
        postRes.EnsureSuccessStatusCode();

        var carnetClient = Factory.CreateClient();
        var response = await carnetClient.GetAsync($"/api/carnet-digital/posiciones-todos-contra-todos?zonaId={zonaId}");
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<PosicionesDTO>();

        Assert.NotNull(dto?.Posiciones);
        var bloques = dto.Posiciones.ToList();
        Assert.Equal(3, bloques.Count);
        var catA = bloques[0];
        var catB = bloques[1];
        var general = bloques[2];
        Assert.Equal("General", general.Categoria);
        Assert.Equal(2, general.Renglones.Count);
        var puntosLocalCatA = int.Parse(catA.Renglones.Single(r => r.Equipo == "un equipo").Puntos);
        var puntosAlphaCatA = int.Parse(catA.Renglones.Single(r => r.Equipo == "Alpha").Puntos);
        var puntosLocalCatB = int.Parse(catB.Renglones.Single(r => r.Equipo == "un equipo").Puntos);
        var puntosAlphaCatB = int.Parse(catB.Renglones.Single(r => r.Equipo == "Alpha").Puntos);

        // Puntos: victoria 3, empate 2, derrota 1 (ver PosicionesTodosContraTodosLogica.AcumularPuntos).
        Assert.Equal(3, puntosLocalCatA);
        Assert.Equal(1, puntosAlphaCatA);
        Assert.Equal(1, puntosLocalCatB);
        Assert.Equal(3, puntosAlphaCatB);

        var gLocal = int.Parse(general.Renglones.Single(r => r.Equipo == "un equipo").Puntos);
        var gAlpha = int.Parse(general.Renglones.Single(r => r.Equipo == "Alpha").Puntos);
        Assert.Equal(4, gLocal);
        Assert.Equal(4, gAlpha);

        var pjLocal = int.Parse(general.Renglones.Single(r => r.Equipo == "un equipo").PartidosJugados);
        var pjAlpha = int.Parse(general.Renglones.Single(r => r.Equipo == "Alpha").PartidosJugados);
        Assert.Equal(
            int.Parse(catA.Renglones.Single(r => r.Equipo == "un equipo").PartidosJugados) +
            int.Parse(catB.Renglones.Single(r => r.Equipo == "un equipo").PartidosJugados),
            pjLocal);
        Assert.Equal(
            int.Parse(catA.Renglones.Single(r => r.Equipo == "Alpha").PartidosJugados) +
            int.Parse(catB.Renglones.Single(r => r.Equipo == "Alpha").PartidosJugados),
            pjAlpha);

        var gr = general.Renglones.ToList();
        Assert.Equal("1", gr[0].Posicion);
        Assert.Equal("2", gr[1].Posicion);
        Assert.Equal("Alpha", gr[0].Equipo);
        Assert.Equal("un equipo", gr[1].Equipo);
    }
}
