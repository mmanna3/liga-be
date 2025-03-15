using Api.Core.DTOs;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;

namespace Api.Core.Servicios;

public class ReporteCore : IReporteCore
{
    private readonly IHistorialDePagosRepo _historialDePagosRepo;

    public ReporteCore(IHistorialDePagosRepo historialDePagosRepo)
    {
        _historialDePagosRepo = historialDePagosRepo;
    }

    public async Task<IEnumerable<ReportePagosDTO>> ObtenerReportePagos(int? mes, int? anio)
    {
        return await _historialDePagosRepo.ObtenerPagosPorMesYEquipo(mes, anio);
    }
} 