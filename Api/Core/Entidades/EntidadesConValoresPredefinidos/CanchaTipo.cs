using System.ComponentModel.DataAnnotations;
using Api.Core.Entidades;

namespace Api.Core.Entidades.EntidadesConValoresPredefinidos;

public class CanchaTipo : Entidad, IEntidadConValorPredefinido
{
    [Required, MaxLength(30)]
    public string Tipo { get; set; } = string.Empty;
    public ICollection<Club> Clubes { get; set; } = [];
}
