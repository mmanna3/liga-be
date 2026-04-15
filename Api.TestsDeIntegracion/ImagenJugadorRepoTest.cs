using Api.Core.DTOs;
using Api.Core.Logica;
using Api.Core.Repositorios;
using Api.Persistencia.Repositorios;
using SkiaSharp;

namespace Api.TestsDeIntegracion;

[Collection("ImagenPersonaFichada")]
public class ImagenJugadorRepoTest : ImagenPersonaFichadaBaseTest
{
    private readonly ImagenJugadorRepo _imagenJugadorRepo;

    public ImagenJugadorRepoTest()
    {
        _imagenJugadorRepo = new ImagenJugadorRepo(Paths);
        LimpiarCarpetasDeFotos();
    }

    protected override IImagenPersonaFichadaRepo Repo => _imagenJugadorRepo;
    protected override string ImagenesDefinitivasAbsolute => Paths.ImagenesJugadoresAbsolute;

    [Fact]
    public void GuardarFotosTemporalesDePersonaFichada_CreaArchivosEnCarpetasTemporales()
    {
        var fotos = CrearFotosDto(DNI);

        Repo.GuardarFotosTemporalesDePersonaFichada(DNI, fotos);

        Assert.True(File.Exists($"{Paths.ImagenesTemporalesCarnetAbsolute}/{DNI}.jpg"));
        Assert.True(File.Exists($"{Paths.ImagenesTemporalesDNIFrenteAbsolute}/{DNI}.jpg"));
        Assert.True(File.Exists($"{Paths.ImagenesTemporalesDNIDorsoAbsolute}/{DNI}.jpg"));
    }

    [Fact]
    public void GuardarFotosTemporalesDeJugadorAutofichado_CreaArchivosEnCarpetasTemporales()
    {
        var vm = new JugadorDTO
        {
            DNI = DNI,
            FotoCarnet = PuntoRojoBase64,
            FotoDNIFrente = PuntoRojoBase64,
            FotoDNIDorso = PuntoRojoBase64
        };

        _imagenJugadorRepo.GuardarFotosTemporalesDeJugadorAutofichado(vm);

        Assert.True(File.Exists($"{Paths.ImagenesTemporalesCarnetAbsolute}/{DNI}.jpg"));
        Assert.True(File.Exists($"{Paths.ImagenesTemporalesDNIFrenteAbsolute}/{DNI}.jpg"));
        Assert.True(File.Exists($"{Paths.ImagenesTemporalesDNIDorsoAbsolute}/{DNI}.jpg"));
    }

    [Fact]
    public void GuardarFotosTemporalesDePersonaFichadaSiendoEditada_CreaArchivosEnCarpetasTemporales()
    {
        var fotos = CrearFotosDto(DNI);

        Repo.GuardarFotosTemporalesDePersonaFichadaSiendoEditada(DNI, fotos);

        Assert.True(File.Exists($"{Paths.ImagenesTemporalesCarnetAbsolute}/{DNI}.jpg"));
        Assert.True(File.Exists($"{Paths.ImagenesTemporalesDNIFrenteAbsolute}/{DNI}.jpg"));
        Assert.True(File.Exists($"{Paths.ImagenesTemporalesDNIDorsoAbsolute}/{DNI}.jpg"));
    }

    [Fact]
    public void FicharJugadorTemporal_ConDefinitivaHuerfana_ReemplazaConFotoTemporal()
    {
        // Carnet distinto al PuntoRojoBase64 por defecto (otro PNG 1×1)
        const string fotoNueva =
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==";

        LimpiarCarpetasDeFotos();
        Repo.GuardarFotosTemporalesDePersonaFichada(DNI, CrearFotosDto(DNI, fotoNueva));
        _imagenJugadorRepo.FicharJugadorTemporal(DNI);
        var esperado = Repo.GetFotoCarnetEnBase64(DNI);

        LimpiarCarpetasDeFotos();
        Repo.GuardarFotosTemporalesDePersonaFichada(DNI, CrearFotosDto(DNI, PuntoRojoBase64));
        _imagenJugadorRepo.FicharJugadorTemporal(DNI);
        Assert.NotEqual(esperado, Repo.GetFotoCarnetEnBase64(DNI));

        EliminarTodosLosArchivosEnLaCarpeta(Paths.ImagenesTemporalesCarnetAbsolute);
        EliminarTodosLosArchivosEnLaCarpeta(Paths.ImagenesTemporalesDNIFrenteAbsolute);
        EliminarTodosLosArchivosEnLaCarpeta(Paths.ImagenesTemporalesDNIDorsoAbsolute);
        Repo.GuardarFotosTemporalesDePersonaFichada(DNI, CrearFotosDto(DNI, fotoNueva));

        _imagenJugadorRepo.FicharJugadorTemporal(DNI);

        Assert.Equal(esperado, Repo.GetFotoCarnetEnBase64(DNI));
    }

    [Fact]
    public void FicharJugadorTemporal_MueveFotoDeTemporalADefinitiva()
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

        _imagenJugadorRepo.FicharJugadorTemporal(DNI);

