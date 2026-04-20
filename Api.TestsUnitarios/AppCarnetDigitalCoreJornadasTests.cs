using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Logica;
using Api.Core.Repositorios;
using Api.Core.Servicios;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace Api.TestsUnitarios;

public class AppCarnetDigitalCoreJornadasTests
{
    [Fact]
    public async Task JornadasTodosContraTodosAsync_JornadaNormal_CalculaPuntosTotalesYPartidosJugados_ExcluyendoNP()
    {
        var catA = new TorneoCategoria { Id = 1, Nombre = "Sub 15", AnioDesde = 2009, AnioHasta = 2010, TorneoId = 1 };
        var catB = new TorneoCategoria { Id = 2, Nombre = "Sub 17", AnioDesde = 2007, AnioHasta = 2008, TorneoId = 1 };

        var local = CrearEquipo(10, "Local FC", 1);
        var visitante = CrearEquipo(20, "Visitante FC", 2);

        var jornada = new JornadaNormal
        {
            Id = 100,
            FechaId = 1,
            ResultadosVerificados = true,
            LocalEquipoId = local.Id,
            LocalEquipo = local,
            VisitanteEquipoId = visitante.Id,
            VisitanteEquipo = visitante,
            Partidos =
            [
                new Partido
                {
                    Id = 1,
                    JornadaId = 100,
                    Jornada = null!,
                    CategoriaId = catA.Id,
                    Categoria = catA,
                    ResultadoLocal = "2",
                    ResultadoVisitante = "1"
                },
                new Partido
                {
                    Id = 2,
                    JornadaId = 100,
                    Jornada = null!,
                    CategoriaId = catB.Id,
                    Categoria = catB,
                    ResultadoLocal = "NP",
                    ResultadoVisitante = "3"
                }
            ]
        };
        foreach (var partido in jornada.Partidos)
            partido.Jornada = jornada;

        var fecha = CrearFechaTodosContraTodos(1, jornada, catA, catB);
        var core = CrearCoreConFechas([fecha]);

        var dto = await core.JornadasTodosContraTodosAsync(77);
        var partidoDto = dto.Fechas.Single().Jornadas.Single();

        Assert.Equal(3, partidoDto.Local.PuntosTotales);
        Assert.Equal(1, partidoDto.Local.PartidosJugados);
        Assert.Equal(4, partidoDto.Visitante.PuntosTotales);
        Assert.Equal(2, partidoDto.Visitante.PartidosJugados);

        Assert.Equal(2, partidoDto.Local.Categorias.Count);
        Assert.Equal("2 - 1", partidoDto.Local.Categorias.First(c => c.Categoria == "Sub 15").Resultado);
        Assert.Equal("NP - 3", partidoDto.Local.Categorias.First(c => c.Categoria == "Sub 17").Resultado);
    }

    [Fact]
    public async Task JornadasTodosContraTodosAsync_JornadaLibre_GP_PP_YCategoriaSinResultados_RespetaTotalesYFormato()
    {
        var catA = new TorneoCategoria { Id = 1, Nombre = "Primera", AnioDesde = 2000, AnioHasta = 2005, TorneoId = 1 };
        var catB = new TorneoCategoria { Id = 2, Nombre = "Reserva", AnioDesde = 2006, AnioHasta = 2010, TorneoId = 1 };
        var local = CrearEquipo(10, "Local FC", 1);

        var jornada = new JornadaLibre
        {
            Id = 101,
            FechaId = 1,
            ResultadosVerificados = true,
            EquipoLocalId = local.Id,
            EquipoLocal = local,
            Partidos =
            [
                new Partido
                {
                    Id = 3,
                    JornadaId = 101,
                    Jornada = null!,
                    CategoriaId = catA.Id,
                    Categoria = catA,
                    ResultadoLocal = "GP",
                    ResultadoVisitante = "PP"
                },
                new Partido
                {
                    Id = 4,
                    JornadaId = 101,
                    Jornada = null!,
                    CategoriaId = catB.Id,
                    Categoria = catB,
                    ResultadoLocal = "",
                    ResultadoVisitante = ""
                }
            ]
        };
        foreach (var partido in jornada.Partidos)
            partido.Jornada = jornada;

        var fecha = CrearFechaTodosContraTodos(2, jornada, catA, catB);
        var core = CrearCoreConFechas([fecha]);

        var dto = await core.JornadasTodosContraTodosAsync(77);
        var partidoDto = dto.Fechas.Single().Jornadas.Single();

        Assert.Equal(3, partidoDto.Local.PuntosTotales);
        Assert.Equal(1, partidoDto.Local.PartidosJugados);
        Assert.Equal(0, partidoDto.Visitante.PuntosTotales);
        Assert.Equal(0, partidoDto.Visitante.PartidosJugados);
        Assert.Equal("LIBRE", partidoDto.Visitante.Equipo);

        Assert.Equal("GP - PP", partidoDto.Local.Categorias.First(c => c.Categoria == "Primera").Resultado);
        Assert.Equal(string.Empty, partidoDto.Local.Categorias.First(c => c.Categoria == "Reserva").Resultado);
    }

