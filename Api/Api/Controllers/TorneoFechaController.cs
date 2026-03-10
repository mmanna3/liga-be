using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/TorneoZona/{padreId}/fechas")]
public class TorneoFechaController : ABMControllerAnidado<TorneoFechaDTO, ITorneoFechaCore>
{
    public TorneoFechaController(ITorneoFechaCore core) : base(core)
    {
    }

    protected override void DespuesDeCrear(int padreId, TorneoFechaDTO dto)
    {
        dto.ZonaId = padreId;
    }
}
