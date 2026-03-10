using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class TorneoZonaDTO : DTO
{
    [Required]
    public required string Nombre { get; set; }

    public int TorneoFaseId { get; set; }
}
