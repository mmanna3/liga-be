using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface IArbitroAsignacionCore
{
    Task<AsignacionArbitrosPorAgrupadorDTO> ObtenerAsignacionPorAgrupador(int agrupadorId, int anio);
    Task<AsignacionHistoricaArbitrosPorAgrupadorDTO> ObtenerAsignacionHistoricaPorAgrupador(int agrupadorId, int anio);
    Task AsignarArbitrosAJornada(int jornadaId, AsignarArbitrosJornadaDTO dto);
    Task MarcarWhatsappEnviado(int jornadaId, int arbitroId, MarcarWhatsappEnviadoArbitroJornadaDTO dto);
}
