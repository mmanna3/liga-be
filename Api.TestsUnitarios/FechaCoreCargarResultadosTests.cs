using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Entidades.EntidadesConValoresPredefinidos;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios;
using Api.Persistencia._Config;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Api.TestsUnitarios;

public class FechaCoreCargarResultadosTests
{
    private static FechaCore CrearCore(AppDbContext ctx)
    {
        var bd = new BDVirtual(ctx);
        var repoMock = new Mock<IFechaRepo>();
        var zonaRepoMock = new Mock<IZonaRepo>();
        var mapperMock = new Mock<IMapper>();
        return new FechaCore(bd, repoMock.Object, zonaRepoMock.Object, ctx, mapperMock.Object);
    }

    /// <summary>
    /// Jornada con dos partidos (dos categorías del torneo). Devuelve zonaId, jornadaId, ids de partidos y categorías.
    /// </summary>
    private static async Task<(int zonaId, int jornadaId, int p1Id, int p2Id, int cat1Id, int cat2Id)>
        SeedJornadaConDosPartidos(AppDbContext ctx)
    {
        var agr = new TorneoAgrupador { Id = 0, Nombre = "G", EsVisibleEnApp = false, ColorId = (int)ColorEnum.Negro };
        ctx.TorneoAgrupadores.Add(agr);
        await ctx.SaveChangesAsync();

        var torneo = new Torneo
        {
            Id = 0,
            Nombre = "T",
            Anio = 2026,
            EsVisibleEnApp = true,
            TorneoAgrupadorId = agr.Id,
            Categorias = [],
            Fases = []
        };
        ctx.Torneos.Add(torneo);
        await ctx.SaveChangesAsync();

        var cat1 = new TorneoCategoria
        {
            Id = 0,
            Nombre = "CatA",
            AnioDesde = 2010,
            AnioHasta = 2020,
            TorneoId = torneo.Id
        };
        var cat2 = new TorneoCategoria
        {
            Id = 0,
            Nombre = "CatB",
            AnioDesde = 2010,
            AnioHasta = 2020,
            TorneoId = torneo.Id
        };
        ctx.TorneoCategorias.AddRange(cat1, cat2);
        await ctx.SaveChangesAsync();

        if (!await ctx.EstadoFase.AnyAsync(e => e.Id == 100))
            ctx.EstadoFase.Add(new EstadoFase { Id = 100, Estado = "Inicio pendiente" });
        await ctx.SaveChangesAsync();

        var fase = new FaseTodosContraTodos
        {
            Id = 0,
            Nombre = "",
            Numero = 1,
            TorneoId = torneo.Id,
            EstadoFaseId = 100,
            EsVisibleEnApp = true
        };
        ctx.Fases.Add(fase);
        await ctx.SaveChangesAsync();

        var zona = new ZonaTodosContraTodos { Id = 0, Nombre = "Z", FaseId = fase.Id };
        ctx.Zonas.Add(zona);
        await ctx.SaveChangesAsync();

        var club = new Club { Id = 0, Nombre = "Club", Equipos = [], DelegadoClubs = [] };
        ctx.Clubs.Add(club);
        await ctx.SaveChangesAsync();

        var e1 = new Equipo { Id = 0, Nombre = "E1", ClubId = club.Id, Jugadores = [], Zonas = [] };
        var e2 = new Equipo { Id = 0, Nombre = "E2", ClubId = club.Id, Jugadores = [], Zonas = [] };
        ctx.Equipos.AddRange(e1, e2);
        await ctx.SaveChangesAsync();

        var fecha = new FechaTodosContraTodos
        {
            Id = 0,
            Dia = new DateOnly(2026, 1, 1),
            Numero = 1,
            ZonaId = zona.Id,
            EsVisibleEnApp = true
        };
        ctx.Fechas.Add(fecha);
        await ctx.SaveChangesAsync();

        var jornada = new JornadaNormal
        {
            Id = 0,
            FechaId = fecha.Id,
            ResultadosVerificados = false,
            LocalEquipoId = e1.Id,
            VisitanteEquipoId = e2.Id
        };
        ctx.Jornadas.Add(jornada);
        await ctx.SaveChangesAsync();

        var p1 = new Partido
        {
            Id = 0,
            JornadaId = jornada.Id,
            CategoriaId = cat1.Id,
            ResultadoLocal = "",
            ResultadoVisitante = ""
        };
        var p2 = new Partido
        {
            Id = 0,
            JornadaId = jornada.Id,
            CategoriaId = cat2.Id,
            ResultadoLocal = "",
            ResultadoVisitante = ""
        };
        ctx.Partidos.AddRange(p1, p2);
        await ctx.SaveChangesAsync();

        return (zona.Id, jornada.Id, p1.Id, p2.Id, cat1.Id, cat2.Id);
    }

