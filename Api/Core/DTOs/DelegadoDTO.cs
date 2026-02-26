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
    
    public string? NombreUsuario { get; set; }

    public string CodigoAlfanumerico { get; set; }
    
    public string FotoCarnet { get; set; } = string.Empty;
    public string FotoDNIFrente { get; set; } = string.Empty;
    public string FotoDNIDorso { get; set; } = string.Empty;
    
    public bool BlanqueoPendiente { get; set; }
    public virtual required int ClubId { get; set; }
}