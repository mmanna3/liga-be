using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class CrearSponsorWebPublicaDTO : SponsorWebPublicaDTO
{
    [Required]
    public string ImagenBase64 { get; set; } = string.Empty;
}
