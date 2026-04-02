using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface ITorneoCategoriaRepo : IRepositorioABMAnidado<TorneoCategoria, int>
{
    /// <summary>
    /// Indica si alguna de las categorías tiene partidos o zonas de eliminación directa asociadas.
    /// </summary>
    Task<bool> AlgunaTienePartidosOZonas(IEnumerable<int> categoriaIds);
}
