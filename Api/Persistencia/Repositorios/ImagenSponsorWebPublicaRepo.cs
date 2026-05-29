using Api.Core.Logica;
using Api.Core.Otros;
using Api.Core.Repositorios;
using SkiaSharp;

namespace Api.Persistencia.Repositorios;

public class ImagenSponsorWebPublicaRepo : IImagenSponsorWebPublicaRepo
{
    private static readonly string[] ExtensionesSoportadas = [".jpg", ".jpeg", ".png", ".svg"];

    private readonly AppPaths _paths;

    public ImagenSponsorWebPublicaRepo(AppPaths paths)
    {
        _paths = paths;
    }

    public string GetImagenEnBase64(int sponsorId)
    {
        var path = BuscarArchivoLogo(sponsorId);
        if (path is null)
            return string.Empty;

        try
        {
            if (Path.GetExtension(path).Equals(".svg", StringComparison.OrdinalIgnoreCase))
                return Convert.ToBase64String(File.ReadAllBytes(path));

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

    public string GetImagenDataUrl(int sponsorId)
    {
        var path = BuscarArchivoLogo(sponsorId);
        if (path is null)
            return string.Empty;

        var base64 = GetImagenEnBase64(sponsorId);
        if (string.IsNullOrEmpty(base64))
            return string.Empty;

        var mimeType = ImagenUtility.ObtenerMimeTypeDesdeExtension(Path.GetExtension(path));
        return ImagenUtility.AgregarMimeType(base64, mimeType);
    }

    public string GetRutaRelativaLogo(int sponsorId)
    {
        var path = BuscarArchivoLogo(sponsorId);
        if (path is null)
            return string.Empty;

        var baseRel = _paths.ImagenesSponsorsRelative.TrimEnd('/');
        return $"{baseRel}/{Path.GetFileName(path)}";
    }

    public string? GetRutaAbsolutaLogo(int sponsorId) => BuscarArchivoLogo(sponsorId);

    public string? GetContentTypeLogo(int sponsorId)
    {
        var path = BuscarArchivoLogo(sponsorId);
        return path is null ? null : ImagenUtility.ObtenerMimeTypeDesdeExtension(Path.GetExtension(path));
    }

    public void Guardar(int sponsorId, string imagenBase64)
    {
        Directory.CreateDirectory(_paths.ImagenesSponsorsAbsolute);
        Eliminar(sponsorId);

        var formato = ImagenUtility.DetectarFormatoSponsor(imagenBase64);
        var extension = ImagenUtility.ExtensionSponsor(formato);
        var pathDestino = Path.Combine(_paths.ImagenesSponsorsAbsolute, $"{sponsorId}{extension}");

        switch (formato)
        {
            case SponsorImagenFormato.Svg:
                File.WriteAllBytes(pathDestino, ImagenUtility.ObtenerBytesSvg(imagenBase64));
                break;
            case SponsorImagenFormato.Png:
                GuardarPngEnDisco(pathDestino, ImagenUtility.RedimensionarPng(imagenBase64));
                break;
            default:
                GuardarJpegEnDisco(pathDestino, ImagenUtility.Comprimir(imagenBase64));
                break;
        }
    }

    public void Eliminar(int sponsorId)
    {
        foreach (var extension in ExtensionesSoportadas)
        {
            var path = Path.Combine(_paths.ImagenesSponsorsAbsolute, $"{sponsorId}{extension}");
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    public bool Existe(int sponsorId) => BuscarArchivoLogo(sponsorId) is not null;

    private string? BuscarArchivoLogo(int sponsorId)
    {
        foreach (var extension in ExtensionesSoportadas)
        {
            var path = Path.Combine(_paths.ImagenesSponsorsAbsolute, $"{sponsorId}{extension}");
            if (File.Exists(path))
                return path;
        }

        return null;
    }

    private static void GuardarJpegEnDisco(string path, SKBitmap imagen)
    {
        using var stream = new FileStream(path, FileMode.Create);
        using var image = SKImage.FromBitmap(imagen);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 80);
        data.SaveTo(stream);
    }

    private static void GuardarPngEnDisco(string path, SKBitmap imagen)
    {
        using var stream = new FileStream(path, FileMode.Create);
        using var image = SKImage.FromBitmap(imagen);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        data.SaveTo(stream);
    }
}
