using Api.Api.Authorization;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BackupController : ControllerBase
{
    private readonly IBackupCore _backupCore;
    private readonly IGoogleDriveCore _googleDriveCore;

    public BackupController(IBackupCore backupCore, IGoogleDriveCore googleDriveCore)
    {
        _backupCore = backupCore;
        _googleDriveCore = googleDriveCore;
    }

    [HttpGet("generar-backup-base-de-datos")]
    public async Task<IActionResult> GenerarBackupBaseDeDatos()
    {
        var (stream, fileName) = await _backupCore.GenerarBackupBaseDeDatos();
        return File(stream, "application/zip", fileName);
    }

    [HttpGet("generar-backup-imagenes")]
    public async Task<IActionResult> GenerarBackupImagenes()
    {
        var (stream, fileName) = await _backupCore.GenerarBackupImagenes();
        return File(stream, "application/zip", fileName);
    }

    [HttpGet("guardar-backup-bd-en-disco")]
    [AllowAnonymous]
    public async Task<IActionResult> GuardarBackupBdEnDisco()
    {
        var ruta = await _backupCore.GuardarBackupBaseDeDatosEnDisco();
        return Ok(new { ruta });
    }

    [HttpGet("guardar-backup-imagenes-en-disco")]
    [AllowAnonymous]
    public async Task<IActionResult> GuardarBackupImagenesEnDisco()
    {
        var ruta = await _backupCore.GuardarBackupImagenesEnDisco();
        return Ok(new { ruta });
    }

    [HttpGet("generar-backup-y-subirlo-a-drive")]
    [AllowAnonymous]
    public async Task<IActionResult> GenerarBackupYSubirloADrive()
    {
        _backupCore.ValidarCantidadArchivosEnCarpetaBackup();

        var rutaBd = await _backupCore.GuardarBackupBaseDeDatosEnDisco();
        var rutaImagenes = await _backupCore.GuardarBackupImagenesEnDisco();

        var idBdEnDrive = await _googleDriveCore.SubirArchivo(rutaBd, Path.GetFileName(rutaBd));
        var idImagenesEnDrive = await _googleDriveCore.SubirArchivo(rutaImagenes, Path.GetFileName(rutaImagenes));

        if (System.IO.File.Exists(rutaBd))
            System.IO.File.Delete(rutaBd);
        if (System.IO.File.Exists(rutaImagenes))
            System.IO.File.Delete(rutaImagenes);

        return Ok(new { idBdEnDrive = idBdEnDrive, idImagenesEnDrive = idImagenesEnDrive });
    }

    // Comentado porque SOLO FUE NECESARIO UNA SOLA VEZ.
    // 
    // [HttpGet("google-drive-autorizar")]
    // [AllowAnonymous]
    // public IActionResult GoogleDriveAutorizar()
    // {
    //     var redirectUri = $"{Request.Scheme}://{Request.Host}/api/backup/google-drive-callback";
    //     var url = _googleDriveCore.ObtenerUrlDeAutorizacion(redirectUri);
    //     return Redirect(url);
    // }

    // [HttpGet("google-drive-callback")]
    // [AllowAnonymous]
    // public async Task<IActionResult> GoogleDriveCallback([FromQuery] string code)
    // {
    //     var redirectUri = $"{Request.Scheme}://{Request.Host}/api/backup/google-drive-callback";
    //     await _googleDriveCore.GuardarRefreshToken(code, redirectUri);
    //     return Ok("Refresh token guardado correctamente. Ya podés usar el backup.");
    // }

    [HttpPost("restaurar-bd-desde-backup")]
    [AutorizarSoloSuperAdmin]
    public async Task<IActionResult> RestaurarBdDesdeBackup()
    {
        await _backupCore.RestaurarDesdeBackup();
        return Ok();
    }

    [HttpPost("restaurar-imagenes-desde-backup")]
    [AutorizarSoloSuperAdmin]
    public async Task<IActionResult> RestaurarImagenesDesdeBackup()
    {
        await _backupCore.RestaurarImagenesDesdeBackup();
        return Ok();
    }
}
