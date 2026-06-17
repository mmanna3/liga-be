using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.Enums;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[ModuloSistema(ModuloSistema.Configuracion)]
public class SponsorWebPublicaController
    : ABMController<SponsorWebPublicaDTO, ISponsorWebPublicaCore, CrearSponsorWebPublicaDTO>
{
    public SponsorWebPublicaController(ISponsorWebPublicaCore core) : base(core)
    {
    }

    [HttpPost]
    public override async Task<ActionResult<SponsorWebPublicaDTO>> Crear(CrearSponsorWebPublicaDTO dto)
    {
        var id = await Core.CrearConImagen(dto);
        var creado = await Core.ObtenerPorId(id);
        return Ok(creado);
    }
}
