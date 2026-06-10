using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface IArbitroAsignacionCore
{
    Task<AsignacionArbitrosPorAgrupadorDTO> ObtenerAsignacionPorAgrupador(int agrupadorId, int anio);
    Task AsignarArbitrosAJornada(int jornadaId, AsignarArbitrosJornadaDTO dto);
}
