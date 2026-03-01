using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades;

public class EstadoDelegado : Entidad
{
    [Required, MaxLength(50)]
    public string Estado { get; set; } = string.Empty;
}