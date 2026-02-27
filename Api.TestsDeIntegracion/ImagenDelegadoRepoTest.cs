using Api.Core.Repositorios;
using Api.Persistencia.Repositorios;
using SkiaSharp;

namespace Api.TestsDeIntegracion;

[Collection("ImagenPersonaFichada")]
public class ImagenDelegadoRepoTest : ImagenPersonaFichadaBaseTest
{
    private readonly ImagenDelegadoRepo _imagenDelegadoRepo;

    public ImagenDelegadoRepoTest()
    {
        _imagenDelegadoRepo = new ImagenDelegadoRepo(Paths);
        LimpiarCarpetasDeFotos();
    }

    protected override IImagenPersonaFichadaRepo Repo => _imagenDelegadoRepo;
    protected override string ImagenesDefinitivasAbsolute => Paths.ImagenesDelegadosAbsolute;

    [Fact]
    public void GuardarFotosTemporalesDePersonaFichada_CreaArchivosEnCarpetasTemporales()
    {
        var fotos = CrearFotosDtoDelegado(DNI);

        Repo.GuardarFotosTemporalesDePersonaFichada(DNI, fotos);

        Assert.True(File.Exists($"{Paths.ImagenesTemporalesCarnetAbsolute}/{DNI}.jpg"));
        Assert.True(File.Exists($"{Paths.ImagenesTemporalesDNIFrenteAbsolute}/{DNI}.jpg"));
        Assert.True(File.Exists($"{Paths.ImagenesTemporalesDNIDorsoAbsolute}/{DNI}.jpg"));
    }

    [Fact]
    public void GuardarFotosTemporalesDePersonaFichadaSiendoEditada_CreaArchivosEnCarpetasTemporales()
    {
        var fotos = CrearFotosDtoDelegado(DNI);

        Repo.GuardarFotosTemporalesDePersonaFichadaSiendoEditada(DNI, fotos);

        Assert.True(File.Exists($"{Paths.ImagenesTemporalesCarnetAbsolute}/{DNI}.jpg"));
        Assert.True(File.Exists($"{Paths.ImagenesTemporalesDNIFrenteAbsolute}/{DNI}.jpg"));
        Assert.True(File.Exists($"{Paths.ImagenesTemporalesDNIDorsoAbsolute}/{DNI}.jpg"));
    }

    [Fact]
    public void FicharPersonaTemporal_MueveFotoDeTemporalADefinitiva()
    {
        Directory.CreateDirectory(Paths.ImagenesTemporalesCarnetAbsolute);
        Directory.CreateDirectory(Paths.ImagenesTemporalesDNIFrenteAbsolute);
        Directory.CreateDirectory(Paths.ImagenesTemporalesDNIDorsoAbsolute);

        using (var bitmap = new SKBitmap(1, 1))
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
        {
            File.WriteAllBytes(Path.Combine(Paths.ImagenesTemporalesCarnetAbsolute, $"{DNI}.png"), data.ToArray());
            File.WriteAllBytes(Path.Combine(Paths.ImagenesTemporalesDNIFrenteAbsolute, $"{DNI}.png"), data.ToArray());
            File.WriteAllBytes(Path.Combine(Paths.ImagenesTemporalesDNIDorsoAbsolute, $"{DNI}.png"), data.ToArray());
        }

        Repo.FicharPersonaTemporal(DNI);

        Assert.True(File.Exists(Path.Combine(Paths.ImagenesDelegadosAbsolute, $"{DNI}.jpg")));
        Assert.False(File.Exists(Path.Combine(Paths.ImagenesTemporalesCarnetAbsolute, $"{DNI}.png")));
        Assert.False(File.Exists(Path.Combine(Paths.ImagenesTemporalesDNIFrenteAbsolute, $"{DNI}.png")));
        Assert.False(File.Exists(Path.Combine(Paths.ImagenesTemporalesDNIDorsoAbsolute, $"{DNI}.png")));
    }

    [Fact]
    public void EliminarTodasLasFotos_EliminaTodasLasFotos()
    {
        var fotos = CrearFotosDtoDelegado(DNI);
        Repo.GuardarFotosTemporalesDePersonaFichada(DNI, fotos);
        Repo.FicharPersonaTemporal(DNI);

        var pathDefinitivo = $"{Paths.ImagenesDelegadosAbsolute}/{DNI}.jpg";
        var pathCarnet = $"{Paths.ImagenesTemporalesCarnetAbsolute}/{DNI}.jpg";
        var pathDNIFrente = $"{Paths.ImagenesTemporalesDNIFrenteAbsolute}/{DNI}.jpg";
        var pathDNIDorso = $"{Paths.ImagenesTemporalesDNIDorsoAbsolute}/{DNI}.jpg";

        Assert.True(File.Exists(pathDefinitivo));

        Repo.EliminarTodasLasFotos(DNI);

        Assert.False(File.Exists(pathDefinitivo));
        Assert.False(File.Exists(pathCarnet));
        Assert.False(File.Exists(pathDNIFrente));
        Assert.False(File.Exists(pathDNIDorso));
    }

