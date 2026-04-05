using System.ComponentModel.DataAnnotations;
using Api.Core.Entidades;

namespace Api.Core.Entidades.EntidadesConValoresPredefinidos;

public class Color : Entidad, IEntidadConValorPredefinido
{
    [Required, MaxLength(30)]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<TorneoAgrupador> Agrupadores { get; set; } = [];
}
