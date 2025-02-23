using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class JugadorEquipoDTO : DTO
{
    public virtual JugadorDTO Jugador { get; set; } = null!;

    public virtual EquipoDTO Equipo { get; set; } = null!;

    [Required]
    public DateTime FechaFichaje { get; set; }
    
    public virtual EstadoJugadorDTO EstadoJugador { get; set; } = null!;

    [MaxLength(250)]
    public string? MotivoDeRechazoFichaje { get; set; }
}