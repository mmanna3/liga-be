using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades;

public class LeyendaTablaPosiciones : Entidad
{
    [Required, MaxLength(1000)]
    public string Leyenda { get; set; } = string.Empty;

    public int? CategoriaId { get; set; }

    public virtual TorneoCategoria? Categoria { get; set; }

    public int ZonaId { get; set; }

    public virtual Zona Zona { get; set; } = null!;
}