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
/// Integración del endpoint <c>GET /api/carnet-digital/jornadas-todos-contra-todos</c> (app carnet digital).
/// </summary>
public class JornadasTodosContraTodosAppIT : TestBase
{
    private static readonly JsonSerializerOptions FechaJsonOptions = CreateFechaJsonOptions();

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
        var torneo = new Torneo { Id = 0, Nombre = "Torneo Jornadas App", Anio = 2026, TorneoAgrupadorId = 1, EsVisibleEnApp = true };
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

        var zona = new ZonaTodosContraTodos { Id = 0, Nombre = "Zona jornadas app", FaseId = fase.Id };
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

    private static async Task SeedTorneoCategorias(CustomWebApplicationFactory<Program> factory, int torneoId, int cantidad)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        for (var i = 0; i < cantidad; i++)
        {
            context.TorneoCategorias.Add(new TorneoCategoria
            {
                Id = 0,
                Nombre = $"Cat Jornadas {i}",
                AnioDesde = 2010,
                AnioHasta = 2020,
                TorneoId = torneoId
            });
        }

        await context.SaveChangesAsync();
    }

    private Club? _club;

    public JornadasTodosContraTodosAppIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var util = new global::Api.TestsUtilidades.Utilidades(context);
        _club = util.DadoQueExisteElClub();
        util.DadoQueExisteElEquipo(_club);
        context.SaveChanges();
    }

    [Fact]
    public async Task JornadasTodosContraTodos_ZonaSinFechas_DevuelveFechasVacia()
    {
        var zonaId = await CrearZonaDePrueba(Factory);
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/carnet-digital/jornadas-todos-contra-todos?zonaId={zonaId}");

        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<JornadasDTO>();
        Assert.NotNull(dto);
        Assert.NotNull(dto.Fechas);
        Assert.Empty(dto.Fechas);
    }

    [Fact]
    public async Task JornadasTodosContraTodos_ConFechaNormalYLibre_ResultadosCargados_DevuelveEstructuraYTextoResultado()
    {
        Assert.NotNull(_club);
        var zonaId = await CrearZonaDePrueba(Factory);
        var torneoId = await ObtenerTorneoIdDeZona(Factory, zonaId);
        await SeedTorneoCategorias(Factory, torneoId, 2);
        var client = await GetAuthenticatedClient();

        int equipo1Id;
        int equipo2Id;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var equipos = context.Equipos.Where(e => e.ClubId == _club!.Id).ToList();
            equipo1Id = equipos[0].Id;
            var eq2 = new Equipo { Id = 0, Nombre = "Equipo rival jornadas", ClubId = _club.Id, Jugadores = [] };
            context.Equipos.Add(eq2);
            context.SaveChanges();
            equipo2Id = eq2.Id;
            context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = equipo2Id, ZonaId = zonaId });
            context.SaveChanges();
        }

        var postFechas = await client.PostAsJsonAsync(
            $"/api/Zona/{zonaId}/fechas/crear-fechas-todoscontratodos-masivamente",
            new List<FechaTodosContraTodosDTO>
            {
                new()
                {
                    Dia = new DateOnly(2026, 7, 1),
                    Numero = 1,
                    EsVisibleEnApp = true,
                    Jornadas =
                    [
                        new JornadaDTO
                        {
                            Tipo = "Normal",
                            ResultadosVerificados = false,
                            LocalId = equipo1Id,
                            VisitanteId = equipo2Id
                        },
                        new JornadaDTO
                        {
                            Tipo = "Libre",
                            ResultadosVerificados = false,
                            EquipoLocalId = equipo1Id
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
                .FirstAsync(j => j.LocalEquipoId == equipo1Id && j.VisitanteEquipoId == equipo2Id);
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
                    ResultadoVisitante = "0"
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
        var response = await carnetClient.GetAsync($"/api/carnet-digital/jornadas-todos-contra-todos?zonaId={zonaId}");
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<JornadasDTO>();

        Assert.NotNull(dto?.Fechas);
        var bloque = Assert.Single(dto.Fechas);
        Assert.Equal("Fecha 1", bloque.Titulo);
        Assert.NotNull(bloque.Jornadas);
        Assert.Equal(2, bloque.Jornadas.Count);

        var jornadasLista = bloque.Jornadas.ToList();
        var normal = jornadasLista.Single(j =>
            j.Local.Equipo == "un equipo" && j.Visitante.Equipo == "Equipo rival jornadas");
        var libre = jornadasLista.Single(j =>
            j.Local.Equipo == "un equipo" && j.Visitante.Equipo == "LIBRE");
        Assert.Equal("un equipo", normal.Local.Equipo);
        Assert.Equal("Equipo rival jornadas", normal.Visitante.Equipo);
        Assert.Equal($"/Imagenes/Escudos/{_club.Id}.jpg", normal.Local.Escudo);
        Assert.Equal($"/Imagenes/Escudos/{_club.Id}.jpg", normal.Visitante.Escudo);

        var resLocal = normal.Local.Categorias.OrderBy(c => c.Categoria).Select(c => c.Resultado).ToList();
        var resVisit = normal.Visitante.Categorias.OrderBy(c => c.Categoria).Select(c => c.Resultado).ToList();
        Assert.Equal(resLocal, resVisit);
        Assert.Contains("2 - 1", resLocal);
        Assert.Contains("1 - 0", resLocal);
        Assert.Equal("un equipo", libre.Local.Equipo);
        Assert.Equal("LIBRE", libre.Visitante.Equipo);
        Assert.Empty(libre.Visitante.Escudo);
        Assert.Equal(2, libre.Local.Categorias.Count);
    }

    [Fact]
    public async Task JornadasTodosContraTodos_DosFechasVisibles_DevuelveDosBloquesConTitulosFecha1yFecha2()
    {
        Assert.NotNull(_club);
        var zonaId = await CrearZonaDePrueba(Factory);
        var torneoId = await ObtenerTorneoIdDeZona(Factory, zonaId);
        await SeedTorneoCategorias(Factory, torneoId, 1);
        var client = await GetAuthenticatedClient();

        int e1;
        int e2;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            e1 = context.Equipos.First(e => e.ClubId == _club!.Id).Id;
            var eq2 = new Equipo { Id = 0, Nombre = "Eq fecha 2", ClubId = _club.Id, Jugadores = [] };
            context.Equipos.Add(eq2);
            context.SaveChanges();
            e2 = eq2.Id;
            context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = e2, ZonaId = zonaId });
            context.SaveChanges();
        }

        var postFechas = await client.PostAsJsonAsync(
            $"/api/Zona/{zonaId}/fechas/crear-fechas-todoscontratodos-masivamente",
            new List<FechaTodosContraTodosDTO>
            {
                new()
                {
                    Dia = new DateOnly(2026, 8, 1),
                    Numero = 1,
                    EsVisibleEnApp = true,
                    Jornadas =
                    [
                        new JornadaDTO { Tipo = "Normal", ResultadosVerificados = false, LocalId = e1, VisitanteId = e2 }
                    ]
                },
                new()
                {
                    Dia = new DateOnly(2026, 8, 8),
                    Numero = 2,
                    EsVisibleEnApp = true,
                    Jornadas =
                    [
                        new JornadaDTO { Tipo = "Normal", ResultadosVerificados = false, LocalId = e2, VisitanteId = e1 }
                    ]
                }
            }, FechaJsonOptions);
        postFechas.EnsureSuccessStatusCode();

        var carnetClient = Factory.CreateClient();
        var response = await carnetClient.GetAsync($"/api/carnet-digital/jornadas-todos-contra-todos?zonaId={zonaId}");
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<JornadasDTO>();

        Assert.NotNull(dto?.Fechas);
        Assert.Equal(2, dto.Fechas.Count);
        Assert.Equal("Fecha 1", dto.Fechas.ElementAt(0).Titulo);
        Assert.Single(dto.Fechas.ElementAt(0).Jornadas);
        Assert.Equal("Fecha 2", dto.Fechas.ElementAt(1).Titulo);
        Assert.Single(dto.Fechas.ElementAt(1).Jornadas);
    }
}
