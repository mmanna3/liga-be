using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/TorneoZona/{padreId}/fechas")]
public class TorneoFechaController : ABMControllerAnidado<TorneoFechaDTO, ITorneoFechaCore>
{
    public TorneoFechaController(ITorneoFechaCore core) : base(core)
    {
    }

    protected override void DespuesDeCrear(int padreId, TorneoFechaDTO dto)
    {
        dto.ZonaId = padreId;
    }

    [HttpPost("crear-fechas-masivamente")]
    public async Task<ActionResult<IEnumerable<TorneoFechaDTO>>> CrearMasivamente(int padreId, [FromBody] List<TorneoFechaDTO> dtos)
    {
        var creados = await Core.CrearMasivamente(padreId, dtos);
        return Ok(creados);
    }

    [HttpPut("modificar-fechas-masivamente")]
    public async Task<IActionResult> ModificarMasivamente(int padreId, [FromBody] List<TorneoFechaDTO> dtos)
    {
        await Core.ModificarMasivamente(padreId, dtos);
        return NoContent();
    }
}
