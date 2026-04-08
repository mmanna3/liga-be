using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class LeyendaTablaPosicionesDTO : DTO
{
    [Required, MaxLength(1000)]
    public string Leyenda { get; set; } = string.Empty;

    /// <summary>
    /// Categoría del torneo; null = leyenda general de la zona (como máximo una por zona).
    /// </summary>
    public int? CategoriaId { get; set; }

    public int ZonaId { get; set; }
}
