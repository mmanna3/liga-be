using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Logica;
using Api.Core.Repositorios;
using Api.Core.Servicios;
using Moq;

namespace Api.TestsUnitarios;

public class EquipoCoreTests
{
    [Fact]
    public async Task ObtenerClubPorCodigoAlfanumerico_CodigoValido_RetornaNombreClub()
    {
        var bdMock = new Mock<IBDVirtual>();
        var repoMock = new Mock<IEquipoRepo>();
        var mapperMock = new Mock<AutoMapper.IMapper>();

        var club = new Club { Id = 1, Nombre = "Club de Prueba" };
        var equipo = new Equipo
        {
            Id = 1,
            Nombre = "Equipo Test",
            ClubId = 1,
            Club = club,
            Jugadores = []
        };

        repoMock.Setup(r => r.ObtenerPorId(1)).ReturnsAsync(equipo);

        var core = new EquipoCore(bdMock.Object, repoMock.Object, mapperMock.Object);
        var codigo = GeneradorDeHash.GenerarAlfanumerico7Digitos(1);

        var result = await core.ObtenerClubPorCodigoAlfanumericoDelEquipo(codigo);

        Assert.False(result.HayError);
        Assert.Equal("Club de Prueba", result.Respuesta);
    }

    [Fact]
    public async Task ObtenerClubPorCodigoAlfanumerico_CodigoInvalido_RetornaHayError()
    {
        var bdMock = new Mock<IBDVirtual>();
        var repoMock = new Mock<IEquipoRepo>();
        var mapperMock = new Mock<AutoMapper.IMapper>();

        var core = new EquipoCore(bdMock.Object, repoMock.Object, mapperMock.Object);

        var result = await core.ObtenerClubPorCodigoAlfanumericoDelEquipo("XXX0001");

        Assert.True(result.HayError);
    }

    [Fact]
    public async Task ObtenerClubPorCodigoAlfanumerico_EquipoNoExiste_RetornaHayError()
    {
        var bdMock = new Mock<IBDVirtual>();
        var repoMock = new Mock<IEquipoRepo>();
        var mapperMock = new Mock<AutoMapper.IMapper>();

        repoMock.Setup(r => r.ObtenerPorId(9999)).ReturnsAsync((Equipo?)null);

        var core = new EquipoCore(bdMock.Object, repoMock.Object, mapperMock.Object);
        var codigo = GeneradorDeHash.GenerarAlfanumerico7Digitos(9999);

        var result = await core.ObtenerClubPorCodigoAlfanumericoDelEquipo(codigo);

        Assert.True(result.HayError);
        Assert.Contains("no pertenece a ningún equipo", result.MensajeError ?? "");
    }
}
