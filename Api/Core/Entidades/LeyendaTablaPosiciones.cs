using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades;

public class LeyendaTablaPosiciones : Entidad
{
    [MaxLength(1000)]
    public string Leyenda { get; set; } = string.Empty;

    public int? CategoriaId { get; set; }

    public virtual TorneoCategoria? Categoria { get; set; }

    public int ZonaId { get; set; }

    public virtual Zona Zona { get; set; } = null!;

    public int? EquipoId { get; set; }
    public virtual Equipo? Equipo { get; set; }

    /// <summary>Puntos descontados al equipo; si hay <see cref="EquipoId"/>, debe ser mayor que cero.</summary>
    public int QuitaDePuntos { get; set; }
}