using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

public class TorneoAgrupadorController : ABMController<TorneoAgrupadorDTO, ITorneoAgrupadorCore, TorneoAgrupadorDTO>
{
    public TorneoAgrupadorController(ITorneoAgrupadorCore core) : base(core)
    {
    }

    [HttpPost]
    public override async Task<ActionResult<TorneoAgrupadorDTO>> Crear(TorneoAgrupadorDTO dto)
    {
        var id = await Core.Crear(dto);
        var creado = await Core.ObtenerPorId(id);
        return Ok(creado);
    }

    [HttpGet("por-ids", Name = "torneoAgrupadoresPorIds")]
    public async Task<ActionResult<IEnumerable<TorneoAgrupadorDTO>>> ObtenerPorIds([FromQuery] IEnumerable<int> ids) =>
        await ObtenerPorIdsCore(ids);
}
