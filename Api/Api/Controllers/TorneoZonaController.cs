using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/TorneoFase/{padreId}/zonas")]
public class TorneoZonaController : ABMControllerAnidado<TorneoZonaDTO, ITorneoZonaCore>
{
    public TorneoZonaController(ITorneoZonaCore core) : base(core)
    {
    }

    protected override void DespuesDeCrear(int padreId, TorneoZonaDTO dto)
    {
        dto.TorneoFaseId = padreId;
    }
}
