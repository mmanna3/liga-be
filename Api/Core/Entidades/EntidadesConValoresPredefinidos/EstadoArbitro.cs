using System.ComponentModel.DataAnnotations;
using Api.Core.Entidades;

namespace Api.Core.Entidades.EntidadesConValoresPredefinidos;

public class EstadoArbitro : Entidad, IEntidadConValorPredefinido
{
    [Required, MaxLength(50)]
    public string Estado { get; set; } = string.Empty;
}
