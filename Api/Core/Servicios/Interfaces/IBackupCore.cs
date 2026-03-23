namespace Api.Core.Servicios.Interfaces;

public interface IBackupCore
{
    Task<(Stream stream, string fileName)> GenerarBackupBaseDeDatos();
    Task<(Stream stream, string fileName)> GenerarBackupImagenes();
    Task<string> GuardarBackupBaseDeDatosEnDisco();
    Task<string> GuardarBackupImagenesEnDisco();
    Task RestaurarDesdeBackup();
    Task RestaurarImagenesDesdeBackup();
    void ValidarCantidadArchivosEnCarpetaBackup();
}
