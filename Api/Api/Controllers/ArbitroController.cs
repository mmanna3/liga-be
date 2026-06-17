using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[ModuloSistema(ModuloSistema.Arbitros)]
public class ArbitroController : ABMController<ArbitroDTO, IArbitroCore, ArbitroDTO>
{
    private readonly IArbitroAsignacionCore _asignacionCore;

    public ArbitroController(IArbitroCore core, IArbitroAsignacionCore asignacionCore) : base(core)
    {
        _asignacionCore = asignacionCore;
    }

    [HttpGet("por-ids", Name = "arbitrosPorIds")]
    public async Task<ActionResult<IEnumerable<ArbitroDTO>>> ObtenerPorIds([FromQuery] IEnumerable<int> ids) =>
        await ObtenerPorIdsCore(ids);

    [HttpGet("asignacion-por-agrupador", Name = "arbitroAsignacionPorAgrupador")]
    public async Task<ActionResult<AsignacionArbitrosPorAgrupadorDTO>> ObtenerAsignacionPorAgrupador(
        [FromQuery] int agrupadorId,
        [FromQuery] int anio)
    {
        try
        {
            return Ok(await _asignacionCore.ObtenerAsignacionPorAgrupador(agrupadorId, anio));
        }
        catch (ExcepcionControlada ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("jornada/{jornadaId}/arbitros", Name = "asignarArbitrosJornada")]
    public async Task<IActionResult> AsignarArbitrosAJornada(int jornadaId, [FromBody] AsignarArbitrosJornadaDTO dto)
    {
        try
        {
            await _asignacionCore.AsignarArbitrosAJornada(jornadaId, dto);
            return NoContent();
        }
        catch (ExcepcionControlada ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("jornada/{jornadaId}/arbitros/{arbitroId}/whatsapp-enviado", Name = "marcarWhatsappEnviadoArbitroJornada")]
    public async Task<IActionResult> MarcarWhatsappEnviado(int jornadaId, int arbitroId)
    {
        try
        {
            await _asignacionCore.MarcarWhatsappEnviado(jornadaId, arbitroId);
            return NoContent();
        }
        catch (ExcepcionControlada ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
