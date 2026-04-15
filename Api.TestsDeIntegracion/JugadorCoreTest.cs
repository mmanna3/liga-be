using Api.Core.DTOs;
using Api.Core.DTOs.CambiosDeEstadoJugador;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Repositorios;
using Api.Core.Servicios;
using Api.Persistencia.Repositorios;
using Microsoft.AspNetCore.Hosting;
using Moq;
using AutoMapper;

namespace Api.TestsDeIntegracion
{
    public class JugadorCoreTest
    {
        private readonly Mock<IBDVirtual> _bdVirtualMock;
        private readonly Mock<IJugadorRepo> _jugadorRepoMock;
        private readonly Mock<IEquipoRepo> _equipoRepoMock;
        private readonly Mock<IImagenJugadorRepo> _imagenJugadorRepoMock;
        private readonly Mock<IHistorialDePagosRepo> _historialDePagosRepoMock;
        private readonly Mock<IDelegadoRepo> _delegadoRepoMock;
        private readonly Mock<IDniExpulsadoDeLaLigaRepo> _dniExpulsadoDeLaLigaRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AppPathsForTest _paths;
        private readonly JugadorCore _jugadorCore;

        public JugadorCoreTest()
        {
            var env = new Mock<IWebHostEnvironment>().Object;
            _paths = new AppPathsForTest(env);
            
            _bdVirtualMock = new Mock<IBDVirtual>();
            _jugadorRepoMock = new Mock<IJugadorRepo>();
            _equipoRepoMock = new Mock<IEquipoRepo>();
            _imagenJugadorRepoMock = new Mock<IImagenJugadorRepo>();
            _historialDePagosRepoMock = new Mock<IHistorialDePagosRepo>();
            _delegadoRepoMock = new Mock<IDelegadoRepo>();
            _dniExpulsadoDeLaLigaRepoMock = new Mock<IDniExpulsadoDeLaLigaRepo>();
            _dniExpulsadoDeLaLigaRepoMock
                .Setup(x => x.ExistePorDniAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mapperMock = new Mock<IMapper>();

            _jugadorCore = new JugadorCore(
                _bdVirtualMock.Object,
                _jugadorRepoMock.Object,
                _mapperMock.Object,
                _equipoRepoMock.Object,
                _imagenJugadorRepoMock.Object,
                _paths,
                _historialDePagosRepoMock.Object,
                _delegadoRepoMock.Object,
                _dniExpulsadoDeLaLigaRepoMock.Object
            );
        }

        [Fact]
        public async Task Aprobar_ConCambioDeDNI_RenombraFotosTemporalesAntesDeFichar()
        {
            // Arrange
            const string dniAnterior = "11111111";
            const string dniNuevo = "22222222";

            var jugador = new Jugador
            {
                Id = 1,
                DNI = dniAnterior,
                Nombre = "Test",
                Apellido = "Jugador",
                FechaNacimiento = new DateTime(1990, 1, 1),
                JugadorEquipos = new List<JugadorEquipo> { new() { Id = 10, EquipoId = 5 } }
            };

            _jugadorRepoMock.Setup(x => x.ObtenerPorId(1)).ReturnsAsync(jugador);

            var dto = new AprobarJugadorDTO
            {
                Id = 1,
                DNI = dniNuevo,
                Nombre = "Test",
                Apellido = "Jugador",
                FechaNacimiento = new DateTime(1990, 1, 1),
                JugadorEquipoId = 10
            };

            // Act
            await _jugadorCore.Aprobar(dto);

            // Assert: el rename debe ocurrir para que FicharJugadorTemporal encuentre el archivo con el nuevo DNI
            _imagenJugadorRepoMock.Verify(
                x => x.RenombrarFotosTemporalesPorCambioDeDNI(dniAnterior, dniNuevo),
                Times.Once);
        }

        [Fact]
        public async Task Aprobar_SinCambioDeDNI_NoRenombraFotosTemporales()
        {
            // Arrange
            const string dni = "11111111";

            var jugador = new Jugador
            {
                Id = 1,
                DNI = dni,
                Nombre = "Test",
                Apellido = "Jugador",
                FechaNacimiento = new DateTime(1990, 1, 1),
                JugadorEquipos = new List<JugadorEquipo> { new() { Id = 10, EquipoId = 5 } }
            };

            _jugadorRepoMock.Setup(x => x.ObtenerPorId(1)).ReturnsAsync(jugador);

            var dto = new AprobarJugadorDTO
            {
                Id = 1,
                DNI = dni, // mismo DNI
                Nombre = "Test",
                Apellido = "Jugador",
                FechaNacimiento = new DateTime(1990, 1, 1),
                JugadorEquipoId = 10
            };

            // Act
            await _jugadorCore.Aprobar(dto);

            // Assert
            _imagenJugadorRepoMock.Verify(
                x => x.RenombrarFotosTemporalesPorCambioDeDNI(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Aprobar_ConCambioDeDNI_LlamaFicharJugadorTemporalConNuevoDNI()
        {
            // Arrange
            const string dniAnterior = "11111111";
            const string dniNuevo = "22222222";

            var jugador = new Jugador
            {
                Id = 1,
                DNI = dniAnterior,
                Nombre = "Test",
                Apellido = "Jugador",
                FechaNacimiento = new DateTime(1990, 1, 1),
                JugadorEquipos = new List<JugadorEquipo> { new() { Id = 10, EquipoId = 5 } }
            };

            _jugadorRepoMock.Setup(x => x.ObtenerPorId(1)).ReturnsAsync(jugador);

            var dto = new AprobarJugadorDTO
            {
                Id = 1,
                DNI = dniNuevo,
                Nombre = "Test",
                Apellido = "Jugador",
                FechaNacimiento = new DateTime(1990, 1, 1),
                JugadorEquipoId = 10
            };

            // Act
            await _jugadorCore.Aprobar(dto);

            // Assert: debe fichar con el DNI nuevo (no el anterior), porque las fotos ya fueron renombradas
            _imagenJugadorRepoMock.Verify(
                x => x.FicharJugadorTemporal(dniNuevo),
                Times.Once);
            _imagenJugadorRepoMock.Verify(
                x => x.FicharJugadorTemporal(dniAnterior),
                Times.Never);
        }

        [Fact]
        public async Task Aprobar_PrimerEquipo_LlamaFicharJugadorTemporal()
        {
            // Arrange
            const string dni = "33333333";

            var jugador = new Jugador
            {
                Id = 1,
                DNI = dni,
                Nombre = "Test",
                Apellido = "Jugador",
                FechaNacimiento = new DateTime(1990, 1, 1),
                JugadorEquipos = new List<JugadorEquipo> { new() { Id = 10, EquipoId = 5 } } // un solo equipo
            };

            _jugadorRepoMock.Setup(x => x.ObtenerPorId(1)).ReturnsAsync(jugador);

            var dto = new AprobarJugadorDTO
            {
                Id = 1,
                DNI = dni,
                Nombre = "Test",
                Apellido = "Jugador",
                FechaNacimiento = new DateTime(1990, 1, 1),
                JugadorEquipoId = 10
            };

            // Act
            await _jugadorCore.Aprobar(dto);

            // Assert
            _imagenJugadorRepoMock.Verify(x => x.FicharJugadorTemporal(dni), Times.Once);
        }

        [Fact]
        public async Task Aprobar_SegundoEquipo_NoLlamaFicharJugadorTemporal()
        {
            // Arrange: jugador ya fichado en otro equipo (foto ya está en definitivas)
            const string dni = "44444444";

            var jugador = new Jugador
            {
                Id = 1,
                DNI = dni,
                Nombre = "Test",
                Apellido = "Jugador",
                FechaNacimiento = new DateTime(1990, 1, 1),
                JugadorEquipos = new List<JugadorEquipo>
                {
                    new() { Id = 10, EquipoId = 5 },
                    new() { Id = 11, EquipoId = 6 } // dos equipos = no es el primero
                }
            };

            _jugadorRepoMock.Setup(x => x.ObtenerPorId(1)).ReturnsAsync(jugador);

            var dto = new AprobarJugadorDTO
            {
                Id = 1,
                DNI = dni,
                Nombre = "Test",
                Apellido = "Jugador",
                FechaNacimiento = new DateTime(1990, 1, 1),
                JugadorEquipoId = 11
            };

            // Act
            await _jugadorCore.Aprobar(dto);

            // Assert: la foto ya está en definitivas, no hay que moverla
            _imagenJugadorRepoMock.Verify(
                x => x.FicharJugadorTemporal(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Eliminar_WhenJugadorExists_DeletesJugadorAndImages()
        {
            // Arrange
            var jugadorId = 1;
            var jugador = new Jugador
            {
                Id = jugadorId,
                DNI = "12345678",
                Nombre = "Test",
                Apellido = "Player",
                FechaNacimiento = DateTime.Now
            };

            _jugadorRepoMock.Setup(x => x.ObtenerPorIdParaEliminar(jugadorId))
                .ReturnsAsync(jugador);

            // Act
            var resultado = await _jugadorCore.Eliminar(jugadorId);

            // Assert
            Assert.Equal(jugadorId, resultado);
            _jugadorRepoMock.Verify(x => x.Eliminar(jugador), Times.Once);
            _imagenJugadorRepoMock.Verify(x => x.EliminarFotosDelJugador(jugador.DNI), Times.Once);
            _bdVirtualMock.Verify(x => x.GuardarCambios(), Times.Once);
        }

        [Fact]
        public async Task Eliminar_WhenJugadorDoesNotExist_ReturnsNegativeOne()
        {
            // Arrange
            var jugadorId = 1;
            _jugadorRepoMock.Setup(x => x.ObtenerPorIdParaEliminar(jugadorId))
                .ReturnsAsync((Jugador?)null);

            // Act
            var resultado = await _jugadorCore.Eliminar(jugadorId);

            // Assert
            Assert.Equal(-1, resultado);
            _jugadorRepoMock.Verify(x => x.Eliminar(It.IsAny<Jugador>()), Times.Never);
            _imagenJugadorRepoMock.Verify(x => x.EliminarFotosDelJugador(It.IsAny<string>()), Times.Never);
            _bdVirtualMock.Verify(x => x.GuardarCambios(), Times.Never);
        }
    }
} 