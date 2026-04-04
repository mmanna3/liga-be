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
    public async Task<ActionResult<FechaEliminacionDirectaDTO>> CrearFechasEliminacionDirectaMasivamente(
        int padreId, [FromBody] FechaEliminacionDirectaDTO dto)
    {
        var creado = await Core.CrearFechasEliminacionDirectaMasivamente(padreId, dto);
        return Ok(creado);
    }

    /// <summary>
    /// Elimina todas las fechas de la zona (solo eliminación directa), con sus jornadas y partidos.
    /// </summary>
    [HttpDelete("borrar-fechas-eliminaciondirecta-masivamente")]
    public async Task<IActionResult> BorrarFechasEliminacionDirectaMasivamente(int padreId)
    {
        await Core.BorrarFechasEliminacionDirectaMasivamente(padreId);
        return NoContent();
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

    /// <summary>Cambia solo la visibilidad de la fecha en la app (request con una sola propiedad).</summary>
    [HttpPut("{id}/visibilidad-en-app", Name = "fechasCambiarVisibilidadEnApp")]
    public async Task<IActionResult> CambiarVisibilidadEnApp(
        int padreId,
        int id,
        [FromBody] CambiarVisibilidadEnAppDTO dto)
    {
        await Core.CambiarVisibilidadEnApp(padreId, id, dto.EsVisibleEnApp);
        return NoContent();
    }
}
