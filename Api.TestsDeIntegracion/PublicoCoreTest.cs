using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios;
using Api.Core.Servicios.Interfaces;
using Moq;

namespace Api.TestsDeIntegracion;

public class PublicoCoreTest
{
    private readonly Mock<IJugadorRepo> _jugadorRepoMock;
    private readonly Mock<IJugadorCore> _jugadorCoreMock;
    private readonly Mock<IBDVirtual> _bdVirtualMock;
    private readonly PublicoCore _publicoCore;

    public PublicoCoreTest()
    {
        _jugadorRepoMock = new Mock<IJugadorRepo>();
        _jugadorCoreMock = new Mock<IJugadorCore>();
        _bdVirtualMock = new Mock<IBDVirtual>();
        _publicoCore = new PublicoCore(_jugadorRepoMock.Object, _jugadorCoreMock.Object, _bdVirtualMock.Object);
    }

    [Fact]
    public async Task ElDniEstaFichado_CuandoElJugadorNoExiste_RetornaFalse()
    {
        // Arrange
        const string dni = "12345678";
        _jugadorRepoMock.Setup(x => x.ObtenerPorDNI(dni)).ReturnsAsync((Jugador?)null);

        // Act
        var result = await _publicoCore.ElDniEstaFichado(dni);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ElDniEstaFichado_CuandoElJugadorExisteYEstaActivo_RetornaTrue()
    {
        // Arrange
        const string dni = "12345678";
        var jugador = new Jugador
        {
            Id = 1,
            DNI = dni,
            JugadorEquipos = new List<JugadorEquipo>
            {
                new() 
                { 
                    Id = 1,
                    EstadoJugadorId = (int)EstadoJugadorEnum.Activo,
                    EstadoJugador = new EstadoJugador { Id = (int)EstadoJugadorEnum.Activo }
                }
            }
        };
        _jugadorRepoMock.Setup(x => x.ObtenerPorDNI(dni)).ReturnsAsync(jugador);

        // Act
        var result = await _publicoCore.ElDniEstaFichado(dni);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ElDniEstaFichado_CuandoElJugadorExisteYEstaPendiente_RetornaFalse()
    {
        // Arrange
        const string dni = "12345678";
        var jugador = new Jugador
        {
            Id = 1,
            DNI = dni,
            JugadorEquipos = new List<JugadorEquipo>
            {
                new() 
                { 
                    Id = 1,
                    EstadoJugadorId = (int)EstadoJugadorEnum.FichajePendienteDeAprobacion,
                    EstadoJugador = new EstadoJugador { Id = (int)EstadoJugadorEnum.FichajePendienteDeAprobacion }
                }
            }
        };
        _jugadorRepoMock.Setup(x => x.ObtenerPorDNI(dni)).ReturnsAsync(jugador);

        // Act
        var result = await _publicoCore.ElDniEstaFichado(dni);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ElDniEstaFichado_CuandoElJugadorExisteYEstaRechazado_RetornaFalse()
    {
        // Arrange
        const string dni = "12345678";
        var jugador = new Jugador
        {
            Id = 1,
            DNI = dni,
            JugadorEquipos = new List<JugadorEquipo>
            {
                new() 
                { 
                    Id = 1,
                    EstadoJugadorId = (int)EstadoJugadorEnum.FichajeRechazado,
                    EstadoJugador = new EstadoJugador { Id = (int)EstadoJugadorEnum.FichajeRechazado }
                }
            }
        };
        _jugadorRepoMock.Setup(x => x.ObtenerPorDNI(dni)).ReturnsAsync(jugador);

        // Act
        var result = await _publicoCore.ElDniEstaFichado(dni);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ElDniEstaFichado_CuandoElJugadorExisteConMultiplesEquipos_RetornaFalseSiAlgunEquipoEstaPendienteORechazado()
    {
        // Arrange
        const string dni = "12345678";
        var jugador = new Jugador
        {
            Id = 1,
            DNI = dni,
            JugadorEquipos = new List<JugadorEquipo>
            {
                new() 
                { 
                    Id = 1,
                    EstadoJugadorId = (int)EstadoJugadorEnum.FichajePendienteDeAprobacion,
                    EstadoJugador = new EstadoJugador { Id = (int)EstadoJugadorEnum.FichajePendienteDeAprobacion }
                },
                new() 
                { 
                    Id = 2,
                    EstadoJugadorId = (int)EstadoJugadorEnum.Activo,
                    EstadoJugador = new EstadoJugador { Id = (int)EstadoJugadorEnum.Activo }
                },
                new() 
                { 
                    Id = 3,
                    EstadoJugadorId = (int)EstadoJugadorEnum.FichajeRechazado,
                    EstadoJugador = new EstadoJugador { Id = (int)EstadoJugadorEnum.FichajeRechazado }
                }
            }
        };
        _jugadorRepoMock.Setup(x => x.ObtenerPorDNI(dni)).ReturnsAsync(jugador);

        // Act
        var result = await _publicoCore.ElDniEstaFichado(dni);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task FicharEnOtroEquipo_JugadorNoExiste_LanzaExcepcionControlada()
    {
        // Arrange
        var dto = new FicharEnOtroEquipoDTO { DNI = "99999999", CodigoAlfanumerico = "ABC1234" };
        _jugadorRepoMock.Setup(x => x.ObtenerPorDNI(dto.DNI)).ReturnsAsync((Jugador?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ExcepcionControlada>(() => _publicoCore.FicharEnOtroEquipo(dto));
    }

    [Fact]
    public async Task FicharEnOtroEquipo_JugadorExiste_LlamaFicharYRetornaId()
    {
        // Arrange
        var jugador = new Jugador
        {
            Id = 5,
            DNI = "11111111",
            Nombre = "Juan",
            Apellido = "Perez",
            FechaNacimiento = new DateTime(2000, 1, 1),
            JugadorEquipos = new List<JugadorEquipo>()
        };
        var codigoAlfanumerico = Api.Core.Logica.GeneradorDeHash.GenerarAlfanumerico7Digitos(1);
        var dto = new FicharEnOtroEquipoDTO { DNI = jugador.DNI, CodigoAlfanumerico = codigoAlfanumerico };

        _jugadorRepoMock.Setup(x => x.ObtenerPorDNI(dto.DNI)).ReturnsAsync(jugador);
        _jugadorCoreMock.Setup(x => x.FicharJugadorEnElEquipo(1, jugador)).ReturnsAsync(jugador);
        _bdVirtualMock.Setup(x => x.GuardarCambios()).Returns(Task.CompletedTask);

        // Act
        var result = await _publicoCore.FicharEnOtroEquipo(dto);

        // Assert
        Assert.Equal(5, result);
        _jugadorCoreMock.Verify(x => x.FicharJugadorEnElEquipo(1, jugador), Times.Once);
    }

    [Fact]
    public async Task ElDniEstaFichado_CuandoElJugadorExisteConMultiplesEquipos_RetornaTrueSiTodosLosEquiposEstanActivos()
    {
        // Arrange
        const string dni = "12345678";
        var jugador = new Jugador
        {
            Id = 1,
            DNI = dni,
            JugadorEquipos = new List<JugadorEquipo>
            {
                new() 
                { 
                    Id = 1,
                    EstadoJugadorId = (int)EstadoJugadorEnum.Activo,
                    EstadoJugador = new EstadoJugador { Id = (int)EstadoJugadorEnum.Activo }
                },
                new() 
                { 
                    Id = 2,
                    EstadoJugadorId = (int)EstadoJugadorEnum.Activo,
                    EstadoJugador = new EstadoJugador { Id = (int)EstadoJugadorEnum.Activo }
                }
            }
        };
        _jugadorRepoMock.Setup(x => x.ObtenerPorDNI(dni)).ReturnsAsync(jugador);

        // Act
        var result = await _publicoCore.ElDniEstaFichado(dni);

        // Assert
        Assert.True(result);
    }
} 