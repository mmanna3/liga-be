using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IFechaRepo : IRepositorioABMAnidado<Fecha, int>
{
    Task<IEnumerable<int>> ListarIdsPorPadre(int padreId);
    Task<Fecha?> ObtenerPorIdYPadreParaEliminar(int padreId, int id);
}
