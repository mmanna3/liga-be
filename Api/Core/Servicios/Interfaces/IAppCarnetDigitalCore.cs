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

    Task<IReadOnlyList<ClubesDTO>> ClubesPorZonaAsync(int zonaId, CancellationToken cancellationToken = default);

    Task<FixtureDTO> FixtureTodosContraTodosAsync(int zonaId, CancellationToken cancellationToken = default);

    Task<JornadasDTO> JornadasTodosContraTodosAsync(int zonaId, CancellationToken cancellationToken = default);

    Task<PosicionesDTO> PosicionesTodosContraTodosAsync(int zonaId, CancellationToken cancellationToken = default);

    Task<PosicionesDTO> PosicionesAnualAsync(int zonaId, CancellationToken cancellationToken = default);

    Task<EliminacionDirectaDTO> EliminacionDirectaAsync(int zonaId, CancellationToken cancellationToken = default);

    Task<PlanillaDeJuegoDTO> PlanillasDeJuegoAsync(string codigoAlfanumerico,
        CancellationToken cancellationToken = default);
}