    private static AppCarnetDigitalCore CrearCoreConFechas(IReadOnlyList<FechaTodosContraTodos> fechas)
    {
        var delegadoRepo = new Mock<IDelegadoRepo>();
        var equipoRepo = new Mock<IEquipoRepo>();
        var mapper = new Mock<IMapper>();
        var imagenJugadorRepo = new Mock<IImagenJugadorRepo>();
        var imagenDelegadoRepo = new Mock<IImagenDelegadoRepo>();
        var torneoAgrupadorRepo = new Mock<ITorneoAgrupadorRepo>();
        var fechaRepo = new Mock<IFechaRepo>();
        var torneoRepo = new Mock<ITorneoRepo>();
        var leyendaRepo = new Mock<ILeyendaTablaPosicionesRepo>();
        var imagenEscudoRepo = new Mock<IImagenEscudoRepo>();

        fechaRepo
            .Setup(r => r.ListarTodosContraTodosPorZonaParaAppConPartidosAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fechas);

        imagenEscudoRepo
            .Setup(r => r.GetRutaRelativaEscudo(It.IsAny<int>()))
            .Returns<int>(clubId => $"/Imagenes/Escudos/{clubId}.jpg");

        var env = new Mock<IWebHostEnvironment>();
        env.SetupGet(x => x.ContentRootPath).Returns("/tmp");
        var paths = new AppPathsWebApp(env.Object);

        return new AppCarnetDigitalCore(
            delegadoRepo.Object,
            equipoRepo.Object,
            mapper.Object,
            imagenJugadorRepo.Object,
            imagenDelegadoRepo.Object,
            torneoAgrupadorRepo.Object,
            fechaRepo.Object,
            torneoRepo.Object,
            leyendaRepo.Object,
            paths,
            imagenEscudoRepo.Object);
    }

    private static FechaTodosContraTodos CrearFechaTodosContraTodos(int numero, Jornada jornada,
        params TorneoCategoria[] categorias)
    {
        var torneo = new Torneo
        {
            Id = 1,
            Nombre = "Torneo test",
            Anio = DateTime.Today.Year,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true,
            TorneoAgrupadorId = 1,
            TorneoAgrupador = null!,
            Categorias = categorias.ToList(),
            Fases = []
        };
        foreach (var categoria in categorias)
            categoria.Torneo = torneo;

        var fase = new FaseTodosContraTodos
        {
            Id = 1,
            Nombre = "Apertura",
            Numero = 1,
            TorneoId = torneo.Id,
            Torneo = torneo,
            EstadoFaseId = 1,
            EstadoFase = null!,
            EsVisibleEnApp = true,
            Zonas = []
        };

        var zona = new ZonaTodosContraTodos
        {
            Id = 77,
            Nombre = "Zona A",
            FaseId = fase.Id,
            Fase = fase,
            EquiposZona = [],
            Fechas = [],
            LeyendasTablaPosiciones = []
        };

        var fecha = new FechaTodosContraTodos
        {
            Id = numero,
            Numero = numero,
            ZonaId = zona.Id,
            Zona = zona,
            EsVisibleEnApp = true,
            Dia = new DateOnly(2026, 4, 20),
            Jornadas = [jornada]
        };

        jornada.Fecha = fecha;
        return fecha;
    }

    private static Equipo CrearEquipo(int id, string nombre, int clubId) =>
        new()
        {
            Id = id,
            Nombre = nombre,
            ClubId = clubId,
            Club = new Club
            {
                Id = clubId,
                Nombre = $"Club {clubId}",
                Equipos = [],
                DelegadoClubs = [],
                CanchaTipo = null!
            },
            Jugadores = [],
            Zonas = []
        };
}
