using Api.Core.DTOs.AppCarnetDigital;

namespace Api.Core.Servicios.Interfaces;

public interface IAppCarnetDigitalCore
{
    Task<EquiposDelDelegadoDTO> ObtenerEquiposPorUsuarioDeDelegado(string usuario);
    Task<ICollection<CarnetDigitalDTO>?> Carnets(int equipoId);
    Task<ICollection<CarnetDigitalPendienteDTO>?> JugadoresPendientes(int equipoId);
    Task<ICollection<CarnetDigitalDTO>?> CarnetsPorCodigoAlfanumerico(string codigoAlfanumerico);
    Task<IReadOnlyList<InformacionInicialAgrupadorDTO>> InformacionInicialDeTorneosAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClubDTO>> ClubesPorZonaAsync(int zonaId, CancellationToken cancellationToken = default);
}