using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades.EntidadesConValoresPredefinidos;

public class FaseFormato : Entidad, IEntidadConValorPredefinido
{
    [Required, MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;
}
