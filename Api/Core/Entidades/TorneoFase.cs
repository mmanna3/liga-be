using Api.Core.Entidades.EntidadesConValoresPredefinidos;

namespace Api.Core.Entidades;

public class TorneoFase : Entidad
{
    public required int Numero { get; set; }
    public virtual Torneo Torneo { get; set; } = null!;
    public required int TorneoId { get; set; }

    public int FaseFormatoId { get; set; }
    public virtual FaseFormato FaseFormato { get; set; } = null!;

    public int? InstanciaEliminacionDirectaId { get; set; }
    public virtual InstanciaEliminacionDirecta? InstanciaEliminacionDirecta { get; set; }

    public int FaseTipoDeVueltaId { get; set; }
    public virtual FaseTipoDeVuelta FaseTipoDeVuelta { get; set; } = null!;

    public int EstadoFaseId { get; set; }
    public virtual EstadoFase EstadoFase { get; set; } = null!;

    public bool EsVisibleEnApp { get; set; }
}