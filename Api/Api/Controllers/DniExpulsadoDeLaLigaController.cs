using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

public class DniExpulsadoDeLaLigaController : ABMController<DniExpulsadoDeLaLigaDTO, IDniExpulsadoDeLaLigaCore, DniExpulsadoDeLaLigaDTO>
{
    public DniExpulsadoDeLaLigaController(IDniExpulsadoDeLaLigaCore core) : base(core)
    {
    }

    [HttpGet("por-ids", Name = "dnisExpulsadosDeLaLigaPorIds")]
    public async Task<ActionResult<IEnumerable<DniExpulsadoDeLaLigaDTO>>> ObtenerPorIds([FromQuery] IEnumerable<int> ids) =>
        await ObtenerPorIdsCore(ids);
}
