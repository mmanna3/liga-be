using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface ILeyendaTablaPosicionesRepo : IRepositorioABMAnidado<LeyendaTablaPosiciones, int>
{
    /// <summary>
    /// Indica si ya existe otra fila con el mismo ZonaId, CategoriaId y EquipoId (null = leyenda sin equipo asociado).
    /// </summary>
    Task<bool> ExisteOtraConMismaZonaCategoriaYEquipo(int zonaId, int? categoriaId, int? equipoId, int? excluirLeyendaId);
}
