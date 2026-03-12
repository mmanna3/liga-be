using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface ITorneoZonaRepo : IRepositorioABMAnidado<TorneoZona, int>
{
    Task<IEnumerable<int>> ListarIdsPorPadre(int padreId);
    Task<TorneoZona?> ObtenerPorIdYPadreParaEliminar(int padreId, int id);
}
