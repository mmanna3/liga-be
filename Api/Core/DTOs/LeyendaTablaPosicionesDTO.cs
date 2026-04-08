using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class LeyendaTablaPosicionesDTO : DTO
{
    [MaxLength(1000)]
    public string Leyenda { get; set; } = string.Empty;

    /// <summary>
    /// Categoría del torneo; null = leyenda general de la zona (como máximo una por zona).
    /// </summary>
    public int? CategoriaId { get; set; }

    public int ZonaId { get; set; }

    /// <summary>Si es null, leyenda general de la categoría/zona. Si tiene valor, leyenda específica de ese equipo (única por zona+categoría+equipo).</summary>
    public int? EquipoId { get; set; }

    /// <summary>Nombre del equipo al que se le quitan puntos; solo informado cuando <see cref="EquipoId"/> tiene valor.</summary>
    public string? Equipo { get; set; }

    /// <summary>Solo aplica con <see cref="EquipoId"/>; debe ser mayor que cero en ese caso.</summary>
    public int QuitaDePuntos { get; set; }
}
