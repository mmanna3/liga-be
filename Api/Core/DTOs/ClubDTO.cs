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

    [MaxLength(150)]
    public string? Direccion { get; set; }

    public bool? EsTechado { get; set; }

    [MaxLength(100)]
    public string? Localidad { get; set; }
    
    public ICollection<EquipoDTO>? Equipos { get; set; }
    public ICollection<DelegadoDTO>? Delegados { get; set; }
}