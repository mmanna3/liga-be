using System.ComponentModel.DataAnnotations;
using Api.Core.Entidades;

namespace Api.Core.Entidades.EntidadesConValoresPredefinidos;

public class FranjaHorariaArbitro : Entidad, IEntidadConValorPredefinido
{
    [Required, MaxLength(50)]
    public string Franja { get; set; } = string.Empty;
}
