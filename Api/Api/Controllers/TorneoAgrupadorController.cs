using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

public class TorneoAgrupadorController : ABMController<TorneoAgrupadorDTO, ITorneoAgrupadorCore, TorneoAgrupadorDTO>
{
    public TorneoAgrupadorController(ITorneoAgrupadorCore core) : base(core)
    {
    }

    [HttpGet("por-ids", Name = "torneoAgrupadoresPorIds")]
    [Authorize(Roles = "Administrador,Consulta")]
    public async Task<ActionResult<IEnumerable<TorneoAgrupadorDTO>>> ObtenerPorIds([FromQuery] IEnumerable<int> ids) =>
        await ObtenerPorIdsCore(ids);
}
