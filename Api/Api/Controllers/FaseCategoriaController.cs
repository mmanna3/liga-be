using Api.Core.DTOs;
using Api.Core.Otros;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/Torneo/{torneoId}/fases/{faseId}/categorias")]
public class FaseCategoriaController : ControllerBase
{
    private readonly IFaseCategoriaCore _core;
    private readonly IFaseCore _faseCore;

    public FaseCategoriaController(IFaseCategoriaCore core, IFaseCore faseCore)
    {
        _core = core;
        _faseCore = faseCore;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FaseCategoriaDTO>>> Listar(int torneoId, int faseId)
    {
        await AsegurarFaseDelTorneo(torneoId, faseId);
        return Ok(await _core.ListarPorPadre(faseId));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FaseCategoriaDTO>> Obtener(int torneoId, int faseId, int id)
    {
        await AsegurarFaseDelTorneo(torneoId, faseId);
        var dto = await _core.ObtenerPorId(faseId, id);
        if (dto == null)
            return NotFound();
        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<FaseCategoriaDTO>> Crear(int torneoId, int faseId, FaseCategoriaDTO dto)
    {
        await AsegurarFaseDelTorneo(torneoId, faseId);
        var id = await _core.Crear(faseId, dto);
        await _core.ValidarCategoriasAnualSiAplica(faseId, torneoId);
        var creado = await _core.ObtenerPorId(faseId, id);
        if (creado == null)
            return NotFound();
        creado.FaseId = faseId;
        return Ok(creado);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FaseCategoriaDTO>> Modificar(int torneoId, int faseId, int id, FaseCategoriaDTO dto)
    {
        await AsegurarFaseDelTorneo(torneoId, faseId);
        await _core.Modificar(faseId, id, dto);
        await _core.ValidarCategoriasAnualSiAplica(faseId, torneoId);
        var actualizado = await _core.ObtenerPorId(faseId, id);
        if (actualizado == null)
            return NotFound();
        return Ok(actualizado);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int torneoId, int faseId, int id)
    {
        await AsegurarFaseDelTorneo(torneoId, faseId);
        var eliminado = await _core.Eliminar(faseId, id);
        if (eliminado < 0)
            return NotFound();
        await _core.ValidarCategoriasAnualSiAplica(faseId, torneoId);
        return NoContent();
    }

    private async Task AsegurarFaseDelTorneo(int torneoId, int faseId)
    {
        var fase = await _faseCore.ObtenerPorId(torneoId, faseId);
        if (fase == null)
            throw new ExcepcionControlada("La fase no pertenece al torneo indicado.");
    }
}
