using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [AutorizarCualquierUsuarioAdministrativo]
    [ModuloSistema(ModuloSistema.Reportes)]
    public class ReporteController : ControllerBase
    {
        private readonly IReporteCore _reporteCore;

        public ReporteController(IReporteCore reporteCore)
        {
            _reporteCore = reporteCore;
        }
        
        [HttpGet("obtener-reporte-pagos")]
        public async Task<ActionResult<IEnumerable<ReportePagosDTO>>> ObtenerReportePagos([FromQuery] int? mes, [FromQuery] int? anio)
        {
            var reporte = await _reporteCore.ObtenerReportePagos(mes, anio);
            return Ok(reporte);
        }

        [HttpGet("obtener-reporte-fichajes-pagados-por-torneo")]
        public async Task<ActionResult<IEnumerable<ReporteFichajesPagadosPorAgrupadorDeTorneoDTO>>> ObtenerReporteFichajesPagadosPorTorneo([FromQuery] int anio)
        {
            var reporte = await _reporteCore.ObtenerReporteFichajesPagadosPorTorneo(anio);
            return Ok(reporte);
        }

        [HttpGet("obtener-reporte-jugadores-activos-por-torneo")]
        public async Task<ActionResult<IEnumerable<ReporteJugadoresActivosPorAgrupadorDeTorneoDTO>>> ObtenerReporteJugadoresActivosPorTorneo(
            [FromQuery] int anio,
            [FromQuery] bool mostrarEquipos = false)
        {
            var reporte = await _reporteCore.ObtenerReporteJugadoresActivosPorTorneo(anio, mostrarEquipos);
            return Ok(reporte);
        }
    }
} 