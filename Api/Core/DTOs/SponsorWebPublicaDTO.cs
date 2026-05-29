using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class SponsorWebPublicaDTO : DTO
{
    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    public int Orden { get; set; }

    /// <summary>
    /// Logo en base64 con mime type (solo para el panel administrativo).
    /// </summary>
    public string Imagen { get; set; } = string.Empty;
}
