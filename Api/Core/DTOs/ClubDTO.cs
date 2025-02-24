using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class ClubDTO : DTO
{
    [Required]
    public required string Nombre { get; set; }
    
    public ICollection<EquipoDTO>? Equipos { get; set; }
}