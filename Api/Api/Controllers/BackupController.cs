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

    [HttpGet("subir-backup-bd-a-google-drive")]
    [AllowAnonymous]
    public async Task<IActionResult> SubirBackupBdAGoogleDrive()
    {
        var rutaArchivo = await _backupCore.GuardarBackupBaseDeDatosEnDisco();
        var nombreArchivo = Path.GetFileName(rutaArchivo);
        var idArchivoEnDrive = await _googleDriveCore.SubirArchivo(rutaArchivo, nombreArchivo);
        return Ok(new { idArchivoEnDrive });
    }

    [HttpGet("subir-backup-imagenes-a-google-drive")]
    [AllowAnonymous]
    public async Task<IActionResult> SubirBackupImagenesAGoogleDrive()
    {
        var rutaArchivo = await _backupCore.GuardarBackupImagenesEnDisco();
        var nombreArchivo = Path.GetFileName(rutaArchivo);
        var idArchivoEnDrive = await _googleDriveCore.SubirArchivo(rutaArchivo, nombreArchivo);
        return Ok(new { idArchivoEnDrive });
    }

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
