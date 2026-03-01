using Api.Core.DTOs;
using Api.Core.Logica;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace Api.TestsDeIntegracion;

[CollectionDefinition("ImagenPersonaFichada", DisableParallelization = true)]
public class ImagenPersonaFichadaCollection;

public abstract class ImagenPersonaFichadaBaseTest
{
    protected const string PuntoRojoBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";
    protected const string DNI = "12345678";

    protected AppPaths Paths { get; }
    protected abstract IImagenPersonaFichadaRepo Repo { get; }
    protected abstract string ImagenesDefinitivasAbsolute { get; }

    protected ImagenPersonaFichadaBaseTest()
    {
        var env = new Mock<IWebHostEnvironment>().Object;
        Paths = new AppPathsForTest(env);
    }

    protected void LimpiarCarpetasDeFotos()
    {
        EliminarTodosLosArchivosEnLaCarpeta(ImagenesDefinitivasAbsolute);
        EliminarTodosLosArchivosEnLaCarpeta(Paths.ImagenesTemporalesCarnetAbsolute);
        EliminarTodosLosArchivosEnLaCarpeta(Paths.ImagenesTemporalesDNIFrenteAbsolute);
        EliminarTodosLosArchivosEnLaCarpeta(Paths.ImagenesTemporalesDNIDorsoAbsolute);
    }

    protected static void EliminarTodosLosArchivosEnLaCarpeta(string carpeta)
    {
        if (Directory.Exists(carpeta))
        {
            foreach (var archivo in Directory.GetFiles(carpeta))
                File.Delete(archivo);
        }
    }

    protected static IFotosDTO CrearFotosDto(string dni, string fotoBase64 = PuntoRojoBase64) => new JugadorDTO
    {
        DNI = dni,
        Nombre = "Test",
        Apellido = "Test",
        FechaNacimiento = new DateTime(1990, 1, 1),
        FotoCarnet = fotoBase64,
        FotoDNIFrente = fotoBase64,
        FotoDNIDorso = fotoBase64
    };

    [Fact]
    public void FicharPersonaTemporal_SinFotosTemporales_LanzaExcepcionControlada()
    {
        LimpiarCarpetasDeFotos();
        const string dniSinFotos = "99999999";

        var ex = Assert.Throws<ExcepcionControlada>(() => Repo.FicharPersonaTemporal(dniSinFotos));

        Assert.Equal("No se encontró una foto temporal del carnet para la persona", ex.Message);
    }

    protected static IFotosDTO CrearFotosDtoDelegado(string dni, string fotoBase64 = PuntoRojoBase64) => new DelegadoDTO
    {
        DNI = dni,
        Nombre = "Test",
        Apellido = "Test",
        FechaNacimiento = new DateTime(1990, 1, 1),
        ClubIds = new List<int> { 1 },
        FotoCarnet = fotoBase64,
        FotoDNIFrente = fotoBase64,
        FotoDNIDorso = fotoBase64
    };
}
