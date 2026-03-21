using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/TorneoFase/{padreId}/zonas")]
public class TorneoZonaController : ABMControllerAnidado<TorneoZonaDTO, ITorneoZonaCore>
{
    public TorneoZonaController(ITorneoZonaCore core) : base(core)
    {
    }

    protected override void DespuesDeCrear(int padreId, TorneoZonaDTO dto)
    {
        dto.TorneoFaseId = padreId;
    }

    [HttpPost("crear-zonas-masivamente")]
    public async Task<ActionResult<IEnumerable<TorneoZonaDTO>>> CrearMasivamente(int padreId, [FromBody] List<TorneoZonaDTO> dtos)
    {
        var creados = await Core.CrearMasivamente(padreId, dtos);
        return Ok(creados);
    }

    [HttpPut("modificar-zonas-masivamente")]
    public async Task<IActionResult> ModificarMasivamente(int padreId, [FromBody] List<TorneoZonaDTO> dtos)
    {
        await Core.ModificarMasivamente(padreId, dtos);
        return NoContent();
    }

}
