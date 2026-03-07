using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Core.Servicios;
using AutoMapper;
using Moq;

namespace Api.TestsUnitarios;

public class ABMCoreObtenerPorIdsTests
{
    [Fact]
    public async Task ObtenerPorId_ConIds_DevuelveDtosMapeados()
    {
        var bdMock = new Mock<IBDVirtual>();
        var repoMock = new Mock<ITorneoRepo>();
        var mapperMock = new Mock<IMapper>();
        var equipoRepoMock = new Mock<IEquipoRepo>();

        var torneos = new List<Torneo>
        {
            new() { Id = 1, Nombre = "Torneo 1" },
            new() { Id = 2, Nombre = "Torneo 2" }
        };

        repoMock.Setup(r => r.ObtenerPorIds(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(torneos);

        var dto1 = new TorneoDTO { Id = 1, Nombre = "Torneo 1" };
        var dto2 = new TorneoDTO { Id = 2, Nombre = "Torneo 2" };
        mapperMock.Setup(m => m.Map<TorneoDTO>(torneos[0])).Returns(dto1);
        mapperMock.Setup(m => m.Map<TorneoDTO>(torneos[1])).Returns(dto2);

        var core = new TorneoCore(bdMock.Object, repoMock.Object, mapperMock.Object, equipoRepoMock.Object);

        var result = (await core.ObtenerPorId(new[] { 1, 2 })).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Torneo 1", result[0].Nombre);
        Assert.Equal(2, result[1].Id);
        Assert.Equal("Torneo 2", result[1].Nombre);
    }

    [Fact]
    public async Task ObtenerPorId_ConListaVacia_DevuelveListaVacia()
    {
        var bdMock = new Mock<IBDVirtual>();
        var repoMock = new Mock<ITorneoRepo>();
        var mapperMock = new Mock<IMapper>();
        var equipoRepoMock = new Mock<IEquipoRepo>();

        repoMock.Setup(r => r.ObtenerPorIds(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(Array.Empty<Torneo>());

        var core = new TorneoCore(bdMock.Object, repoMock.Object, mapperMock.Object, equipoRepoMock.Object);

        var result = await core.ObtenerPorId(Array.Empty<int>());

        Assert.Empty(result);
    }
}
