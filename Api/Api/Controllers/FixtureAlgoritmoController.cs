using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

public class FixtureAlgoritmoController : ABMController<FixtureAlgoritmoDTO, IFixtureAlgoritmoCore, FixtureAlgoritmoDTO>
{
    public FixtureAlgoritmoController(IFixtureAlgoritmoCore core) : base(core)
    {
    }

    public override async Task<ActionResult<FixtureAlgoritmoDTO>> Crear(FixtureAlgoritmoDTO dto)
    {
        var result = await base.Crear(dto);
        if (result.Value != null)
            result.Value.FixtureAlgoritmoId = result.Value.Id;
        return result;
    }

    [HttpGet("por-ids", Name = "fixtureAlgoritmosPorIds")]
    public async Task<ActionResult<IEnumerable<FixtureAlgoritmoDTO>>> ObtenerPorIds([FromQuery] IEnumerable<int> ids) =>
        await ObtenerPorIdsCore(ids);
}
