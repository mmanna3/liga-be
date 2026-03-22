namespace Api.Core.Servicios.Interfaces;

public interface IGoogleDriveCore
{
    Task<string> SubirArchivo(string rutaArchivoLocal, string nombreArchivoEnDrive);
}
