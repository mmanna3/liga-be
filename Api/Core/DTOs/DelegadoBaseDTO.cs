using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class DelegadoBaseDTO : DTO
{
    [Required, MaxLength(9)]
    public string DNI { get; set; } = string.Empty;

    [Required, MaxLength(14)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(14)]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    public DateTime FechaNacimiento { get; set; }

    [MaxLength(20)]
    public string? TelefonoCelular { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }
}
