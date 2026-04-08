using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class DniExpulsadoDeLaLigaDTO : DTO
{
    [Required]
    [MaxLength(1000)]
    public string Explicacion { get; set; } = string.Empty;

    public int DNI { get; set; }
}
