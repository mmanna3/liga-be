using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class JugadorDTO : DTO
{
    [Required, MaxLength(9)] 
    // ReSharper disable once InconsistentNaming
    public string DNI { get; set; } = string.Empty;

    [Required, MaxLength(14)] 
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(14)] 
    public string Apellido { get; set; } = string.Empty;

    [Required]
    public DateTime FechaNacimiento { get; set; }
    
    public int EquipoInicialId { get; set; }
    public ICollection<EquipoDelJugadorDTO> Equipos { get; set; } = new List<EquipoDelJugadorDTO>();
}