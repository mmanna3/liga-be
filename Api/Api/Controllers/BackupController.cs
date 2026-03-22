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

    public BackupController(IBackupCore backupCore)
    {
        _backupCore = backupCore;
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
}
