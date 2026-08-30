using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;

namespace Api.Core.Repositorios;

public interface IDelegadoRepo : IRepositorioABM<Delegado>
{
    /// <summary>Listado plano y liviano para el home de delegados.</summary>
    Task<List<DelegadoListadoDTO>> ListarParaListado(IList<EstadoDelegadoEnum> estados);
    Task<Delegado?> ObtenerPorIdParaEliminar(int id);
    Task<List<Delegado>> ListarActivosDelClub(int clubId);
    Task<List<(Delegado Delegado, int? JugadorId)>> ListarConFiltroConJugadorIds(IList<EstadoDelegadoEnum> estados);
    Task<Delegado?> ObtenerPorDNI(string dni);
    Task<Delegado> ObtenerPorUsuario(string usuario);
    Task<List<(Delegado Delegado, int? JugadorId)>> ListarConJugadorIds();
    Task<int?> ObtenerJugadorIdPorDNI(string dni);
    Task<int> ContarClubsDelDelegado(int delegadoId);
}