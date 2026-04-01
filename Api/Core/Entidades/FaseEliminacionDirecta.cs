using Api.Core.Entidades.EntidadesConValoresPredefinidos;

namespace Api.Core.Entidades;

public class FaseEliminacionDirecta : TorneoFase
{
    public int? InstanciaEliminacionDirectaId { get; set; }
    public virtual InstanciaEliminacionDirecta? InstanciaEliminacionDirecta { get; set; }
}
