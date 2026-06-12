using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/Torneo/{padreId}/grupos-de-fases")]
public class GrupoDeFasesController : ABMControllerAnidado<GrupoDeFasesDTO, IGrupoDeFasesCore>
{
    public GrupoDeFasesController(IGrupoDeFasesCore core) : base(core)
    {
    }

    protected override void DespuesDeCrear(int padreId, GrupoDeFasesDTO dto)
    {
        dto.TorneoId = padreId;
    }
}
