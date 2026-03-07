using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

public class TorneoController : ABMController<TorneoDTO, ITorneoCore>
{
    public TorneoController(ITorneoCore core) : base(core)
    {
    }

    [HttpGet("por-ids", Name = "torneosPorIds")]
    [Authorize(Roles = "Administrador,Consulta")]
    public async Task<ActionResult<IEnumerable<TorneoDTO>>> ObtenerPorIds([FromQuery] IEnumerable<int> ids) =>
        await ObtenerPorIdsCore(ids);
}