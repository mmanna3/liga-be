using System.ComponentModel.DataAnnotations;
using Api.Core.Entidades.EntidadesConValoresPredefinidos;

namespace Api.Core.Entidades;

public class TorneoFase : Entidad
{
    [Required, MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;
    public required int Numero { get; set; }
    public virtual Torneo Torneo { get; set; } = null!;
    public required int TorneoId { get; set; }

    public int FaseFormatoId { get; set; }
    public virtual FaseFormato FaseFormato { get; set; } = null!;

    public int? InstanciaEliminacionDirectaId { get; set; }
    public virtual InstanciaEliminacionDirecta? InstanciaEliminacionDirecta { get; set; }

    public int EstadoFaseId { get; set; }
    public virtual EstadoFase EstadoFase { get; set; } = null!;
    public virtual ICollection<TorneoZona> Zonas { get; set; } = null!;

    public bool EsVisibleEnApp { get; set; }
}