        Assert.True(File.Exists(Path.Combine(Paths.ImagenesJugadoresAbsolute, $"{DNI}.jpg")));
        Assert.False(File.Exists(Path.Combine(Paths.ImagenesTemporalesCarnetAbsolute, $"{DNI}.png")));
        Assert.False(File.Exists(Path.Combine(Paths.ImagenesTemporalesDNIFrenteAbsolute, $"{DNI}.png")));
        Assert.False(File.Exists(Path.Combine(Paths.ImagenesTemporalesDNIDorsoAbsolute, $"{DNI}.png")));
    }

    [Fact]
    public void FicharJugadorTemporal_ConArchivoJpg_MueveFotoYBorraFotosDNI()
    {
        // El flujo normal: GuardarFotosTemporalesDePersonaFichada siempre guarda .jpg
        var fotos = CrearFotosDto(DNI);
        Repo.GuardarFotosTemporalesDePersonaFichada(DNI, fotos);

        _imagenJugadorRepo.FicharJugadorTemporal(DNI);

        Assert.True(File.Exists($"{Paths.ImagenesJugadoresAbsolute}/{DNI}.jpg"));
        Assert.False(File.Exists($"{Paths.ImagenesTemporalesCarnetAbsolute}/{DNI}.jpg"));
        Assert.False(File.Exists($"{Paths.ImagenesTemporalesDNIFrenteAbsolute}/{DNI}.jpg"));
        Assert.False(File.Exists($"{Paths.ImagenesTemporalesDNIDorsoAbsolute}/{DNI}.jpg"));
    }

    [Fact]
    public void EliminarFotosDelJugador_EliminaTodasLasFotos()
    {
        var fotos = CrearFotosDto(DNI);
        Repo.GuardarFotosTemporalesDePersonaFichada(DNI, fotos);
        _imagenJugadorRepo.FicharJugadorTemporal(DNI);

        var pathDefinitivo = $"{Paths.ImagenesJugadoresAbsolute}/{DNI}.jpg";
        var pathCarnet = $"{Paths.ImagenesTemporalesCarnetAbsolute}/{DNI}.jpg";
        var pathDNIFrente = $"{Paths.ImagenesTemporalesDNIFrenteAbsolute}/{DNI}.jpg";
        var pathDNIDorso = $"{Paths.ImagenesTemporalesDNIDorsoAbsolute}/{DNI}.jpg";

        Assert.True(File.Exists(pathDefinitivo));

        _imagenJugadorRepo.EliminarFotosDelJugador(DNI);

        Assert.False(File.Exists(pathDefinitivo));
        Assert.False(File.Exists(pathCarnet));
        Assert.False(File.Exists(pathDNIFrente));
        Assert.False(File.Exists(pathDNIDorso));
    }

    [Fact]
    public void GetFotoCarnetEnBase64_ConFotoDefinitiva_RetornaBase64()
    {
        var fotos = CrearFotosDto(DNI);
        Repo.GuardarFotosTemporalesDePersonaFichada(DNI, fotos);
        _imagenJugadorRepo.FicharJugadorTemporal(DNI);

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
        var fotos = CrearFotosDto(DNI);
        Repo.GuardarFotosTemporalesDePersonaFichada(DNI, fotos);
        var pathAbsoluto = $"{Paths.ImagenesTemporalesCarnetAbsolute}/{DNI}.jpg";

        var result = Repo.GetFotoEnBase64ConPathAbsoluto(pathAbsoluto);

        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public void Path_RetornaRutaRelativaCorrecta()
    {
        var result = Repo.Path(DNI);

        Assert.Equal($"{Paths.ImagenesJugadoresRelative}/{DNI}.jpg", result);
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
        var fotos = CrearFotosDto(DNI);
        Repo.GuardarFotosTemporalesDePersonaFichada(DNI, fotos);
        _imagenJugadorRepo.FicharJugadorTemporal(DNI);

        Repo.Eliminar(DNI);

        Assert.False(File.Exists($"{Paths.ImagenesJugadoresAbsolute}/{DNI}.jpg"));
    }

    [Fact]
    public void CambiarDNI_RenombraArchivoDefinitivo()
    {
        var fotos = CrearFotosDto(DNI);
        Repo.GuardarFotosTemporalesDePersonaFichada(DNI, fotos);
        _imagenJugadorRepo.FicharJugadorTemporal(DNI);
        const string dniNuevo = "87654321";

        Repo.CambiarDNI(DNI, dniNuevo);

        Assert.False(File.Exists($"{Paths.ImagenesJugadoresAbsolute}/{DNI}.jpg"));
        Assert.True(File.Exists($"{Paths.ImagenesJugadoresAbsolute}/{dniNuevo}.jpg"));
    }

    [Fact]
    public void RenombrarFotosTemporalesPorCambioDeDNI_RenombraArchivosTemporales()
    {
        var fotos = CrearFotosDto(DNI);
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
        Repo.GuardarFotosTemporalesDePersonaFichada(dni1, CrearFotosDto(dni1));
        Repo.GuardarFotosTemporalesDePersonaFichada(dni2, CrearFotosDto(dni2));
        _imagenJugadorRepo.FicharJugadorTemporal(dni1);
        _imagenJugadorRepo.FicharJugadorTemporal(dni2);

        Repo.EliminarLista(new[] { dni1, dni2 });

        Assert.False(File.Exists($"{Paths.ImagenesJugadoresAbsolute}/{dni1}.jpg"));
        Assert.False(File.Exists($"{Paths.ImagenesJugadoresAbsolute}/{dni2}.jpg"));
    }
}
