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

        [HttpGet("obtener-reporte-jugadores-habilitados-por-torneo")]
        public async Task<ActionResult<IEnumerable<ReporteJugadoresHabilitadosPorAgrupadorDeTorneoDTO>>> ObtenerReporteJugadoresHabilitadosPorTorneo([FromQuery] int anio)
        {
            var reporte = await _reporteCore.ObtenerReporteJugadoresHabilitadosPorTorneo(anio);
            return Ok(reporte);
        }
    }
} 