using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades.EntidadesConValoresPredefinidos;

public class LocalVisitante : Entidad, IEntidadConValorPredefinido
{
    [Required, MaxLength(50)]
    public string Estado { get; set; } = string.Empty;
}
