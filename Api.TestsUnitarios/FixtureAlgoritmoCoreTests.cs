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
            Fechas =
            [
                new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 0, EquipoVisitante = 1 },
                new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 2, EquipoVisitante = 3 },
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 1, EquipoVisitante = 2 },
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 3, EquipoVisitante = 0 },
                new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 2, EquipoVisitante = 0 }
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
            Fechas =
            [
                new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 0, EquipoVisitante = 1 },
                new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 2, EquipoVisitante = 3 },
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 1, EquipoVisitante = 2 },
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 3, EquipoVisitante = 0 },
                new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 2, EquipoVisitante = 0 },
                new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 1, EquipoVisitante = 3 },
                new FixtureAlgoritmoFechaDTO { Fecha = 4, EquipoLocal = 0, EquipoVisitante = 2 } // 7ma fecha de más
            ]
        };

        var core = new FixtureAlgoritmoCore(_bdMock.Object, _repoMock.Object, _mapperMock.Object);

        var ex = await Assert.ThrowsAsync<ExcepcionControlada>(() => core.Crear(dto));

        Assert.Contains("6", ex.Message);
        Assert.Contains("7", ex.Message);
        _repoMock.Verify(r => r.Crear(It.IsAny<FixtureAlgoritmo>()), Times.Never);
    }
}
