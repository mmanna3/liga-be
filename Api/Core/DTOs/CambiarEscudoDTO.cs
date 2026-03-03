using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class CambiarEscudoDTO
{
    [Required]
    public required string ImagenBase64 { get; set; }
}
