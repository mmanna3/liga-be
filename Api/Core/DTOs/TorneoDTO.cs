using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class TorneoDTO : DTO
{
    [Required]
    public required string Nombre { get; set; }

    public required int Anio { get; set; }
    public int TorneoAgrupadorId { get; set; }
    public string TorneoAgrupadorNombre { get; set; } = string.Empty;
}