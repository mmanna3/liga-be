using Api.Core.Logica;
using Api.Core.Repositorios;
using Api.Persistencia.Repositorios;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace Api.TestsDeIntegracion;

[Collection("ImagenEscudo")]
public class ImagenEscudoRepoTest
{
    protected const string PuntoRojoBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";
    private readonly ImagenEscudoRepo _repo;
    private readonly AppPaths _paths;

    public ImagenEscudoRepoTest()
    {
        var env = new Mock<IWebHostEnvironment>().Object;
        _paths = new AppPathsForTest(env);
        _repo = new ImagenEscudoRepo(_paths);
        LimpiarCarpetaEscudos();
    }

    private void LimpiarCarpetaEscudos()
    {
        if (Directory.Exists(_paths.ImagenesEscudosAbsolute))
        {
            foreach (var archivo in Directory.GetFiles(_paths.ImagenesEscudosAbsolute))
            {
                if (!Path.GetFileName(archivo).Equals("default.jpg", StringComparison.OrdinalIgnoreCase))
                    File.Delete(archivo);
            }
        }
    }

    [Fact]
    public void PathRelativo_SinEscudoPropio_RetornaDefault()
    {
        const int clubId = 999;
        var result = _repo.PathRelativo(clubId);

        Assert.Equal(_paths.EscudoDefaultRelative, result);
    }

    [Fact]
    public void PathRelativo_ConEscudoPropio_RetornaRutaDelClub()
    {
        const int clubId = 1;
        _repo.Guardar(clubId, PuntoRojoBase64);

        var result = _repo.PathRelativo(clubId);

        Assert.Equal($"{_paths.ImagenesEscudosRelative}/{clubId}.jpg", result);
    }

    [Fact]
    public void Guardar_CreaArchivoEnDisco()
    {
        const int clubId = 2;
        _repo.Guardar(clubId, PuntoRojoBase64);

        var pathAbsoluto = $"{_paths.ImagenesEscudosAbsolute}/{clubId}.jpg";
        Assert.True(File.Exists(pathAbsoluto));
    }

    [Fact]
    public void Eliminar_ConEscudoExistente_EliminaArchivo()
    {
        const int clubId = 3;
        _repo.Guardar(clubId, PuntoRojoBase64);
        var pathAbsoluto = $"{_paths.ImagenesEscudosAbsolute}/{clubId}.jpg";
        Assert.True(File.Exists(pathAbsoluto));

        _repo.Eliminar(clubId);

        Assert.False(File.Exists(pathAbsoluto));
    }

    [Fact]
    public void Eliminar_SinEscudo_NoLanzaExcepcion()
    {
        const int clubId = 99999;
        var ex = Record.Exception(() => _repo.Eliminar(clubId));
        Assert.Null(ex);
    }
}

[CollectionDefinition("ImagenEscudo", DisableParallelization = true)]
public class ImagenEscudoCollection;