    private static async Task<(int zonaId, int jornadaId)> SeedJornadaSinPartidos(AppDbContext ctx)
    {
        var agr = new TorneoAgrupador { Id = 0, Nombre = "G2", EsVisibleEnApp = false, ColorId = (int)ColorEnum.Negro };
        ctx.TorneoAgrupadores.Add(agr);
        await ctx.SaveChangesAsync();

        var torneo = new Torneo
        {
            Id = 0,
            Nombre = "T2",
            Anio = 2026,
            EsVisibleEnApp = true,
            TorneoAgrupadorId = agr.Id,
            Categorias = [],
            Fases = []
        };
        ctx.Torneos.Add(torneo);
        await ctx.SaveChangesAsync();

        if (!await ctx.EstadoFase.AnyAsync(e => e.Id == 100))
            ctx.EstadoFase.Add(new EstadoFase { Id = 100, Estado = "Inicio pendiente" });
        await ctx.SaveChangesAsync();

        var fase = new FaseTodosContraTodos
        {
            Id = 0,
            Nombre = "",
            Numero = 1,
            TorneoId = torneo.Id,
            EstadoFaseId = 100,
            EsVisibleEnApp = true
        };
        ctx.Fases.Add(fase);
        await ctx.SaveChangesAsync();

        var zona = new ZonaTodosContraTodos { Id = 0, Nombre = "Z2", FaseId = fase.Id };
        ctx.Zonas.Add(zona);
        await ctx.SaveChangesAsync();

        var club = new Club { Id = 0, Nombre = "C2", Equipos = [], DelegadoClubs = [] };
        ctx.Clubs.Add(club);
        await ctx.SaveChangesAsync();

        var e1 = new Equipo { Id = 0, Nombre = "A", ClubId = club.Id, Jugadores = [], Zonas = [] };
        var e2 = new Equipo { Id = 0, Nombre = "B", ClubId = club.Id, Jugadores = [], Zonas = [] };
        ctx.Equipos.AddRange(e1, e2);
        await ctx.SaveChangesAsync();

        var fecha = new FechaTodosContraTodos
        {
            Id = 0,
            Dia = new DateOnly(2026, 2, 1),
            Numero = 1,
            ZonaId = zona.Id,
            EsVisibleEnApp = true
        };
        ctx.Fechas.Add(fecha);
        await ctx.SaveChangesAsync();

        var jornada = new JornadaNormal
        {
            Id = 0,
            FechaId = fecha.Id,
            ResultadosVerificados = false,
            LocalEquipoId = e1.Id,
            VisitanteEquipoId = e2.Id
        };
        ctx.Jornadas.Add(jornada);
        await ctx.SaveChangesAsync();

        return (zona.Id, jornada.Id);
    }

    [Fact]
    public async Task CargarResultados_JornadaIdEnDtoDistintoDeRuta_Lanza()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var ctx = new AppDbContext(options);
        var core = CrearCore(ctx);
        var (_, jornadaId, _, _, _, _) = await SeedJornadaConDosPartidos(ctx);

        var ex = await Assert.ThrowsAsync<ExcepcionControlada>(() => core.CargarResultados(1, jornadaId,
            new CargarResultadosDTO
            {
                JornadaId = jornadaId + 999,
                ResultadosVerificados = true,
                Partidos = []
            }));

