using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades;

public class Rol : Entidad
{
    [Required, MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;
} 