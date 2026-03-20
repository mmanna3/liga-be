using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios;
using AutoMapper;
using Moq;

namespace Api.TestsUnitarios;

public class FixtureAlgoritmoCoreTests
{
    private readonly Mock<IBDVirtual> _bdMock = new();
    private readonly Mock<IFixtureAlgoritmoRepo> _repoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    [Fact]
    public async Task Crear_ConN4YCantidadIncorrectaDeFechas_LanzaExcepcionControlada()
    {
        var dto = new FixtureAlgoritmoDTO
        {
            FixtureAlgoritmoId = 0,
            CantidadDeEquipos = 4,
            Nombre = "Test",
            Fechas =
            [
                new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 1, EquipoVisitante = 2 },
                new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 3, EquipoVisitante = 4 },
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 2, EquipoVisitante = 3 },
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 4, EquipoVisitante = 1 },
                new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 3, EquipoVisitante = 1 }
                // Solo 5 fechas, deberían ser 6
            ]
        };

        var core = new FixtureAlgoritmoCore(_bdMock.Object, _repoMock.Object, _mapperMock.Object);

        var ex = await Assert.ThrowsAsync<ExcepcionControlada>(() => core.Crear(dto));

        Assert.Contains("6", ex.Message);
        Assert.Contains("5", ex.Message);
        _repoMock.Verify(r => r.Crear(It.IsAny<FixtureAlgoritmo>()), Times.Never);
    }

    [Fact]
    public async Task Crear_ConN4YDemasiadasFechas_LanzaExcepcionControlada()
    {
        var dto = new FixtureAlgoritmoDTO
        {
            FixtureAlgoritmoId = 0,
            CantidadDeEquipos = 4,
            Nombre = "Test",
            Fechas =
            [
                new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 1, EquipoVisitante = 2 },
                new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 3, EquipoVisitante = 4 },
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 2, EquipoVisitante = 3 },
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 4, EquipoVisitante = 1 },
                new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 3, EquipoVisitante = 1 },
                new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 2, EquipoVisitante = 4 },
                new FixtureAlgoritmoFechaDTO { Fecha = 4, EquipoLocal = 1, EquipoVisitante = 3 } // 7ma fecha de más
            ]
        };

        var core = new FixtureAlgoritmoCore(_bdMock.Object, _repoMock.Object, _mapperMock.Object);

        var ex = await Assert.ThrowsAsync<ExcepcionControlada>(() => core.Crear(dto));

        Assert.Contains("6", ex.Message);
        Assert.Contains("7", ex.Message);
        _repoMock.Verify(r => r.Crear(It.IsAny<FixtureAlgoritmo>()), Times.Never);
    }

    private static FixtureAlgoritmoDTO DtoValidoN4() => new()
    {
        FixtureAlgoritmoId = 0,
        CantidadDeEquipos = 4,
        Nombre = "Test",
        Fechas =
        [
            new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 1, EquipoVisitante = 2 },
            new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 3, EquipoVisitante = 4 },
            new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 2, EquipoVisitante = 3 },
            new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 4, EquipoVisitante = 1 },
            new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 3, EquipoVisitante = 1 },
            new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 2, EquipoVisitante = 4 }
        ]
    };

    [Fact]
    public async Task Crear_ConEquiposUnaSolaVezPorFecha_NoLanza()
    {
        var dto = DtoValidoN4();
        var core = new FixtureAlgoritmoCore(_bdMock.Object, _repoMock.Object, _mapperMock.Object);

        await core.Crear(dto);

        _repoMock.Verify(r => r.Crear(It.IsAny<FixtureAlgoritmo>()), Times.Once);
    }

    [Fact]
    public async Task Crear_ConEquipoRepetidoEnMismaFecha_LanzaExcepcionControlada()
    {
        var dto = new FixtureAlgoritmoDTO
        {
            FixtureAlgoritmoId = 0,
            CantidadDeEquipos = 4,
            Nombre = "Test",
            Fechas =
            [
                new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 1, EquipoVisitante = 2 },
                new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 3, EquipoVisitante = 1 }, // 1 repetido en fecha 1
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 2, EquipoVisitante = 3 },
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 4, EquipoVisitante = 1 },
                new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 3, EquipoVisitante = 1 },
                new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 2, EquipoVisitante = 4 }
            ]
        };

        var core = new FixtureAlgoritmoCore(_bdMock.Object, _repoMock.Object, _mapperMock.Object);

        var ex = await Assert.ThrowsAsync<ExcepcionControlada>(() => core.Crear(dto));

        Assert.Contains("repetidos", ex.Message);
        Assert.Contains("fecha 1", ex.Message);
        _repoMock.Verify(r => r.Crear(It.IsAny<FixtureAlgoritmo>()), Times.Never);
    }

    [Fact]
    public async Task Crear_ConEquipoFaltanteEnUnaFecha_LanzaExcepcionControlada()
    {
        // 6 partidos total; en fecha 1 solo hay uno (1,2): faltan equipos 3 y 4 en esa fecha
        var dto = new FixtureAlgoritmoDTO
        {
            FixtureAlgoritmoId = 0,
            CantidadDeEquipos = 4,
            Nombre = "Test",
            Fechas =
            [
                new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 1, EquipoVisitante = 2 },
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 2, EquipoVisitante = 3 },
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 4, EquipoVisitante = 1 },
                new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 3, EquipoVisitante = 1 },
                new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 2, EquipoVisitante = 4 },
                new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 3, EquipoVisitante = 4 }
            ]
        };

        var core = new FixtureAlgoritmoCore(_bdMock.Object, _repoMock.Object, _mapperMock.Object);

        var ex = await Assert.ThrowsAsync<ExcepcionControlada>(() => core.Crear(dto));

        Assert.Contains("faltan", ex.Message);
        Assert.Contains("fecha 1", ex.Message);
        _repoMock.Verify(r => r.Crear(It.IsAny<FixtureAlgoritmo>()), Times.Never);
    }

    [Fact]
    public async Task Crear_ConEquipoFueraDeRango_LanzaExcepcionControlada()
    {
        var dto = new FixtureAlgoritmoDTO
        {
            FixtureAlgoritmoId = 0,
            CantidadDeEquipos = 4,
            Nombre = "Test",
            Fechas =
            [
                new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 1, EquipoVisitante = 2 },
                new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 3, EquipoVisitante = 5 }, // 5 fuera de rango (1-4)
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 2, EquipoVisitante = 3 },
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 4, EquipoVisitante = 1 },
                new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 3, EquipoVisitante = 1 },
                new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 2, EquipoVisitante = 4 }
            ]
        };

        var core = new FixtureAlgoritmoCore(_bdMock.Object, _repoMock.Object, _mapperMock.Object);

        var ex = await Assert.ThrowsAsync<ExcepcionControlada>(() => core.Crear(dto));

        Assert.Contains("fuera de rango", ex.Message);
        Assert.Contains("entre 1 y 4", ex.Message);
        Assert.Contains("fecha 1", ex.Message);
        _repoMock.Verify(r => r.Crear(It.IsAny<FixtureAlgoritmo>()), Times.Never);
    }

    [Fact]
    public async Task Crear_ConEncuentroRepetidoEntreEquipos_LanzaExcepcionControlada()
    {
        // Mismo par 1-2 en fecha 1 y fecha 3 (y 3-4 también repetido)
        var dto = new FixtureAlgoritmoDTO
        {
            FixtureAlgoritmoId = 0,
            CantidadDeEquipos = 4,
            Nombre = "Test",
            Fechas =
            [
                new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 1, EquipoVisitante = 2 },
                new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 3, EquipoVisitante = 4 },
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 1, EquipoVisitante = 3 },
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 2, EquipoVisitante = 4 },
                new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 1, EquipoVisitante = 2 },
                new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 3, EquipoVisitante = 4 }
            ]
        };

        var core = new FixtureAlgoritmoCore(_bdMock.Object, _repoMock.Object, _mapperMock.Object);

        var ex = await Assert.ThrowsAsync<ExcepcionControlada>(() => core.Crear(dto));

        Assert.Contains("repetido", ex.Message);
        Assert.Contains("equipo 1", ex.Message);
        Assert.Contains("equipo 2", ex.Message);
        _repoMock.Verify(r => r.Crear(It.IsAny<FixtureAlgoritmo>()), Times.Never);
    }
}
