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

        var torneos = new List<Torneo>
        {
            new() { Id = 1, Nombre = "Torneo 1", Anio = 2026 },
            new() { Id = 2, Nombre = "Torneo 2", Anio = 2026 }
        };

        repoMock.Setup(r => r.ObtenerPorIds(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(torneos);

        var dto1 = new TorneoDTO { Id = 1, Nombre = "Torneo 1", Anio = 2026 };
        var dto2 = new TorneoDTO { Id = 2, Nombre = "Torneo 2", Anio = 2026 };
        mapperMock.Setup(m => m.Map<TorneoDTO>(torneos[0])).Returns(dto1);
        mapperMock.Setup(m => m.Map<TorneoDTO>(torneos[1])).Returns(dto2);

        var torneoFaseRepoMock = new Mock<IFaseRepo>();
        var torneoCategoriaRepoMock = new Mock<ITorneoCategoriaRepo>();
        var torneoZonaRepoMock = new Mock<IZonaRepo>();
        var core = new TorneoCore(bdMock.Object, repoMock.Object, torneoFaseRepoMock.Object,
            torneoCategoriaRepoMock.Object, torneoZonaRepoMock.Object, mapperMock.Object);

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

        repoMock.Setup(r => r.ObtenerPorIds(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(Array.Empty<Torneo>());

        var torneoFaseRepoMock = new Mock<IFaseRepo>();
        var torneoCategoriaRepoMock = new Mock<ITorneoCategoriaRepo>();
        var torneoZonaRepoMock = new Mock<IZonaRepo>();
        var core = new TorneoCore(bdMock.Object, repoMock.Object, torneoFaseRepoMock.Object,
            torneoCategoriaRepoMock.Object, torneoZonaRepoMock.Object, mapperMock.Object);

        var result = await core.ObtenerPorId(Array.Empty<int>());

        Assert.Empty(result);
    }
}
