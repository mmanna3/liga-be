namespace Api.Core.Repositorios;

public interface IImagenSponsorWebPublicaRepo
{
    string GetImagenEnBase64(int sponsorId);
    string GetImagenDataUrl(int sponsorId);
    string GetRutaRelativaLogo(int sponsorId);
    string? GetRutaAbsolutaLogo(int sponsorId);
    string? GetContentTypeLogo(int sponsorId);
    void Guardar(int sponsorId, string imagenBase64);
    void Eliminar(int sponsorId);
    bool Existe(int sponsorId);
}
