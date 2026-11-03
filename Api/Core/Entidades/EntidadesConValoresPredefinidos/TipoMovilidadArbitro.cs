using System.ComponentModel.DataAnnotations;
using Api.Core.Entidades;

namespace Api.Core.Entidades.EntidadesConValoresPredefinidos;

public class TipoMovilidadArbitro : Entidad, IEntidadConValorPredefinido
{
    [Required, MaxLength(50)]
    public string Tipo { get; set; } = string.Empty;
}
