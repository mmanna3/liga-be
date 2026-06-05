using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class ArbitroDTO : DTO
{
    [Required, MaxLength(9)]
    public string DNI { get; set; } = string.Empty;

    [Required, MaxLength(14)]
    public required string Nombre { get; set; }

    [Required, MaxLength(14)]
    public required string Apellido { get; set; }

    [MaxLength(20)]
    public string? TelefonoCelular { get; set; }
}
