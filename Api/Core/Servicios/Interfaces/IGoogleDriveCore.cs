namespace Api.Core.Servicios.Interfaces;

public interface IGoogleDriveCore
{
    Task<string> SubirArchivo(string rutaArchivoLocal, string nombreArchivoEnDrive);
    Task RotarBackupsEnDrive();
    string ObtenerUrlDeAutorizacion(string redirectUri);
    Task GuardarRefreshToken(string code, string redirectUri);
}
