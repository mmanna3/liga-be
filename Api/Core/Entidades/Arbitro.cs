using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades;

public class Arbitro : Entidad
{
    [Required, MaxLength(9)] 
    public string DNI { get; set; } = string.Empty;

    [MaxLength(14), Required] 
    public required string Nombre { get; set; }
    
    [MaxLength(14), Required]
    public required string Apellido { get; set; }

    [MaxLength(20)]
    public string? TelefonoCelular { get; set; }
}