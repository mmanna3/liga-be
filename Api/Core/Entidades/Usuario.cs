using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades;

public class Usuario : Entidad
{
    [Required, MaxLength(14)]
    public string NombreUsuario { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string Password { get; set; } = string.Empty;
    
    [Required, MaxLength(14)]
    public string Rol { get; set; } = string.Empty;
} 