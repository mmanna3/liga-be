using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface ILeyendaTablaPosicionesRepo : IRepositorioABMAnidado<LeyendaTablaPosiciones, int>
{
    /// <summary>
    /// Indica si ya existe otra fila con el mismo ZonaId, CategoriaId, EquipoId (null = leyenda sin equipo asociado) y EsDeTablaAnual.
    /// </summary>
    Task<bool> ExisteOtraConMismaZonaCategoriaYEquipo(int zonaId, int? categoriaId, int? equipoId, bool esDeTablaAnual,
        int? excluirLeyendaId);

    /// <summary>Borra las leyendas de tabla Anual de las zonas de la fase indicada.</summary>
    Task EliminarDeTablaAnualPorFase(int faseId);
}
