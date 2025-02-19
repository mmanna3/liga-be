using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades;

public class EstadoJugador : Entidad
{
    [Required, MaxLength(50)]
    public string Estado { get; set; } = string.Empty;

    public ICollection<JugadorEquipo> JugadorEquipos { get; set; } = new List<JugadorEquipo>();
}