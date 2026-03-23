using System.IO.Compression;
using System.Text.Json;
using Api.Core.Logica;
using Api.Core.Otros;
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

    // Orden de inserción que respeta las dependencias de FK
    private static readonly string[] OrdenDeRestauracion =
    [
        "Clubs", "Jugadores", "Delegados", "TorneoAgrupadores", "FixtureAlgoritmos",
        "Equipos", "DelegadoClub", "Torneos", "Usuarios", "JugadorEquipo",
        "TorneoCategorias", "TorneoFases", "TorneoZonas", "HistorialDePagos",
        "EquipoZona", "TorneoFechas", "FixtureAlgoritmoFecha", "Jornadas"
    ];

    // Tablas que ya tienen datos de seed tras una migración limpia:
    // se borran antes de reinsertar para lograr una restauración completa y fiel al backup.
    private static readonly HashSet<string> TablasConDatosSemilla =
    [
        "TorneoAgrupadores", "FixtureAlgoritmos", "Usuarios"
    ];

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

    public async Task RestaurarDesdeBackup()
    {
        var rutaJson = Path.Combine(_appPaths.BackupAbsolute(), "backup-bd.json");

        if (!File.Exists(rutaJson))
            throw new ExcepcionControlada($"No se encontró el archivo 'backup-bd.json' en App_Data.");

        var connectionString = _configuration.GetConnectionString("Default")!;

        var hayClubs = await Task.Run(() =>
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            using var cmd = new SqlCommand("SELECT COUNT(1) FROM [Clubs]", conn);
            return (int)cmd.ExecuteScalar()! > 0;
        });

        if (hayClubs)
            throw new ExcepcionControlada("La base de datos ya tiene datos. Sólo se puede restaurar sobre una base de datos vacía.");

        var json = await File.ReadAllTextAsync(rutaJson);
        var datos = JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, JsonElement>>>>(json)
            ?? throw new ExcepcionControlada("El archivo 'backup-bd.json' tiene un formato inválido.");

        await Task.Run(() =>
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            using var transaccion = conn.BeginTransaction();

            try
            {
                foreach (var tabla in OrdenDeRestauracion)
                {
                    if (!datos.TryGetValue(tabla, out var filas) || filas.Count == 0) continue;

                    if (TablasConDatosSemilla.Contains(tabla))
                        EjecutarComando($"DELETE FROM [{tabla}]", conn, transaccion);

                    RestaurarTabla(conn, transaccion, tabla, filas);
                }

                transaccion.Commit();
            }
            catch
            {
                transaccion.Rollback();
                throw;
            }
        });

        if (File.Exists(rutaJson))
            File.Delete(rutaJson);
    }

    public void ValidarCantidadArchivosEnCarpetaBackup()
    {
        var carpetaBackup = Path.Combine(_appPaths.BackupAbsolute(), "backup");
        if (!Directory.Exists(carpetaBackup))
            return;

        var cantidadArchivos = Directory.GetFiles(carpetaBackup).Length;
        if (cantidadArchivos > 4)
            throw new ExcepcionControlada(
                $"La carpeta App_Data/backup tiene {cantidadArchivos} archivos. No puede haber más de 4. Por favor, eliminá los backups antiguos antes de generar uno nuevo.");
    }

    public string ObtenerRutaBackupBdEnDisco()
    {
        var carpetaBackup = Path.Combine(_appPaths.BackupAbsolute(), "backup");
        var archivo = Directory.GetFiles(carpetaBackup, "backup-bd-*.zip")
            .OrderByDescending(f => f)
            .FirstOrDefault()
            ?? throw new ExcepcionControlada("No se encontró ningún backup de BD en App_Data/backup.");
        return archivo;
    }

    public string ObtenerRutaBackupImagenesEnDisco()
    {
        var carpetaBackup = Path.Combine(_appPaths.BackupAbsolute(), "backup");
        var archivo = Directory.GetFiles(carpetaBackup, "backup-imagenes-*.zip")
            .OrderByDescending(f => f)
            .FirstOrDefault()
            ?? throw new ExcepcionControlada("No se encontró ningún backup de imágenes en App_Data/backup.");
        return archivo;
    }

    public void LimpiarBackupsLocales()
    {
        var carpetaBackup = Path.Combine(_appPaths.BackupAbsolute(), "backup");
        if (!Directory.Exists(carpetaBackup)) return;
        foreach (var archivo in Directory.GetFiles(carpetaBackup, "*.zip"))
            File.Delete(archivo);
    }

    public async Task RestaurarImagenesDesdeBackup()
    {
        var rutaZip = Path.Combine(_appPaths.BackupAbsolute(), "backup-imagenes.zip");

        if (!File.Exists(rutaZip))
            throw new ExcepcionControlada("No se encontró el archivo 'backup-imagenes.zip' en App_Data.");

        await Task.Run(() =>
        {
            var carpetaImagenes = _appPaths.ImagenesAbsolute;

            if (Directory.Exists(carpetaImagenes))
                Directory.Delete(carpetaImagenes, recursive: true);

            Directory.CreateDirectory(carpetaImagenes);
            ZipFile.ExtractToDirectory(rutaZip, carpetaImagenes, overwriteFiles: true);
        });

        if (File.Exists(rutaZip))
            File.Delete(rutaZip);
    }

    private static void RestaurarTabla(
        SqlConnection conn, SqlTransaction transaccion,
        string tabla, List<Dictionary<string, JsonElement>> filas)
    {
        var columnas = filas[0].Keys.ToList();
        var tieneIdentity = false;

        try
        {
            EjecutarComando($"SET IDENTITY_INSERT [{tabla}] ON", conn, transaccion);
            tieneIdentity = true;
        }
        catch { /* tabla sin columna identity */ }

        try
        {
            var cols = string.Join(", ", columnas.Select(c => $"[{c}]"));
            var parms = string.Join(", ", columnas.Select((_, i) => $"@p{i}"));
            var sql = $"INSERT INTO [{tabla}] ({cols}) VALUES ({parms})";

            foreach (var fila in filas)
            {
                using var cmd = new SqlCommand(sql, conn, transaccion);
                cmd.CommandTimeout = 300;
                for (var i = 0; i < columnas.Count; i++)
                    cmd.Parameters.AddWithValue($"@p{i}", ConvertirValorParaDb(fila[columnas[i]]));
                cmd.ExecuteNonQuery();
            }
        }
        finally
        {
            if (tieneIdentity)
                EjecutarComando($"SET IDENTITY_INSERT [{tabla}] OFF", conn, transaccion);
        }
    }

    private static void EjecutarComando(string sql, SqlConnection conn, SqlTransaction transaccion)
    {
        using var cmd = new SqlCommand(sql, conn, transaccion);
        cmd.CommandTimeout = 60;
        cmd.ExecuteNonQuery();
    }

    private static object ConvertirValorParaDb(JsonElement elemento) =>
        elemento.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => DBNull.Value,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => elemento.TryGetInt64(out var l) ? (object)l
                : elemento.TryGetDecimal(out var d) ? (object)d
                : elemento.GetDouble(),
            JsonValueKind.String => (object?)elemento.GetString() ?? DBNull.Value,
            _ => elemento.GetRawText()
        };

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
