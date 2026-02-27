using Api.Core.DTOs;
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
            _mapperMock = new Mock<IMapper>();

            _jugadorCore = new JugadorCore(
                _bdVirtualMock.Object,
                _jugadorRepoMock.Object,
                _mapperMock.Object,
                _equipoRepoMock.Object,
                _imagenJugadorRepoMock.Object,
                _paths,
                _historialDePagosRepoMock.Object,
                _delegadoRepoMock.Object
            );
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