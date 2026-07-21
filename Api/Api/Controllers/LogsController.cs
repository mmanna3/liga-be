using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[AutorizarSoloSuperAdmin]
public class LogsController : ControllerBase
{
    private readonly ILogsCore _logsCore;

    public LogsController(ILogsCore logsCore)
    {
        _logsCore = logsCore;
    }

    [HttpGet("buscar", Name = "logsBuscar")]
    public ActionResult<BusquedaLogsDTO> Buscar(
        [FromQuery] string texto,
        [FromQuery] int dias = 14,
        [FromQuery] int maxResultados = 200)
    {
        return Ok(_logsCore.Buscar(texto, dias, maxResultados));
    }

    [HttpGet("archivos", Name = "logsArchivos")]
    public ActionResult<IReadOnlyList<LogArchivoDTO>> Archivos([FromQuery] int? dias = null)
    {
        return Ok(_logsCore.ListarArchivos(dias));
    }
}
