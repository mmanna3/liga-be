using Api.Core.Logica;
using Api.Core.Repositorios;
using SkiaSharp;

namespace Api.Persistencia.Repositorios;

public class ImagenSponsorWebPublicaRepo : IImagenSponsorWebPublicaRepo
{
    private readonly AppPaths _paths;

    public ImagenSponsorWebPublicaRepo(AppPaths paths)
    {
        _paths = paths;
    }

    public string GetImagenEnBase64(int sponsorId)
    {
        try
        {
            var path = Path.Combine(_paths.ImagenesPublicidadesAbsolute, $"{sponsorId}.jpg");
            if (!File.Exists(path))
                return string.Empty;

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var img = SKImage.FromEncodedData(stream);
            if (img is null)
                return string.Empty;

            return ImagenUtility.ImageToBase64(img);
        }
        catch (IOException)
        {
            return string.Empty;
        }
        catch (UnauthorizedAccessException)
        {
            return string.Empty;
        }
    }

    public string GetRutaRelativaLogo(int sponsorId)
    {
        var pathCustom = $"{_paths.ImagenesPublicidadesAbsolute}/{sponsorId}.jpg";
        var baseRel = _paths.ImagenesPublicidadesRelative.TrimEnd('/');
        if (File.Exists(pathCustom))
            return $"{baseRel}/{sponsorId}.jpg";
        return string.Empty;
    }

    public void Guardar(int sponsorId, string imagenBase64)
    {
        Directory.CreateDirectory(_paths.ImagenesPublicidadesAbsolute);
        var pathDestino = $"{_paths.ImagenesPublicidadesAbsolute}/{sponsorId}.jpg";
        var imagen = ImagenUtility.Comprimir(imagenBase64);
        GuardarImagenEnDisco(pathDestino, imagen);
    }

    public void Eliminar(int sponsorId)
    {
        var path = $"{_paths.ImagenesPublicidadesAbsolute}/{sponsorId}.jpg";
        if (File.Exists(path))
            File.Delete(path);
    }

    public bool Existe(int sponsorId) =>
        File.Exists($"{_paths.ImagenesPublicidadesAbsolute}/{sponsorId}.jpg");

    private static void GuardarImagenEnDisco(string path, SKBitmap imagen)
    {
        using var stream = new FileStream(path, FileMode.Create);
        using var image = SKImage.FromBitmap(imagen);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 80);
        data.SaveTo(stream);
    }
}
