using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades;

public class Delegado : Entidad
{
    [Required, MaxLength(9)] 
    public string DNI { get; set; } = string.Empty;

    [MaxLength(14)] 
    public required string Nombre { get; set; }
    
    [MaxLength(14)]
    public required string Apellido { get; set; }

    [Required]
    public DateTime FechaNacimiento { get; set; }

    [MaxLength(20)]
    public string? TelefonoCelular { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }
    
    public required int ClubId { get; set; }
    public virtual Club Club { get; set; } = null!;

    public int? UsuarioId { get; set; }
    public virtual Usuario? Usuario { get; set; }

    public int EstadoDelegadoId { get; set; }
    public virtual EstadoDelegado EstadoDelegado { get; set; } = null!;
}