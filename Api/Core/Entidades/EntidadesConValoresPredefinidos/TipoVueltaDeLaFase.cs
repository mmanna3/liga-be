using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades.EntidadesConValoresPredefinidos;

public class TipoVueltaDeLaFase : Entidad, IEntidadConValorPredefinido
{
    [Required, MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;
}
