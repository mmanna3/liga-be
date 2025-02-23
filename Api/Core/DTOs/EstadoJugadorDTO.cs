using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class EstadoJugadorDTO : DTO
{
    [Required, MaxLength(50)]
    public string Estado { get; set; } = string.Empty;

    public ICollection<JugadorEquipoDTO> JugadorEquipos { get; set; } = new List<JugadorEquipoDTO>();
}