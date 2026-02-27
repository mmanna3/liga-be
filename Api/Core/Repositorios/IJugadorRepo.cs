using Api.Core.Entidades;
using Api.Core.Enums;

namespace Api.Core.Repositorios;

public interface IJugadorRepo : IRepositorioABM<Jugador>
{
    Task<IEnumerable<Jugador>> ListarConFiltro(IList<EstadoJugadorEnum> estado);
    Task<List<(Jugador Jugador, int? DelegadoId)>> ListarConFiltroConDelegadoIds(IList<EstadoJugadorEnum> estados);
    Task<Jugador?> ObtenerPorDNI(string dni);
    Task<Jugador?> ObtenerPorIdParaEliminar(int id);
    void SiElDNISeHabiaFichadoYEstaRechazadoEliminarJugador(string entidadDNI);
    void CambiarEstado(int jugadorEquipoId, EstadoJugadorEnum nuevoEstado, string? motivo = null);
    void Eliminar(Jugador jugador);
    void EliminarJugadorEquipo(int jugadorEquipoId);
    Task<bool> JugadorYaJuegaEnTorneoDelEquipoDestino(int jugadorId, int equipoOrigenId, int equipoDestinoId);
    Task<int?> ObtenerDelegadoIdPorDNI(string dni);
}