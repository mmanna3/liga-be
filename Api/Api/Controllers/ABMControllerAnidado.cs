using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Api.Controllers;

/// <summary>
/// Controller base para entidades que siempre dependen de un padre (sub-recursos).
/// La ruta debe incluir el segmento del padre, ej: [Route("api/Torneo/{padreId}/categorias")].
/// </summary>
[ApiController]
[AutorizarCualquierUsuarioAdministrativo]
public abstract class ABMControllerAnidado<TDTO, TCore> : ControllerBase
    where TDTO : DTO
    where TCore : ICoreABMAnidado<int, TDTO>
{
    protected readonly TCore Core = default!;

    protected ABMControllerAnidado()
    {
    }

    protected ABMControllerAnidado(TCore core)
    {
        Core = core;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TDTO>>> Get(int padreId)
    {
        var dtos = await Core.ListarPorPadre(padreId);
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TDTO>> Get(int padreId, int id)
    {
        var dto = await Core.ObtenerPorId(padreId, id);
        if (dto == null)
            return NotFound();
        return Ok(dto);
    }

    [HttpPost]
    public virtual async Task<ActionResult<TDTO>> Crear(int padreId, TDTO dto)
    {
        var id = await Core.Crear(padreId, dto);
        dto.Id = id;
        DespuesDeCrear(padreId, dto);
        return Ok(dto);
    }

    /// <summary>
    /// Override para establecer el padreId en el DTO antes de devolverlo (ej: dto.TorneoId = padreId).
    /// </summary>
    protected virtual void DespuesDeCrear(int padreId, TDTO dto)
    {
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int padreId, int id, TDTO dto)
    {
        if (id != dto.Id)
            return BadRequest();

        try
        {
            await Core.Modificar(padreId, id, dto);
        }
        catch (Exception e)
        {
            if (e is DbUpdateConcurrencyException)
                return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [AutorizarSoloAdmin]
    public async Task<ActionResult<int>> Eliminar(int padreId, int id)
    {
        var resultado = await Core.Eliminar(padreId, id);
        if (resultado == -1)
            return NotFound();
        return Ok(resultado);
    }
}
