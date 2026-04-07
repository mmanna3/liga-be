using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IFechaRepo : IRepositorioABMAnidado<Fecha, int>
{
    Task<IEnumerable<int>> ListarIdsPorPadre(int padreId);
    Task<Fecha?> ObtenerPorIdYPadreParaEliminar(int padreId, int id);

    /// <summary>Filas afectadas (0 si no existe la fecha en esa zona).</summary>
    Task<int> ActualizarEsVisibleEnApp(int zonaId, int fechaId, bool esVisibleEnApp);

    /// <summary>Fechas todos-contra-todos de la zona visibles en app, ordenadas por número, con jornadas y equipos para armar fixture.</summary>
    Task<IReadOnlyList<FechaTodosContraTodos>> ListarTodosContraTodosPorZonaParaAppAsync(int zonaId,
        CancellationToken cancellationToken = default);

    /// <summary>Igual que <see cref="ListarTodosContraTodosPorZonaParaAppAsync"/> más zona→fase→torneo→categorías y partidos con categoría para resultados.</summary>
    Task<IReadOnlyList<FechaTodosContraTodos>> ListarTodosContraTodosPorZonaParaAppConPartidosAsync(int zonaId,
        CancellationToken cancellationToken = default);

    /// <summary>Categorías del torneo de una zona todos-contra-todos (vacío si la zona no existe o no es TCT).</summary>
    Task<IReadOnlyList<TorneoCategoria>> ListarCategoriasTorneoPorZonaTodosContraTodosAsync(int zonaId,
        CancellationToken cancellationToken = default);

    /// <summary>Fechas de eliminación directa visibles en app, ordenadas por instancia (de llave mayor a final).</summary>
    Task<IReadOnlyList<FechaEliminacionDirecta>> ListarEliminacionDirectaPorZonaParaAppAsync(int zonaId,
        CancellationToken cancellationToken = default);
}