    [Fact]
    public void GetFotoCarnetEnBase64_ConFotoDefinitiva_RetornaBase64()
    {
        var fotos = CrearFotosDtoDelegado(DNI);
        Repo.GuardarFotosTemporalesDePersonaFichada(DNI, fotos);
        Repo.FicharPersonaTemporal(DNI);

        var result = Repo.GetFotoCarnetEnBase64(DNI);

        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public void GetFotoCarnetEnBase64_SinFoto_RetornaVacio()
    {
        var result = Repo.GetFotoCarnetEnBase64("99999999");

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetFotoEnBase64ConPathAbsoluto_ConArchivoExistente_RetornaBase64()
    {
        var fotos = CrearFotosDtoDelegado(DNI);
        Repo.GuardarFotosTemporalesDePersonaFichada(DNI, fotos);
        var pathAbsoluto = $"{Paths.ImagenesTemporalesCarnetAbsolute}/{DNI}.jpg";

        var result = Repo.GetFotoEnBase64ConPathAbsoluto(pathAbsoluto);

        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public void Path_RetornaRutaRelativaCorrecta()
    {
        var result = Repo.Path(DNI);

        Assert.Equal($"{Paths.ImagenesDelegadosRelative}/{DNI}.jpg", result);
    }

    [Fact]
    public void PathFotoTemporalCarnet_RetornaRutaCorrecta()
    {
        Assert.Equal($"{Paths.ImagenesTemporalesCarnetRelative}/{DNI}.jpg", Repo.PathFotoTemporalCarnet(DNI));
    }

    [Fact]
    public void PathFotoTemporalDNIFrente_RetornaRutaCorrecta()
    {
        Assert.Equal($"{Paths.ImagenesTemporalesDNIFrenteRelative}/{DNI}.jpg", Repo.PathFotoTemporalDNIFrente(DNI));
    }

    [Fact]
    public void PathFotoTemporalDNIDorso_RetornaRutaCorrecta()
    {
        Assert.Equal($"{Paths.ImagenesTemporalesDNIDorsoRelative}/{DNI}.jpg", Repo.PathFotoTemporalDNIDorso(DNI));
    }

    [Fact]
    public void Eliminar_ConFotoDefinitiva_EliminaArchivo()
    {
        var fotos = CrearFotosDtoDelegado(DNI);
        Repo.GuardarFotosTemporalesDePersonaFichada(DNI, fotos);
        Repo.FicharPersonaTemporal(DNI);

        Repo.Eliminar(DNI);

        Assert.False(File.Exists($"{Paths.ImagenesDelegadosAbsolute}/{DNI}.jpg"));
    }

    [Fact]
    public void CambiarDNI_RenombraArchivoDefinitivo()
    {
        var fotos = CrearFotosDtoDelegado(DNI);
        Repo.GuardarFotosTemporalesDePersonaFichada(DNI, fotos);
        Repo.FicharPersonaTemporal(DNI);
        const string dniNuevo = "87654321";

        Repo.CambiarDNI(DNI, dniNuevo);

        Assert.False(File.Exists($"{Paths.ImagenesDelegadosAbsolute}/{DNI}.jpg"));
        Assert.True(File.Exists($"{Paths.ImagenesDelegadosAbsolute}/{dniNuevo}.jpg"));
    }

    [Fact]
    public void RenombrarFotosTemporalesPorCambioDeDNI_RenombraArchivosTemporales()
    {
        var fotos = CrearFotosDtoDelegado(DNI);
        Repo.GuardarFotosTemporalesDePersonaFichada(DNI, fotos);
        const string dniNuevo = "87654321";

        Repo.RenombrarFotosTemporalesPorCambioDeDNI(DNI, dniNuevo);

        Assert.False(File.Exists($"{Paths.ImagenesTemporalesCarnetAbsolute}/{DNI}.jpg"));
        Assert.True(File.Exists($"{Paths.ImagenesTemporalesCarnetAbsolute}/{dniNuevo}.jpg"));
    }

    [Fact]
    public void EliminarLista_EliminaFotosDeVariosDni()
    {
        var dni1 = "11111111";
        var dni2 = "22222222";
        Repo.GuardarFotosTemporalesDePersonaFichada(dni1, CrearFotosDtoDelegado(dni1));
        Repo.GuardarFotosTemporalesDePersonaFichada(dni2, CrearFotosDtoDelegado(dni2));
        Repo.FicharPersonaTemporal(dni1);
        Repo.FicharPersonaTemporal(dni2);

        Repo.EliminarLista(new[] { dni1, dni2 });

        Assert.False(File.Exists($"{Paths.ImagenesDelegadosAbsolute}/{dni1}.jpg"));
        Assert.False(File.Exists($"{Paths.ImagenesDelegadosAbsolute}/{dni2}.jpg"));
    }
}
