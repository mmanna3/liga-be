using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Core.Entidades;

public class JugadorEquipo : Entidad
{
    public virtual Jugador Jugador { get; set; } = null!;

    public virtual Equipo Equipo { get; set; } = null!;

    [Required]
    public DateTime FechaFichaje { get; set; }
    
    public virtual EstadoJugador EstadoJugador { get; set; } = null!;

    [MaxLength(250)]
    public string? MotivoDeRechazoFichaje { get; set; }
}