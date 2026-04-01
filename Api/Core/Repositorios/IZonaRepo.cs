using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IZonaRepo : IRepositorioABMAnidado<Zona, int>
{
    Task<IEnumerable<int>> ListarIdsPorPadre(int padreId);
    Task<Zona?> ObtenerPorIdYPadreParaEliminar(int padreId, int id);
}
