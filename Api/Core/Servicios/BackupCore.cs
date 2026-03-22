using System.IO.Compression;
using System.Text.Json;
using Api.Core.Logica;
using Api.Core.Servicios.Interfaces;
using Microsoft.Data.SqlClient;

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
        var tempDir = _appPaths.CarpetaTemporalBackupBaseDeDatosAbsolute;
        Directory.CreateDirectory(tempDir);

        var jsonFileName = $"BaseDeDatos-{FechaUtils.AhoraEnArgentinaFormatoBackup}.json";
        var jsonPath = Path.Combine(tempDir, jsonFileName);
        var zipFileName = Path.ChangeExtension(jsonFileName, ".zip");
        var zipPath = Path.Combine(tempDir, zipFileName);

        try
        {
            await ExportarBaseDeDatosComoJson(jsonPath);

            await Task.Run(() =>
            {
                using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
                zip.CreateEntryFromFile(jsonPath, jsonFileName);
            });

            if (File.Exists(jsonPath)) File.Delete(jsonPath);

            var stream = new FileStream(zipPath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
            return (stream, zipFileName);
        }
        catch
        {
            if (File.Exists(jsonPath)) File.Delete(jsonPath);
            if (File.Exists(zipPath)) File.Delete(zipPath);
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

        var jsonFileName = $"BaseDeDatos-{FechaUtils.AhoraEnArgentinaFormatoBackup}.json";
        var jsonPath = Path.Combine(backupDir, jsonFileName);

        var zipFileName = $"backup-bd-{FechaUtils.AhoraEnArgentinaFormatoBackupDisco}.zip";
        var zipPath = Path.Combine(backupDir, zipFileName);

        try
        {
            await ExportarBaseDeDatosComoJson(jsonPath);

            await Task.Run(() =>
            {
                using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
                zip.CreateEntryFromFile(jsonPath, jsonFileName);
            });
        }
        finally
        {
            if (File.Exists(jsonPath)) File.Delete(jsonPath);
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

    private async Task ExportarBaseDeDatosComoJson(string rutaArchivo)
    {
        var connectionString = _configuration.GetConnectionString("Default")!;

        await Task.Run(() =>
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();

            var tablas = ObtenerNombresDeTablas(conn);
            var datos = new Dictionary<string, List<Dictionary<string, object?>>>();

            foreach (var tabla in tablas)
                datos[tabla] = ObtenerDatosDeTabla(conn, tabla);

            var opciones = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            File.WriteAllText(rutaArchivo, JsonSerializer.Serialize(datos, opciones));
        });
    }

    private static List<string> ObtenerNombresDeTablas(SqlConnection conn)
    {
        var tablas = new List<string>();
        using var cmd = new SqlCommand(
            "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME",
            conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            tablas.Add(reader.GetString(0));
        return tablas;
    }

    private static List<Dictionary<string, object?>> ObtenerDatosDeTabla(SqlConnection conn, string nombreTabla)
    {
        var filas = new List<Dictionary<string, object?>>();
        using var cmd = new SqlCommand($"SELECT * FROM [{nombreTabla.Replace("]", "")}] WITH (NOLOCK)", conn);
        cmd.CommandTimeout = 300;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var fila = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
                fila[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            filas.Add(fila);
        }
        return filas;
    }
}
