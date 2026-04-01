using Api.Core.Servicios;

namespace Api.Core.Servicios.Interfaces;

public interface IGoogleDriveCore
{
    Task<string> SubirArchivo(string rutaArchivoLocal, string nombreArchivoEnDrive);
    Task<RotacionBackupsDriveResult> RotarBackupsEnDrive();
    string ObtenerUrlDeAutorizacion(string redirectUri);
    Task GuardarRefreshToken(string code, string redirectUri);
}
