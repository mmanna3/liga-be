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

    /// <summary>Cambia solo la visibilidad del torneo en la app (request con una sola propiedad).</summary>
    [HttpPut("{id}/visibilidad-en-app", Name = "torneoCambiarVisibilidadEnApp")]
    public async Task<IActionResult> CambiarVisibilidadEnApp(
        int id,
        [FromBody] CambiarVisibilidadEnAppDTO dto)
    {
        await Core.CambiarVisibilidadEnApp(id, dto.EsVisibleEnApp);
        return NoContent();
    }

    [HttpPut("{id}/fases-para-tabla-anual", Name = "torneoEditarFasesParaTablaAnual")]
    public async Task<IActionResult> EditarFasesParaTablaAnual(int id, [FromBody] EditarFasesParaTablaAnualDTO dto)
    {
        await Core.EditarFasesParaTablaAnual(id, dto);
        return NoContent();
    }
}