namespace Api.Core.Servicios.Interfaces;

public interface IBackupCore
{
    Task<(Stream stream, string fileName)> GenerarBackupBaseDeDatos();
    Task<(Stream stream, string fileName)> GenerarBackupImagenes();
}
