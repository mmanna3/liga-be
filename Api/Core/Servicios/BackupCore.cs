using System.IO.Compression;
using Api.Core.Logica;
using Api.Core.Servicios.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace Api.Core.Servicios;

public class BackupCore : IBackupCore
{
    private readonly IConfiguration _configuration;
    private readonly AppPaths _appPaths;

    public BackupCore(IConfiguration configuration, AppPaths appPaths)
    {
        _configuration = configuration;
        _appPaths = appPaths;
    }

    public async Task<(Stream stream, string fileName)> GenerarBackupBaseDeDatos()
    {
        var connectionString = _configuration.GetConnectionString("Default")!;
        var connBuilder = new SqlConnectionStringBuilder(connectionString);
        var dbName = connBuilder.InitialCatalog;

        var tempDir = _appPaths.CarpetaTemporalBackupBaseDeDatosAbsolute;
        Directory.CreateDirectory(tempDir);

        // DacFx exporta el esquema y datos leyendo desde SQL Server sobre la conexión de red.
        // El archivo .bacpac se escribe localmente en el servidor de la API,
        // por lo que funciona aunque SQL Server esté en Docker o en otra máquina.
        var fileName = $"BaseDeDatos-{FechaUtils.AhoraEnArgentinaFormatoBackup}.bacpac";
        var filePath = Path.Combine(tempDir, fileName);

        try
        {
            await Task.Run(() =>
            {
                var dacServices = new DacServices(connectionString);
                dacServices.ExportBacpac(filePath, dbName);
            });

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
            return (stream, fileName);
        }
        catch
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            throw;
        }
    }

    public async Task<(Stream stream, string fileName)> GenerarBackupImagenes()
    {
        var tempDir = _appPaths.CarpetaTemporalBackupBaseDeDatosAbsolute;
        Directory.CreateDirectory(tempDir);

        var zipFileName = $"Imagenes-{FechaUtils.AhoraEnArgentinaFormatoBackup}.zip";
        var zipPath = Path.Combine(tempDir, zipFileName);

        await Task.Run(() => ZipFile.CreateFromDirectory(_appPaths.ImagenesAbsolute, zipPath));

        var stream = new FileStream(zipPath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
        return (stream, zipFileName);
    }

    public async Task<string> GuardarBackupBaseDeDatosEnDisco()
    {
        var backupDir = Path.Combine(_appPaths.BackupAbsolute(), "backup");
        Directory.CreateDirectory(backupDir);

        var zipFileName = $"backup-bd-{FechaUtils.AhoraEnArgentinaFormatoBackupDisco}.zip";
        var zipPath = Path.Combine(backupDir, zipFileName);

        var connectionString = _configuration.GetConnectionString("Default")!;
        var connBuilder = new SqlConnectionStringBuilder(connectionString);
        var dbName = connBuilder.InitialCatalog;

        var tempDir = _appPaths.CarpetaTemporalBackupBaseDeDatosAbsolute;
        Directory.CreateDirectory(tempDir);
        var bacpacFileName = $"BaseDeDatos-{FechaUtils.AhoraEnArgentinaFormatoBackup}.bacpac";
        var bacpacPath = Path.Combine(tempDir, bacpacFileName);

        try
        {
            await Task.Run(() =>
            {
                var dacServices = new DacServices(connectionString);
                dacServices.ExportBacpac(bacpacPath, dbName);
            });

            await Task.Run(() =>
            {
                using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(bacpacPath, bacpacFileName);
                }
            });
        }
        finally
        {
            if (File.Exists(bacpacPath)) File.Delete(bacpacPath);
        }

        return zipPath;
    }

    public async Task<string> GuardarBackupImagenesEnDisco()
    {
        var backupDir = Path.Combine(_appPaths.BackupAbsolute(), "backup");
        Directory.CreateDirectory(backupDir);

        var zipFileName = $"backup-imagenes-{FechaUtils.AhoraEnArgentinaFormatoBackupDisco}.zip";
        var zipPath = Path.Combine(backupDir, zipFileName);

        await Task.Run(() => ZipFile.CreateFromDirectory(_appPaths.ImagenesAbsolute, zipPath));

        return zipPath;
    }
}
