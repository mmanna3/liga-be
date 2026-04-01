using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/Fase/{padreId}/zonas")]
public class ZonaController : ABMControllerAnidado<ZonaDTO, IZonaCore>
{
    public ZonaController(IZonaCore core) : base(core)
    {
    }

    protected override void DespuesDeCrear(int padreId, ZonaDTO dto)
    {
        dto.FaseId = padreId;
    }

    [HttpPost("crear-zonas-masivamente")]
    public async Task<ActionResult<IEnumerable<ZonaDTO>>> CrearMasivamente(int padreId, [FromBody] List<ZonaDTO> dtos)
    {
        var creados = await Core.CrearMasivamente(padreId, dtos);
        return Ok(creados);
    }

    [HttpPut("modificar-zonas-masivamente")]
    public async Task<IActionResult> ModificarMasivamente(int padreId, [FromBody] List<ZonaDTO> dtos)
    {
        await Core.ModificarMasivamente(padreId, dtos);
        return NoContent();
    }

}
