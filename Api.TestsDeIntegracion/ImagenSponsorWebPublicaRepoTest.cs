using Api.Core.Logica;
using Api.Persistencia.Repositorios;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace Api.TestsDeIntegracion;

[Collection("ImagenSponsor")]
public class ImagenSponsorWebPublicaRepoTest
{
    protected const string PuntoRojoBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";

    private readonly ImagenSponsorWebPublicaRepo _repo;
    private readonly AppPaths _paths;

    public ImagenSponsorWebPublicaRepoTest()
    {
        var env = new Mock<IWebHostEnvironment>().Object;
        _paths = new AppPathsForTest(env);
        _repo = new ImagenSponsorWebPublicaRepo(_paths);
        LimpiarCarpetaSponsors();
    }

    private void LimpiarCarpetaSponsors()
    {
        if (!Directory.Exists(_paths.ImagenesSponsorsAbsolute))
            return;

        foreach (var archivo in Directory.GetFiles(_paths.ImagenesSponsorsAbsolute))
            File.Delete(archivo);
    }

    [Fact]
    public void AppPaths_UsaCarpetaImagenesSponsors()
    {
        Assert.Equal("/Imagenes/Sponsors", _paths.ImagenesSponsorsRelative);
        Assert.EndsWith(
            $"{Path.DirectorySeparatorChar}Imagenes{Path.DirectorySeparatorChar}Sponsors",
            _paths.ImagenesSponsorsAbsolute);
    }

    [Fact]
    public void Guardar_CreaArchivoEnImagenesSponsors()
    {
        const int sponsorId = 1;
        _repo.Guardar(sponsorId, PuntoRojoBase64);

        var pathAbsoluto = Path.Combine(_paths.ImagenesSponsorsAbsolute, $"{sponsorId}.jpg");
        Assert.True(File.Exists(pathAbsoluto));
        Assert.True(_repo.Existe(sponsorId));
    }

    [Fact]
    public void GetRutaRelativaLogo_ConArchivo_DevuelveRutaSponsors()
    {
        const int sponsorId = 2;
        _repo.Guardar(sponsorId, PuntoRojoBase64);

        var ruta = _repo.GetRutaRelativaLogo(sponsorId);

        Assert.Equal($"{_paths.ImagenesSponsorsRelative.TrimEnd('/')}/{sponsorId}.jpg", ruta);
    }

    [Fact]
    public void GetRutaAbsolutaLogo_ConArchivo_DevuelvePathEnSponsors()
    {
        const int sponsorId = 3;
        _repo.Guardar(sponsorId, PuntoRojoBase64);

        var path = _repo.GetRutaAbsolutaLogo(sponsorId);

        Assert.NotNull(path);
        Assert.Equal(Path.Combine(_paths.ImagenesSponsorsAbsolute, $"{sponsorId}.jpg"), path);
    }

    [Fact]
    public void GetImagenEnBase64_ConArchivo_RetornaBase64()
    {
        const int sponsorId = 4;
        _repo.Guardar(sponsorId, PuntoRojoBase64);

        var result = _repo.GetImagenEnBase64(sponsorId);

        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public void Eliminar_ConArchivo_EliminaDeImagenesSponsors()
    {
        const int sponsorId = 5;
        _repo.Guardar(sponsorId, PuntoRojoBase64);
        var pathAbsoluto = Path.Combine(_paths.ImagenesSponsorsAbsolute, $"{sponsorId}.jpg");
        Assert.True(File.Exists(pathAbsoluto));

        _repo.Eliminar(sponsorId);

        Assert.False(File.Exists(pathAbsoluto));
        Assert.False(_repo.Existe(sponsorId));
    }

    [Fact]
    public void Eliminar_SinArchivo_NoLanzaExcepcion()
    {
        var ex = Record.Exception(() => _repo.Eliminar(99999));
        Assert.Null(ex);
    }
}

[CollectionDefinition("ImagenSponsor", DisableParallelization = true)]
public class ImagenSponsorCollection;
