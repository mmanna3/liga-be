using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.Enums;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/Torneo/{padreId}/categorias")]
[ModuloSistema(ModuloSistema.Torneos)]
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
