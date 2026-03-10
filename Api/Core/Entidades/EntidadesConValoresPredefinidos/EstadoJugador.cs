using System.ComponentModel.DataAnnotations;
using Api.Core.Entidades;

namespace Api.Core.Entidades.EntidadesConValoresPredefinidos;

public class EstadoJugador : Entidad, IEntidadConValorPredefinido
{
    [Required, MaxLength(50)]
    public string Estado { get; set; } = string.Empty;

    public ICollection<JugadorEquipo> JugadorEquipos { get; set; } = new List<JugadorEquipo>();
}
