using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

/// <summary>
/// DTO para fichar un delegado cuyos datos ya existen en el sistema (como delegado o jugador).
/// DNI y ClubId son siempre obligatorios.
/// Email y TelefonoCelular son opcionales.
/// </summary>
public class FicharDelegadoSoloConDniYClubDTO : DTO
{
    [Required, MaxLength(9)]
    public string DNI { get; set; } = string.Empty;

    [Required]
    public int ClubId { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? TelefonoCelular { get; set; }
}
