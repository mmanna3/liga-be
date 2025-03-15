using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface IReporteCore
{
    Task<IEnumerable<ReportePagosDTO>> ObtenerReportePagos(int? mes, int? anio);
} 