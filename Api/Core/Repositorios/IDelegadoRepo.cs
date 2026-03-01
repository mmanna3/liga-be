using Api.Core.Entidades;
using Api.Core.Enums;

namespace Api.Core.Repositorios;

public interface IDelegadoRepo : IRepositorioABM<Delegado>
{
    Task<List<(Delegado Delegado, int? JugadorId)>> ListarConFiltroConJugadorIds(IList<EstadoDelegadoEnum> estados);
    Task<Delegado?> ObtenerPorDNI(string dni);
    Task<Delegado> ObtenerPorUsuario(string usuario);
    void Eliminar(Delegado delegado);
    Task<List<(Delegado Delegado, int? JugadorId)>> ListarConJugadorIds();
    Task<int?> ObtenerJugadorIdPorDNI(string dni);
}