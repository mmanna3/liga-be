using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.Enums;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[ModuloSistema(ModuloSistema.Jugadores)]
public class DniExpulsadoDeLaLigaController : ABMController<DniExpulsadoDeLaLigaDTO, IDniExpulsadoDeLaLigaCore, DniExpulsadoDeLaLigaDTO>
{
    public DniExpulsadoDeLaLigaController(IDniExpulsadoDeLaLigaCore core) : base(core)
    {
    }

    [HttpGet("por-ids", Name = "dnisExpulsadosDeLaLigaPorIds")]
    public async Task<ActionResult<IEnumerable<DniExpulsadoDeLaLigaDTO>>> ObtenerPorIds([FromQuery] IEnumerable<int> ids) =>
        await ObtenerPorIdsCore(ids);
}
