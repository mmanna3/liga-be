using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/Zona/{padreId}/leyendas")]
public class ZonaLeyendaController : ABMControllerAnidado<LeyendaTablaPosicionesDTO, ILeyendaTablaPosicionesCore>
{
    public ZonaLeyendaController(ILeyendaTablaPosicionesCore core) : base(core)
    {
    }

    protected override void DespuesDeCrear(int padreId, LeyendaTablaPosicionesDTO dto)
    {
        dto.ZonaId = padreId;
    }
}
