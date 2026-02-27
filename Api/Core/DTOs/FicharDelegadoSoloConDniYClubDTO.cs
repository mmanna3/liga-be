using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

/// <summary>
/// DTO para fichar un delegado cuyos datos ya existen en el sistema (como delegado o jugador).
/// DNI y ClubId son siempre obligatorios.
/// Email y TelefonoCelular son obligatorios cuando la persona existe como jugador (los jugadores no tienen esos datos).
/// </summary>
public class FicharDelegadoSoloConDniYClubDTO : DTO
{
    [Required, MaxLength(9)]
    public string DNI { get; set; } = string.Empty;

    [Required]
    public int ClubId { get; set; }

    /// <summary>
    /// Obligatorio cuando la persona existe como jugador.
    /// </summary>
    [MaxLength(100)]
    public string? Email { get; set; }

    /// <summary>
    /// Obligatorio cuando la persona existe como jugador.
    /// </summary>
    [MaxLength(20)]
    public string? TelefonoCelular { get; set; }
}
