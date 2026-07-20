using Api.Core.DTOs;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;

namespace Api.Core.Servicios;

public class ReporteCore : IReporteCore
{
    private readonly IHistorialDePagosRepo _historialDePagosRepo;
    private readonly IJugadorRepo _jugadorRepo;

    public ReporteCore(IHistorialDePagosRepo historialDePagosRepo, IJugadorRepo jugadorRepo)
    {
        _historialDePagosRepo = historialDePagosRepo;
        _jugadorRepo = jugadorRepo;
    }

    public async Task<IEnumerable<ReportePagosDTO>> ObtenerReportePagos(int? mes, int? anio)
    {
        return await _historialDePagosRepo.ObtenerPagosPorMesYEquipo(mes, anio);
    }

    public async Task<IEnumerable<ReporteFichajesPagadosPorAgrupadorDeTorneoDTO>> ObtenerReporteFichajesPagadosPorTorneo(int anio)
    {
        return await _historialDePagosRepo.ObtenerFichajesPagadosPorAgrupadorDeTorneo(anio);
    }

    public async Task<IEnumerable<ReporteJugadoresActivosPorAgrupadorDeTorneoDTO>> ObtenerReporteJugadoresActivosPorTorneo(int anio, bool mostrarEquipos)
    {
        return await _jugadorRepo.ObtenerJugadoresActivosPorAgrupadorDeTorneo(anio, mostrarEquipos);
    }
}
