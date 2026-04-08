using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

public class ConfiguracionController : ABMController<ConfiguracionDTO, IConfiguracionCore, ConfiguracionDTO>
{
    public ConfiguracionController(IConfiguracionCore core) : base(core)
    {
    }

    [HttpGet("por-ids", Name = "configuracionesPorIds")]
    public async Task<ActionResult<IEnumerable<ConfiguracionDTO>>> ObtenerPorIds([FromQuery] IEnumerable<int> ids) =>
        await ObtenerPorIdsCore(ids);
}
