using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades.EntidadesConValoresPredefinidos;

public class HabilitacionFichaje : Entidad, IEntidadConValorPredefinido
{
    [Required, MaxLength(30)]
    public string TipoHabilitacion { get; set; } = string.Empty;
}
