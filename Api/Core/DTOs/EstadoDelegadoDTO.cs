using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class EstadoDelegadoDTO : DTO
{
    [Required, MaxLength(50)]
    public string Estado { get; set; } = string.Empty;
}
