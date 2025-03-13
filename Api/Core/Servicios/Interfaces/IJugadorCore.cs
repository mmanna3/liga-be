using Api.Core.DTOs;
using Api.Core.DTOs.CambiosDeEstadoJugador;
using Api.Core.Entidades;
using Api.Core.Enums;

namespace Api.Core.Servicios.Interfaces;

public interface IJugadorCore : ICoreABM<JugadorDTO>
{
    Task<int> Gestionar(GestionarJugadorDTO dto);
    Task<IEnumerable<JugadorDTO>> ListarConFiltro(IList<EstadoJugadorEnum> estados);
    Task<int> Aprobar(AprobarJugadorDTO dto);
    Task<int> Rechazar(RechazarJugadorDTO dto);
    Task<int> Activar(ActivarJugadorDTO dto);
    Task<int> Suspender(SuspenderJugadorDTO dto);
    Task<int> Inhabilitar(InhabilitarJugadorDTO dto);
}