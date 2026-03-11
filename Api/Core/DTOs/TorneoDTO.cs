using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class TorneoDTO : DTO
{
    [Required]
    public required string Nombre { get; set; }

    public required int Anio { get; set; }
    public int TorneoAgrupadorId { get; set; }
    public string TorneoAgrupadorNombre { get; set; } = string.Empty;
    public bool SePuedeEditar { get; set; }
    /// <summary>
    /// Null = no modificar. Lista vacía = borrar todas. Con items = reemplazar.
    /// </summary>
    public List<TorneoFaseDTO>? Fases { get; set; }
    /// <summary>
    /// Null = no modificar. Lista vacía = borrar todas. Con items = reemplazar.
    /// </summary>
    public List<TorneoCategoriaDTO>? Categorias { get; set; }
}