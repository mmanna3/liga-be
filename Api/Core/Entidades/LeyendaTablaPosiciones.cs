using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades;

public class LeyendaTablaPosiciones : Entidad
{
    [MaxLength(1000)]
    public string Leyenda { get; set; } = string.Empty;

    public int? CategoriaId { get; set; }

    public virtual FaseCategoria? Categoria { get; set; }

    public int ZonaId { get; set; }

    public virtual Zona Zona { get; set; } = null!;

    public int? EquipoId { get; set; }
    public virtual Equipo? Equipo { get; set; }

    /// <summary>Puntos descontados al equipo; si hay <see cref="EquipoId"/>, debe ser mayor que cero.</summary>
    public int QuitaDePuntos { get; set; }

    /// <summary>
    /// Si es true, la leyenda es de la tabla Anual (no de la tabla de la fase). Se guarda en la zona de la fase
    /// Apertura y no puede tener equipo ni quita de puntos.
    /// </summary>
    public bool EsDeTablaAnual { get; set; }
}