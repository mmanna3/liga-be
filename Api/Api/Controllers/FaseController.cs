using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/Torneo/{padreId}/fases")]
public class FaseController : ABMControllerAnidado<FaseDTO, IFaseCore>
{
    public FaseController(IFaseCore core) : base(core)
    {
    }

    [HttpPost]
    public override async Task<ActionResult<FaseDTO>> Crear(int padreId, FaseDTO dto)
    {
        var id = await Core.Crear(padreId, dto);
        var creado = await Core.ObtenerPorId(padreId, id);
        if (creado == null)
            return NotFound();
        return Ok(creado);
    }

    protected override void DespuesDeCrear(int padreId, FaseDTO dto)
    {
        dto.TorneoId = padreId;
    }

    /// <summary>Cambia solo la visibilidad de la fase en la app (request con una sola propiedad).</summary>
    [HttpPut("{id}/visibilidad-en-app", Name = "fasesCambiarVisibilidadEnApp")]
    public async Task<IActionResult> CambiarVisibilidadEnApp(
        int padreId,
        int id,
        [FromBody] CambiarVisibilidadEnAppDTO dto)
    {
        await Core.CambiarVisibilidadEnApp(padreId, id, dto.EsVisibleEnApp);
        return NoContent();
    }
}
