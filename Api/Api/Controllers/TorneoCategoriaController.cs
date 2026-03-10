using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/Torneo/{padreId}/categorias")]
public class TorneoCategoriaController : ABMControllerAnidado<TorneoCategoriaDTO, ITorneoCategoriaCore>
{
    public TorneoCategoriaController(ITorneoCategoriaCore core) : base(core)
    {
    }

    protected override void DespuesDeCrear(int padreId, TorneoCategoriaDTO dto)
    {
        dto.TorneoId = padreId;
    }
}
