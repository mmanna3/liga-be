using Api.Core.DTOs;
using Api.Core.DTOs.CambiosDeEstadoJugador;
using Api.Core.Enums;

namespace Api.Core.Servicios.Interfaces;

public interface IJugadorCore : ICoreABM<JugadorDTO>
{
    Task<IEnumerable<JugadorDTO>> ListarConFiltro(IList<EstadoJugadorEnum> estados);
    Task<int> Aprobar(AprobarJugadorDTO dto);
    Task<int> Rechazar(RechazarJugadorDTO dto);
    Task<int> Activar(CambiarEstadoDelJugadorDTO dto);
    Task<int> Suspender(CambiarEstadoDelJugadorDTO dto);
    Task<int> Inhabilitar(CambiarEstadoDelJugadorDTO dto);
    Task<int> PagarFichaje(CambiarEstadoDelJugadorDTO dto);
}