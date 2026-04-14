using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class CambiarEscudoPorDefectoDTO
{
    /// <summary>
    /// Escudo por defecto en base64 con prefijo data:image (ej. data:image/jpeg;base64,...).
    /// </summary>
    [Required]
    public string Escudo { get; set; } = string.Empty;
}
