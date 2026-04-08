using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface ILeyendaTablaPosicionesRepo : IRepositorioABMAnidado<LeyendaTablaPosiciones, int>
{
    /// <summary>
    /// Indica si ya existe otra fila con el mismo ZonaId y CategoriaId (ambos null en categoría = leyenda general).
    /// </summary>
    Task<bool> ExisteOtraConMismaZonaYCategoria(int zonaId, int? categoriaId, int? excluirLeyendaId);
}
