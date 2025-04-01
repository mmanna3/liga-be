using Api.Core.Entidades;
using Api.Core.Enums;
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