using Api.Core.DTOs;
using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IHistorialDePagosRepo
{
    Task RegistrarPago(int jugadorEquipoId);
    Task<IEnumerable<ReportePagosDTO>> ObtenerPagosPorMesYEquipo(int? mes, int? anio);
    HistorialDePagos? ObtenerPagoPorJugadorEquipoId(int jugadorEquipoId);
} 