using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IFechaRepo : IRepositorioABMAnidado<Fecha, int>
{
    Task<IEnumerable<int>> ListarIdsPorPadre(int padreId);
    Task<Fecha?> ObtenerPorIdYPadreParaEliminar(int padreId, int id);

    /// <summary>Filas afectadas (0 si no existe la fecha en esa zona).</summary>
    Task<int> ActualizarEsVisibleEnApp(int zonaId, int fechaId, bool esVisibleEnApp);
}
