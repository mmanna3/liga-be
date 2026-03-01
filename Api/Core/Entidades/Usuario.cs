using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades;

public class Usuario : Entidad
{
    [Required, MaxLength(14)]
    public string NombreUsuario { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? Password { get; set; }

    public int RolId { get; set; }
    public virtual Rol Rol { get; set; } = null!;

    public int? DelegadoId { get; set; }
    public virtual Delegado? Delegado { get; set; }
} 