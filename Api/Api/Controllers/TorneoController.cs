using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

public class TorneoController : ABMController<TorneoDTO, ITorneoCore, CrearTorneoDTO>
{
    public TorneoController(ITorneoCore core) : base(core)
    {
    }

    [HttpGet("por-ids", Name = "torneosPorIds")]
    public async Task<ActionResult<IEnumerable<TorneoDTO>>> ObtenerPorIds([FromQuery] IEnumerable<int> ids) =>
        await ObtenerPorIdsCore(ids);

    [HttpGet("filtrar", Name = "torneosFiltrar")]
    public async Task<ActionResult<IEnumerable<TorneoDTO>>> Filtrar([FromQuery] int? anio, [FromQuery] int? agrupador)
    {
        var resultado = await Core.Filtrar(anio, agrupador);
        return Ok(resultado);
    }
}