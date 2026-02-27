using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class DelegadoDTO : DTO, IFotosDTO
{
    [Required, MaxLength(9)] 
    public string DNI { get; set; } = string.Empty;

    [Required, MaxLength(14)] 
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(14)] 
    public string Apellido { get; set; } = string.Empty;

    [Required]
    public DateTime FechaNacimiento { get; set; }

    [MaxLength(20)]
    public string? TelefonoCelular { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }
    
    public string? NombreUsuario { get; set; }

    [Required]
    public string FotoCarnet { get; set; } = string.Empty;
    [Required]
    public string FotoDNIFrente { get; set; } = string.Empty;
    [Required]
    public string FotoDNIDorso { get; set; } = string.Empty;
    
    public bool BlanqueoPendiente { get; set; }
    public virtual required int ClubId { get; set; }
    public EstadoDelegadoDTO? EstadoDelegado { get; set; }
    public string ClubNombre { get; set; } = string.Empty;
    public List<string> EquiposDelClub { get; set; } = new();
}