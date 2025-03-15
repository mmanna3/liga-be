using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
    }
} 