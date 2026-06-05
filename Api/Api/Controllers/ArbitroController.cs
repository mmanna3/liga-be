using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

public class ArbitroController : ABMController<ArbitroDTO, IArbitroCore, ArbitroDTO>
{
    public ArbitroController(IArbitroCore core) : base(core)
    {
    }

    [HttpGet("por-ids", Name = "arbitrosPorIds")]
    public async Task<ActionResult<IEnumerable<ArbitroDTO>>> ObtenerPorIds([FromQuery] IEnumerable<int> ids) =>
        await ObtenerPorIdsCore(ids);
}
