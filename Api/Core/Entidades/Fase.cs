using System.ComponentModel.DataAnnotations;
using Api.Core.Entidades.EntidadesConValoresPredefinidos;

namespace Api.Core.Entidades;

public abstract class Fase : Entidad
{
    [Required, MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;
    public required int Numero { get; set; }
    public virtual Torneo Torneo { get; set; } = null!;
    public required int TorneoId { get; set; }

    public int EstadoFaseId { get; set; }
    public virtual EstadoFase EstadoFase { get; set; } = null!;

    public bool EsVisibleEnApp { get; set; }
}
