using Api.Core.DTOs;
using Api.Persistencia.Repositorios;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace Api.TestsDeIntegracion
{
    public class ImagenJugadorRepoTest
    {
        private const string PuntoRojoBase64ConUriDataJpg = "data:image/jpeg;base64, iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";
        private const string PuntoRojoBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";

        private readonly AppPathsForTest _paths;
        private readonly ImagenJugadorRepo _imagenJugadorRepo;
        private const string DNI = "12345678";

        public ImagenJugadorRepoTest()
        {
            var env = new Mock<IWebHostEnvironment>().Object;
            _paths = new AppPathsForTest(env);
            _imagenJugadorRepo = new ImagenJugadorRepo(_paths);
            
            EliminarTodosLosArchivosEnLaCarpeta(_paths.ImagenesJugadoresAbsolute);
        }
        
        [Fact]
        public void GuardarFotosTemporalesDeJugadorAutofichado()
        {
            var vm = new JugadorDTO
            {
                FotoCarnet = PuntoRojoBase64,
                FotoDNIFrente = PuntoRojoBase64,
                FotoDNIDorso = PuntoRojoBase64,
                DNI = DNI
            };

            _imagenJugadorRepo.GuardarFotosTemporalesDeJugadorAutofichado(vm);

            var pathFotoCarnetTemporal = $"{_paths.ImagenesTemporalesJugadorCarnetAbsolute}/{DNI}.jpg";
            var pathFotoDNIFrenteTemporal = $"{_paths.ImagenesTemporalesJugadorDNIFrenteAbsolute}/{DNI}.jpg";
            var pathFotoDNIDorsoTemporal = $"{_paths.ImagenesTemporalesJugadorDNIDorsoAbsolute}/{DNI}.jpg";

            Assert.True(File.Exists(pathFotoCarnetTemporal));
            Assert.True(File.Exists(pathFotoDNIFrenteTemporal));
            Assert.True(File.Exists(pathFotoDNIDorsoTemporal));
        }

        private static void EliminarTodosLosArchivosEnLaCarpeta(string carpeta)
        {
            if (Directory.Exists(carpeta))
            {
                var archivos = Directory.GetFiles(carpeta);
                foreach (var archivo in archivos)
                {
                    File.Delete(archivo);
                }
            }
        }

        [Fact]
        public void EliminarFotosDelJugador_DeletesAllPlayerImages()
        {
            // Arrange
            var vm = new JugadorDTO
            {
                FotoCarnet = PuntoRojoBase64,
                FotoDNIFrente = PuntoRojoBase64,
                FotoDNIDorso = PuntoRojoBase64,
                DNI = DNI
            };

            _imagenJugadorRepo.GuardarFotosTemporalesDeJugadorAutofichado(vm);
            _imagenJugadorRepo.FicharJugadorTemporal(DNI);

            var pathJugador = $"{_paths.ImagenesJugadoresAbsolute}/{DNI}.jpg";
            var pathCarnet = $"{_paths.ImagenesTemporalesJugadorCarnetAbsolute}/{DNI}.jpg";
            var pathDNIFrente = $"{_paths.ImagenesTemporalesJugadorDNIFrenteAbsolute}/{DNI}.jpg";
            var pathDNIDorso = $"{_paths.ImagenesTemporalesJugadorDNIDorsoAbsolute}/{DNI}.jpg";

            Assert.True(File.Exists(pathJugador));
            Assert.True(File.Exists(pathCarnet));
            Assert.True(File.Exists(pathDNIFrente));
            Assert.True(File.Exists(pathDNIDorso));

            // Act
            _imagenJugadorRepo.EliminarFotosDelJugador(DNI);

            // Assert
            Assert.False(File.Exists(pathJugador));
            Assert.False(File.Exists(pathCarnet));
            Assert.False(File.Exists(pathDNIFrente));
            Assert.False(File.Exists(pathDNIDorso));
        }
    }
}