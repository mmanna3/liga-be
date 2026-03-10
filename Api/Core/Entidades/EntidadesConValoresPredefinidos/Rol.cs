using System.ComponentModel.DataAnnotations;
using Api.Core.Entidades;

namespace Api.Core.Entidades.EntidadesConValoresPredefinidos;

public class Rol : Entidad, IEntidadConValorPredefinido
{
    [Required, MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;
}
