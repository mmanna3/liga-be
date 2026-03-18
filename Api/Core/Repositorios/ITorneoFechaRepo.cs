using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface ITorneoFechaRepo : IRepositorioABMAnidado<TorneoFecha, int>
{
    Task<IEnumerable<int>> ListarIdsPorPadre(int padreId);
    Task<TorneoFecha?> ObtenerPorIdYPadreParaEliminar(int padreId, int id);
}
