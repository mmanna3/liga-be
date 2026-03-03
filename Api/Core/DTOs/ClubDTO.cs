using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class ClubDTO : DTO
{
    [Required]
    public required string Nombre { get; set; }

    /// <summary>
    /// Escudo del club en base64 con prefijo data:image (ej. data:image/jpeg;base64,...).
    /// </summary>
    public string Escudo { get; set; } = string.Empty;
    
    public ICollection<EquipoDTO>? Equipos { get; set; }
    public ICollection<DelegadoDTO>? Delegados { get; set; }
}