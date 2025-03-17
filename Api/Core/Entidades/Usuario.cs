using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades;

public class Usuario : Entidad
{
    [Required, MaxLength(14)]
    public string NombreUsuario { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? Password { get; set; }
} 