using Api.Core.Logica;
using Api.Core.Servicios.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;

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

        if (string.IsNullOrWhiteSpace(credenciales.ClientId))
            throw new Exception("El campo 'client_id' está vacío en las credenciales de Google Drive.");
        if (string.IsNullOrWhiteSpace(credenciales.ClientSecret))
            throw new Exception("El campo 'client_secret' está vacío en las credenciales de Google Drive.");
        if (string.IsNullOrWhiteSpace(credenciales.RefreshToken))
            throw new Exception("El campo 'refresh_token' está vacío en las credenciales de Google Drive.");

        return credenciales;
    }

    public async Task<RotacionBackupsDriveResult> RotarBackupsEnDrive()
    {
        var credenciales = LeerCredenciales();
        if (string.IsNullOrWhiteSpace(credenciales.IdCarpetaDestino))
            throw new Exception("'id_carpeta_destino' está vacío en las credenciales de Google Drive.");

        var servicio = CrearServicio(credenciales);

        var listRequest = servicio.Files.List();
        listRequest.Q = $"'{credenciales.IdCarpetaDestino}' in parents and trashed = false";
        listRequest.Fields = "files(id, name)";
        var resultado = await listRequest.ExecuteAsync();

        var archivos = resultado.Files
            .Where(f => f.Id != null && f.Name != null)
            .Select(f => (f.Id!, f.Name!))
            .ToList();

        var plan = GoogleDriveBackupRotacion.Calcular(archivos);

        foreach (var id in plan.IdsABorrar)
            await servicio.Files.Delete(id).ExecuteAsync();

        return plan;
    }

    public string ObtenerUrlDeAutorizacion(string redirectUri)
    {
        var credenciales = LeerCredenciales();
        var flow = CrearFlow(credenciales);
        return flow.CreateAuthorizationCodeRequest(redirectUri).Build().ToString();
    }

    public async Task GuardarRefreshToken(string code, string redirectUri)
    {
        var credenciales = LeerCredenciales();
        var flow = CrearFlow(credenciales);
        var token = await flow.ExchangeCodeForTokenAsync("user", code, redirectUri, CancellationToken.None);

        credenciales.RefreshToken = token.RefreshToken;

        var ruta = Path.Combine(_appPaths.BackupAbsolute(), "google-drive-credenciales.dat");
        var opciones = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        await File.WriteAllTextAsync(ruta, JsonSerializer.Serialize(credenciales, opciones));
    }

    private static GoogleAuthorizationCodeFlow CrearFlow(CredencialesGoogleDrive credenciales) =>
        new(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = credenciales.ClientId,
                ClientSecret = credenciales.ClientSecret
            },
            Scopes = [DriveService.Scope.Drive]
        });

    private static DriveService CrearServicio(CredencialesGoogleDrive credenciales)
    {
        var flow = CrearFlow(credenciales);
        var tokenResponse = new TokenResponse { RefreshToken = credenciales.RefreshToken };
        var userCredential = new UserCredential(flow, "user", tokenResponse);

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = userCredential,
            ApplicationName = "LigaBackup"
        });
    }

    private class CredencialesGoogleDrive
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; } = "";

        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; } = "";

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = "";

        [JsonPropertyName("id_carpeta_destino")]
        public string IdCarpetaDestino { get; set; } = "";
    }
}
