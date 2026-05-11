using System.ComponentModel.DataAnnotations;
using Api.Core.Entidades;

namespace Api.Core.Entidades.EntidadesConValoresPredefinidos;

public class CanchaSuperficie : Entidad, IEntidadConValorPredefinido
{
    [Required, MaxLength(30)]
    public string Superficie { get; set; } = string.Empty;
    public ICollection<Club> Clubes { get; set; } = [];
}
