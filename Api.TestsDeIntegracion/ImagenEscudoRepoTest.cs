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
                if (!Path.GetFileName(archivo).Equals("_pordefecto.jpg", StringComparison.OrdinalIgnoreCase))
                    File.Delete(archivo);
            }
        }
    }

    [Fact]
    public void GetRutaRelativaEscudo_ConArchivoPropio_DevuelveRutaDelClub()
    {
        const int clubId = 42;
        _repo.Guardar(clubId, PuntoRojoBase64);

        var ruta = _repo.GetRutaRelativaEscudo(clubId);

        Assert.Equal($"{_paths.ImagenesEscudosRelative.TrimEnd('/')}/{clubId}.jpg", ruta);
    }

    [Fact]
    public void GetRutaRelativaEscudo_SinArchivoPropio_DevuelvePorDefecto()
    {
        const int clubId = 424242;
        var ruta = _repo.GetRutaRelativaEscudo(clubId);

        Assert.Equal(_paths.EscudoDefaultRelative, ruta);
    }

    [Fact]
    public void GetEscudoEnBase64_SinEscudoPropio_RetornaDefault()
    {
        Directory.CreateDirectory(_paths.ImagenesEscudosAbsolute);
        var defaultPath = _paths.EscudoDefaultFileAbsolute;
        if (!File.Exists(defaultPath))
            File.WriteAllBytes(defaultPath, Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="));

        const int clubId = 999;
        var result = _repo.GetEscudoEnBase64(clubId);

        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public void GetEscudoEnBase64_ConEscudoPropio_RetornaBase64()
    {
        const int clubId = 1;
        _repo.Guardar(clubId, PuntoRojoBase64);

        var result = _repo.GetEscudoEnBase64(clubId);

        Assert.False(string.IsNullOrEmpty(result));
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
    public void GuardarEscudoPorDefecto_CreaJpegEnRutaPorDefecto()
    {
        const string dataUriJpeg =
            "data:image/jpeg;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";

        _repo.GuardarEscudoPorDefecto(dataUriJpeg);

        Assert.True(File.Exists(_paths.EscudoDefaultFileAbsolute));
        var bytes = File.ReadAllBytes(_paths.EscudoDefaultFileAbsolute);
        Assert.True(bytes.Length >= 3);
        Assert.Equal(0xFF, bytes[0]);
        Assert.Equal(0xD8, bytes[1]);
        Assert.Equal(0xFF, bytes[2]);
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
