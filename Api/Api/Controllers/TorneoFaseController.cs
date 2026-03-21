using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/Torneo/{padreId}/fases")]
public class TorneoFaseController : ABMControllerAnidado<TorneoFaseDTO, ITorneoFaseCore>
{
    public TorneoFaseController(ITorneoFaseCore core) : base(core)
    {
    }

    [HttpPost]
    public override async Task<ActionResult<TorneoFaseDTO>> Crear(int padreId, TorneoFaseDTO dto)
    {
        var id = await Core.Crear(padreId, dto);
        var creado = await Core.ObtenerPorId(padreId, id);
        if (creado == null)
            return NotFound();
        return Ok(creado);
    }

    protected override void DespuesDeCrear(int padreId, TorneoFaseDTO dto)
    {
        dto.TorneoId = padreId;
    }
}
