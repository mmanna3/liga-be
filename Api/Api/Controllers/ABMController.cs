using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrador")]
public abstract class ABMController<TDTO, TCore> : ControllerBase
    where TDTO : DTO
    where TCore : ICoreABM<TDTO>
{
    protected readonly TCore Core = default!;
    
    protected ABMController()
    {
    }
    
    protected ABMController(TCore core)
    {
        Core = core;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TDTO>>> Get()
    {
        var dto = await Core.Listar();
        return Ok(dto);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<TDTO>> Get(int id)
    {
        var dto = await Core.ObtenerPorId(id);

        return dto;
    }
    
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public virtual async Task<ActionResult<TDTO>> Crear(TDTO dto)
    {
        var id = await Core.Crear(dto);
        dto.Id = id;

        return Ok(dto);
    }
    
    // PUT: api/Servicio/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, TDTO dto)
    {
        if (id != dto.Id)
            return BadRequest();

        try
        {
            await Core.Modificar(id, dto);
        }
        catch (Exception e) 
        {
            if (e is DbUpdateConcurrencyException)
                return NotFound();
            throw;
        }

        return NoContent();
    }
}