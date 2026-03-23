using Api.Core.Logica;
using Api.Core.Servicios.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.Core.Servicios;

public class GoogleDriveCore : IGoogleDriveCore
{
    private readonly AppPaths _appPaths;

    public GoogleDriveCore(AppPaths appPaths)
    {
        _appPaths = appPaths;
    }

    public async Task<string> SubirArchivo(string rutaArchivoLocal, string nombreArchivoEnDrive)
    {
        var credenciales = LeerCredenciales();
        var servicio = CrearServicio(credenciales);

        var metadatos = new Google.Apis.Drive.v3.Data.File
        {
            Name = nombreArchivoEnDrive,
            Parents = string.IsNullOrWhiteSpace(credenciales.IdCarpetaDestino) ? null : [credenciales.IdCarpetaDestino]
        };

        await using var stream = File.OpenRead(rutaArchivoLocal);

        var solicitud = servicio.Files.Create(metadatos, stream, "application/zip");
        solicitud.Fields = "id";
        solicitud.SupportsAllDrives = true;

        var resultado = await solicitud.UploadAsync();

        if (resultado.Status != Google.Apis.Upload.UploadStatus.Completed)
            throw new Exception($"Error al subir el archivo a Google Drive: {resultado.Exception?.Message}");

        return solicitud.ResponseBody.Id;
    }

    private CredencialesGoogleDrive LeerCredenciales()
    {
        var ruta = Path.Combine(_appPaths.BackupAbsolute(), "google-drive-credenciales.dat");

        if (!File.Exists(ruta))
            throw new FileNotFoundException($"No se encontró el archivo de credenciales de Google Drive en: {ruta}");

        var contenido = File.ReadAllText(ruta);
        var credenciales = JsonSerializer.Deserialize<CredencialesGoogleDrive>(contenido)
            ?? throw new Exception("El archivo de credenciales de Google Drive tiene un formato inválido.");

        if (string.IsNullOrWhiteSpace(credenciales.EmailCuentaServicio))
            throw new Exception("El campo 'email_cuenta_servicio' está vacío en las credenciales de Google Drive.");
        if (string.IsNullOrWhiteSpace(credenciales.ClavePrivada))
            throw new Exception("El campo 'clave_privada' está vacío en las credenciales de Google Drive.");
        return credenciales;
    }

    private static DriveService CrearServicio(CredencialesGoogleDrive credenciales)
    {
        var credencialCuenta = new ServiceAccountCredential(
            new ServiceAccountCredential.Initializer(credenciales.EmailCuentaServicio)
            {
                Scopes = [DriveService.Scope.Drive]
            }.FromPrivateKey(credenciales.ClavePrivada)
        );

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credencialCuenta,
            ApplicationName = "LigaBackup"
        });
    }

    private class CredencialesGoogleDrive
    {
        [JsonPropertyName("email_cuenta_servicio")]
        public string EmailCuentaServicio { get; set; } = "";

        [JsonPropertyName("clave_privada")]
        public string ClavePrivada { get; set; } = "";

        [JsonPropertyName("id_carpeta_destino")]
        public string IdCarpetaDestino { get; set; } = "";
    }
}