        Assert.Contains("no coincide", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CargarResultados_JornadaInexistente_Lanza()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var ctx = new AppDbContext(options);
        var core = CrearCore(ctx);

        var ex = await Assert.ThrowsAsync<ExcepcionControlada>(() => core.CargarResultados(1, 99999,
            new CargarResultadosDTO { JornadaId = 99999, ResultadosVerificados = false, Partidos = [] }));

        Assert.Contains("no existe", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CargarResultados_JornadaDeOtraZona_Lanza()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var ctx = new AppDbContext(options);
        var core = CrearCore(ctx);
        var (zonaId, jornadaId, _, _, _, _) = await SeedJornadaConDosPartidos(ctx);

        var ex = await Assert.ThrowsAsync<ExcepcionControlada>(() => core.CargarResultados(zonaId + 99999, jornadaId,
            new CargarResultadosDTO { JornadaId = jornadaId, ResultadosVerificados = false, Partidos = [] }));

        Assert.Contains("no pertenece", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CargarResultados_SinPartidosEnBdNiEnDto_GuardaSoloFlagVerificados()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var ctx = new AppDbContext(options);
        var core = CrearCore(ctx);
        var (zonaId, jornadaId) = await SeedJornadaSinPartidos(ctx);

        await core.CargarResultados(zonaId, jornadaId,
            new CargarResultadosDTO { JornadaId = jornadaId, ResultadosVerificados = true, Partidos = [] });

        var j = await ctx.Jornadas.FirstAsync(x => x.Id == jornadaId);
        Assert.True(j.ResultadosVerificados);
    }

    [Fact]
    public async Task CargarResultados_CantidadPartidosDistinta_Lanza()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var ctx = new AppDbContext(options);
        var core = CrearCore(ctx);
        var (zonaId, jornadaId, p1Id, _, cat1Id, _) = await SeedJornadaConDosPartidos(ctx);

        var ex = await Assert.ThrowsAsync<ExcepcionControlada>(() => core.CargarResultados(zonaId, jornadaId,
            new CargarResultadosDTO
            {
                JornadaId = jornadaId,
                ResultadosVerificados = false,
                Partidos =
                [
                    new PartidoDTO
                    {
                        Id = p1Id,
                        CategoriaId = cat1Id,
                        ResultadoLocal = "1",
                        ResultadoVisitante = "0"
                    }
                ]
            }));

        Assert.Contains("exactamente un partido", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CargarResultados_PartidoConIdInvalido_Lanza()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var ctx = new AppDbContext(options);
        var core = CrearCore(ctx);
        var (zonaId, jornadaId, _, p2Id, cat1Id, cat2Id) = await SeedJornadaConDosPartidos(ctx);

        var ex = await Assert.ThrowsAsync<ExcepcionControlada>(() => core.CargarResultados(zonaId, jornadaId,
            new CargarResultadosDTO
            {
                JornadaId = jornadaId,
                ResultadosVerificados = false,
                Partidos =
                [
                    new PartidoDTO
                    {
                        Id = 0,
                        CategoriaId = cat1Id,
                        ResultadoLocal = "1",
                        ResultadoVisitante = "0"
                    },
                    new PartidoDTO
                    {
                        Id = p2Id,
                        CategoriaId = cat2Id,
                        ResultadoLocal = "2",
                        ResultadoVisitante = "1"
                    }
                ]
            }));

        Assert.Contains("identificador", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CargarResultados_PartidosDuplicadosEnRequest_Lanza()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var ctx = new AppDbContext(options);
        var core = CrearCore(ctx);
        var (zonaId, jornadaId, p1Id, _, cat1Id, cat2Id) = await SeedJornadaConDosPartidos(ctx);

        var ex = await Assert.ThrowsAsync<ExcepcionControlada>(() => core.CargarResultados(zonaId, jornadaId,
            new CargarResultadosDTO
            {
                JornadaId = jornadaId,
                ResultadosVerificados = false,
                Partidos =
                [
                    new PartidoDTO
                    {
                        Id = p1Id,
                        CategoriaId = cat1Id,
                        ResultadoLocal = "1",
                        ResultadoVisitante = "0"
                    },
                    new PartidoDTO
                    {
                        Id = p1Id,
                        CategoriaId = cat2Id,
                        ResultadoLocal = "2",
                        ResultadoVisitante = "1"
                    }
                ]
            }));

        Assert.Contains("duplicados", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CargarResultados_PartidoQueNoEsDeLaJornada_Lanza()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var ctx = new AppDbContext(options);
        var core = CrearCore(ctx);
        var (zonaId, j1, p1a, _, cat1, cat2) = await SeedJornadaConDosPartidos(ctx);
        var (_, j2, p2a, p2b, cat1b, cat2b) = await SeedJornadaConDosPartidos(ctx);
        Assert.NotEqual(j1, j2);

        var ex = await Assert.ThrowsAsync<ExcepcionControlada>(() => core.CargarResultados(zonaId, j1,
            new CargarResultadosDTO
            {
                JornadaId = j1,
                ResultadosVerificados = false,
                Partidos =
                [
                    new PartidoDTO
                    {
                        Id = p1a,
                        CategoriaId = cat1,
                        ResultadoLocal = "1",
                        ResultadoVisitante = "0"
                    },
                    new PartidoDTO
                    {
                        Id = p2b,
                        CategoriaId = cat2b,
                        ResultadoLocal = "2",
                        ResultadoVisitante = "1"
                    }
                ]
            }));

        Assert.Contains("no pertenece", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CargarResultados_CategoriaIdDistintaAlRegistro_Lanza()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var ctx = new AppDbContext(options);
        var core = CrearCore(ctx);
        var (zonaId, jornadaId, p1Id, p2Id, cat1Id, cat2Id) = await SeedJornadaConDosPartidos(ctx);

        var ex = await Assert.ThrowsAsync<ExcepcionControlada>(() => core.CargarResultados(zonaId, jornadaId,
            new CargarResultadosDTO
            {
                JornadaId = jornadaId,
                ResultadosVerificados = false,
                Partidos =
                [
                    new PartidoDTO
                    {
                        Id = p1Id,
                        CategoriaId = cat2Id,
                        ResultadoLocal = "1",
                        ResultadoVisitante = "0"
                    },
                    new PartidoDTO
                    {
                        Id = p2Id,
                        CategoriaId = cat1Id,
                        ResultadoLocal = "2",
                        ResultadoVisitante = "1"
                    }
                ]
            }));

        Assert.Contains("categoría", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CargarResultados_ResultadoVacio_Lanza()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var ctx = new AppDbContext(options);
        var core = CrearCore(ctx);
        var (zonaId, jornadaId, p1Id, p2Id, cat1Id, cat2Id) = await SeedJornadaConDosPartidos(ctx);

        var ex = await Assert.ThrowsAsync<ExcepcionControlada>(() => core.CargarResultados(zonaId, jornadaId,
            new CargarResultadosDTO
            {
                JornadaId = jornadaId,
                ResultadosVerificados = false,
                Partidos =
                [
                    new PartidoDTO
                    {
                        Id = p1Id,
                        CategoriaId = cat1Id,
                        ResultadoLocal = "",
                        ResultadoVisitante = "0"
                    },
                    new PartidoDTO
                    {
                        Id = p2Id,
                        CategoriaId = cat2Id,
                        ResultadoLocal = "2",
                        ResultadoVisitante = "1"
                    }
                ]
            }));

        Assert.Contains("vacío", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CargarResultados_ResultadoInvalido_Lanza()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var ctx = new AppDbContext(options);
        var core = CrearCore(ctx);
        var (zonaId, jornadaId, p1Id, p2Id, cat1Id, cat2Id) = await SeedJornadaConDosPartidos(ctx);

        var ex = await Assert.ThrowsAsync<ExcepcionControlada>(() => core.CargarResultados(zonaId, jornadaId,
            new CargarResultadosDTO
            {
                JornadaId = jornadaId,
                ResultadosVerificados = false,
                Partidos =
                [
                    new PartidoDTO
                    {
                        Id = p1Id,
                        CategoriaId = cat1Id,
                        ResultadoLocal = "XYZ",
                        ResultadoVisitante = "0"
                    },
                    new PartidoDTO
                    {
                        Id = p2Id,
                        CategoriaId = cat2Id,
                        ResultadoLocal = "2",
                        ResultadoVisitante = "1"
                    }
                ]
            }));

        Assert.Contains("no es válido", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CargarResultados_SyPDebenSerIgualesEnAmbos_Lanza()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var ctx = new AppDbContext(options);
        var core = CrearCore(ctx);
        var (zonaId, jornadaId, p1Id, p2Id, cat1Id, cat2Id) = await SeedJornadaConDosPartidos(ctx);

        var ex = await Assert.ThrowsAsync<ExcepcionControlada>(() => core.CargarResultados(zonaId, jornadaId,
            new CargarResultadosDTO
            {
                JornadaId = jornadaId,
                ResultadosVerificados = false,
                Partidos =
                [
                    new PartidoDTO
                    {
                        Id = p1Id,
                        CategoriaId = cat1Id,
                        ResultadoLocal = "S",
                        ResultadoVisitante = "0"
                    },
                    new PartidoDTO
                    {
                        Id = p2Id,
                        CategoriaId = cat2Id,
                        ResultadoLocal = "2",
                        ResultadoVisitante = "1"
                    }
                ]
            }));

        Assert.Contains("S o P", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CargarResultados_Exitoso_ActualizaPartidosYFlagJornada()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var ctx = new AppDbContext(options);
        var core = CrearCore(ctx);
        var (zonaId, jornadaId, p1Id, p2Id, cat1Id, cat2Id) = await SeedJornadaConDosPartidos(ctx);

        await core.CargarResultados(zonaId, jornadaId,
            new CargarResultadosDTO
            {
                JornadaId = jornadaId,
                ResultadosVerificados = true,
                Partidos =
                [
                    new PartidoDTO
                    {
                        Id = p1Id,
                        CategoriaId = cat1Id,
                        ResultadoLocal = " 3 ",
                        ResultadoVisitante = "GP"
                    },
                    new PartidoDTO
                    {
                        Id = p2Id,
                        CategoriaId = cat2Id,
                        ResultadoLocal = "S",
                        ResultadoVisitante = "S"
                    }
                ]
            });

        var j = await ctx.Jornadas.FirstAsync(x => x.Id == jornadaId);
        Assert.True(j.ResultadosVerificados);

        var db1 = await ctx.Partidos.FirstAsync(p => p.Id == p1Id);
        Assert.Equal("3", db1.ResultadoLocal);
        Assert.Equal("GP", db1.ResultadoVisitante);

        var db2 = await ctx.Partidos.FirstAsync(p => p.Id == p2Id);
        Assert.Equal("S", db2.ResultadoLocal);
        Assert.Equal("S", db2.ResultadoVisitante);
    }
}
