using Api.Core.Entidades;

namespace Api.Core.Repositorios;

/// <summary>
/// Interfaz para repositorios de entidades que siempre dependen de un padre.
/// </summary>
public interface IRepositorioABMAnidado<TModel, TPadreId> : IRepositorioABM<TModel>
    where TModel : Entidad
{
    Task<IEnumerable<TModel>> ListarPorPadre(TPadreId padreId);
    Task<TModel?> ObtenerPorIdYPadre(TPadreId padreId, int id);
}
