using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades.EntidadesConValoresPredefinidos;

public class FormatoDeLaFase : Entidad, IEntidadConValorPredefinido
{
    [Required, MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;
}
