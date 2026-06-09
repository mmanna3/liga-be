using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[AutorizarSoloAdmin]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioCore _core;

    public UsuarioController(IUsuarioCore core)
    {
        _core = core;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioAdminDTO>>> Get()
    {
        var dtos = await _core.Listar();
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioAdminDTO>> Get(int id)
    {
        var dto = await _core.ObtenerPorId(id);
        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioAdminDTO>> Crear(UsuarioAdminDTO dto)
    {
        var id = await _core.Crear(dto);
        var creado = await _core.ObtenerPorId(id);
        return Ok(creado);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, UsuarioAdminDTO dto)
    {
        if (id != dto.Id)
            return BadRequest();

        try
        {
            await _core.Modificar(id, dto);
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
    public async Task<ActionResult<int>> Eliminar(int id)
    {
        var resultado = await _core.Eliminar(id);
        if (resultado == -1)
            return NotFound();
        return Ok(resultado);
    }

    [HttpPost("blanquear-clave")]
    public async Task<ActionResult<bool>> BlanquearClave(int id)
    {
        var resultado = await _core.BlanquearClave(id);
        if (!resultado)
            return NotFound();
        return Ok(resultado);
    }

    [HttpGet("roles-asignables")]
    public async Task<ActionResult<IEnumerable<RolDTO>>> RolesAsignables()
    {
        var roles = await _core.ListarRolesAsignables();
        return Ok(roles);
    }
}
