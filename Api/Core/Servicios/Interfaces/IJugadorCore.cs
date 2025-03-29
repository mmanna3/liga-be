using Api.Core.DTOs;
using Api.Core.DTOs.CambiosDeEstadoJugador;
using Api.Core.Enums;

namespace Api.Core.Servicios.Interfaces;

public interface IJugadorCore : ICoreABM<JugadorDTO>
{
    Task<IEnumerable<JugadorDTO>> ListarConFiltro(IList<EstadoJugadorEnum> estados);
    Task<int> Eliminar(int id);
    Task<int> Aprobar(AprobarJugadorDTO dto);
    Task<int> Rechazar(RechazarJugadorDTO dto);
    Task<int> Activar(List<CambiarEstadoDelJugadorDTO> dtos);
    Task<int> Suspender(List<CambiarEstadoDelJugadorDTO> dtos);
    Task<int> Inhabilitar(List<CambiarEstadoDelJugadorDTO> dtos);
    Task<int> PagarFichaje(CambiarEstadoDelJugadorDTO dto);
}