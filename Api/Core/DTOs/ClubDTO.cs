using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class ClubDTO : DTO
{
    [Required]
    public required string Nombre { get; set; }
    
    public virtual ICollection<EquipoDTO> Equipos { get; set; } = null!;
}