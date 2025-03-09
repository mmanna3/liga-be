using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class JugadorBaseDTO : DTO
{
    [Required, MaxLength(9)] 
    public string DNI { get; set; } = string.Empty;

    [Required, MaxLength(14)] 
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(14)] 
    public string Apellido { get; set; } = string.Empty;

    [Required]
    public DateTime FechaNacimiento { get; set; }
}