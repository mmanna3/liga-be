using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.Enums;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/Torneo/{padreId}/grupos-de-fases")]
[ModuloSistema(ModuloSistema.Torneos)]
public class GrupoDeFasesController : ABMControllerAnidado<GrupoDeFasesDTO, IGrupoDeFasesCore>
{
    public GrupoDeFasesController(IGrupoDeFasesCore core) : base(core)
    {
    }

    protected override void DespuesDeCrear(int padreId, GrupoDeFasesDTO dto)
    {
        dto.TorneoId = padreId;
    }

    /// <summary>Cambia solo la visibilidad del grupo de fases en la app (request con una sola propiedad).</summary>
    [HttpPut("{id}/visibilidad-en-app", Name = "gruposDeFasesCambiarVisibilidadEnApp")]
    public async Task<IActionResult> CambiarVisibilidadEnApp(
        int padreId,
        int id,
        [FromBody] CambiarVisibilidadEnAppDTO dto)
    {
        await Core.CambiarVisibilidadEnApp(padreId, id, dto.EsVisibleEnApp);
        return NoContent();
    }
}
