using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades;

public class Jugador : Entidad
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

    public ICollection<JugadorEquipo> JugadorEquipos { get; set; } = new List<JugadorEquipo>();

    public string NombreCompleto() => Nombre + " " + Apellido;
    
    public string NombreCompletoYDNI() => NombreCompleto() + " - " + DNI;
}