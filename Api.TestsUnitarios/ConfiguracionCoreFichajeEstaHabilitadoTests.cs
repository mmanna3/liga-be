using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Repositorios;
using Api.Core.Servicios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;
using Moq;

namespace Api.TestsUnitarios;

public class ConfiguracionCoreFichajeEstaHabilitadoTests
{
    [Fact]
    public async Task Sin_configuracion_devuelve_false()
    {
        var bdMock = new Mock<IBDVirtual>();
        var repoMock = new Mock<IConfiguracionRepo>();
        var mapperMock = new Mock<IMapper>();
        var relojMock = new Mock<IRelojZonaHorariaArgentina>();

        repoMock.Setup(r => r.ObtenerPorId(1)).ReturnsAsync((Configuracion?)null);

        var core = new ConfiguracionCore(bdMock.Object, repoMock.Object, mapperMock.Object, relojMock.Object);

        Assert.False(await core.FichajeEstaHabilitado());
    }

    [Fact]
    public async Task Habilitado_siempre_true_sin_mirar_reloj()
    {
        var bdMock = new Mock<IBDVirtual>();
        var repoMock = new Mock<IConfiguracionRepo>();
        var mapperMock = new Mock<IMapper>();
        var relojMock = new Mock<IRelojZonaHorariaArgentina>();

        repoMock.Setup(r => r.ObtenerPorId(1)).ReturnsAsync(new Configuracion
        {
            Id = 1,
            HabilitacionFichajeId = (int)HabilitacionFichajeEnum.Habilitado
        });

        relojMock.Setup(r => r.AhoraLocal).Returns(new DateTime(2026, 4, 11, 10, 0, 0));

        var core = new ConfiguracionCore(bdMock.Object, repoMock.Object, mapperMock.Object, relojMock.Object);

        Assert.True(await core.FichajeEstaHabilitado());
    }

    [Fact]
    public async Task Deshabilitado_siempre_false()
    {
        var bdMock = new Mock<IBDVirtual>();
        var repoMock = new Mock<IConfiguracionRepo>();
        var mapperMock = new Mock<IMapper>();
        var relojMock = new Mock<IRelojZonaHorariaArgentina>();

        repoMock.Setup(r => r.ObtenerPorId(1)).ReturnsAsync(new Configuracion
        {
            Id = 1,
            HabilitacionFichajeId = (int)HabilitacionFichajeEnum.Deshabilitado
        });

        relojMock.Setup(r => r.AhoraLocal).Returns(new DateTime(2026, 4, 14, 12, 0, 0));

        var core = new ConfiguracionCore(bdMock.Object, repoMock.Object, mapperMock.Object, relojMock.Object);

        Assert.False(await core.FichajeEstaHabilitado());
    }

    [Fact]
    public async Task Programado_martes_al_mediodia_true()
    {
        var bdMock = new Mock<IBDVirtual>();
        var repoMock = new Mock<IConfiguracionRepo>();
        var mapperMock = new Mock<IMapper>();
        var relojMock = new Mock<IRelojZonaHorariaArgentina>();

        repoMock.Setup(r => r.ObtenerPorId(1)).ReturnsAsync(new Configuracion
        {
            Id = 1,
            HabilitacionFichajeId = (int)HabilitacionFichajeEnum.Programado
        });

        relojMock.Setup(r => r.AhoraLocal).Returns(new DateTime(2026, 4, 14, 12, 0, 0));

        var core = new ConfiguracionCore(bdMock.Object, repoMock.Object, mapperMock.Object, relojMock.Object);

        Assert.True(await core.FichajeEstaHabilitado());
    }

    [Fact]
    public async Task Programado_viernes_false_aunque_hubiera_sido_habilitado_en_otro_modo()
    {
        var bdMock = new Mock<IBDVirtual>();
        var repoMock = new Mock<IConfiguracionRepo>();
        var mapperMock = new Mock<IMapper>();
        var relojMock = new Mock<IRelojZonaHorariaArgentina>();

        repoMock.Setup(r => r.ObtenerPorId(1)).ReturnsAsync(new Configuracion
        {
            Id = 1,
            HabilitacionFichajeId = (int)HabilitacionFichajeEnum.Programado
        });

        relojMock.Setup(r => r.AhoraLocal).Returns(new DateTime(2026, 4, 17, 9, 0, 0));

        var core = new ConfiguracionCore(bdMock.Object, repoMock.Object, mapperMock.Object, relojMock.Object);

        Assert.False(await core.FichajeEstaHabilitado());
    }

    [Fact]
    public async Task Id_desconocido_devuelve_false()
    {
        var bdMock = new Mock<IBDVirtual>();
        var repoMock = new Mock<IConfiguracionRepo>();
        var mapperMock = new Mock<IMapper>();
        var relojMock = new Mock<IRelojZonaHorariaArgentina>();

        repoMock.Setup(r => r.ObtenerPorId(1)).ReturnsAsync(new Configuracion
        {
            Id = 1,
            HabilitacionFichajeId = 999
        });

        var core = new ConfiguracionCore(bdMock.Object, repoMock.Object, mapperMock.Object, relojMock.Object);

        Assert.False(await core.FichajeEstaHabilitado());
    }
}
