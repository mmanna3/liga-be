using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/Zona/{padreId}/fechas")]
public class FechaController : ABMControllerAnidado<FechaDTO, IFechaCore>
{
    public FechaController(IFechaCore core) : base(core)
    {
    }

    protected override void DespuesDeCrear(int padreId, FechaDTO dto)
    {
        dto.ZonaId = padreId;
    }

    [HttpPost("crear-fechas-todoscontratodos-masivamente")]
    public async Task<ActionResult<IEnumerable<FechaTodosContraTodosDTO>>> CrearFechasTodosContraTodosMasivamente(
        int padreId, [FromBody] List<FechaTodosContraTodosDTO> dtos)
    {
        var creados = await Core.CrearFechasTodosContraTodosMasivamente(padreId, dtos);
        return Ok(creados);
    }

    [HttpPost("crear-fechas-eliminaciondirecta-masivamente")]
    public async Task<ActionResult<IEnumerable<FechaEliminacionDirectaDTO>>> CrearFechasEliminacionDirectaMasivamente(
        int padreId, [FromBody] List<FechaEliminacionDirectaDTO> dtos)
    {
        var creados = await Core.CrearFechasEliminacionDirectaMasivamente(padreId, dtos);
        return Ok(creados);
    }

    [HttpPut("modificar-fechas-masivamente")]
    public async Task<IActionResult> ModificarMasivamente(int padreId, [FromBody] List<FechaDTO> dtos)
    {
        await Core.ModificarMasivamente(padreId, dtos);
        return NoContent();
    }

    /// <summary>
    /// Actualiza los resultados (local y visitante) de todos los partidos de una jornada y el flag ResultadosVerificados.
    /// </summary>
    [HttpPost("cargar-resultados/{jornadaId}")]
    public async Task<IActionResult> CargarResultados(int padreId, int jornadaId, [FromBody] CargarResultadosDTO dto)
    {
        await Core.CargarResultados(padreId, jornadaId, dto);
        return NoContent();
    }
}
