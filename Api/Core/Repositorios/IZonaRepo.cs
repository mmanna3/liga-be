using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IZonaRepo : IRepositorioABMAnidado<Zona, int>
{
    Task<IEnumerable<int>> ListarIdsPorPadre(int padreId);
    Task<Zona?> ObtenerPorIdYPadreParaEliminar(int padreId, int id);

    /// <summary>
    /// Mueve el Orden de las zonas indicadas a un valor temporal único negativo (-Id),
    /// para liberar los valores positivos del índice único (FaseId, Orden) antes de
    /// aplicar nuevos Orden en una modificación masiva.
    /// </summary>
    Task AsignarOrdenTemporal(int padreId, IEnumerable<int> ids);
}
