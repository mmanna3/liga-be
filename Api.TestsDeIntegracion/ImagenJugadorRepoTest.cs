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
            var vm = new JugadorPendienteDeAprobacionDTO
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
                foreach (var archivo in Directory.GetFiles(carpeta))
                {
                    File.Delete(archivo);
                }
            }
        }
    }
}