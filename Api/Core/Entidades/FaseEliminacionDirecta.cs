using Api.Core.Entidades.EntidadesConValoresPredefinidos;

namespace Api.Core.Entidades;

public class FaseEliminacionDirecta : Fase
{
    public int? InstanciaEliminacionDirectaId { get; set; }
    public virtual InstanciaEliminacionDirecta? InstanciaEliminacionDirecta { get; set; }
    public virtual ICollection<ZonaEliminacionDirecta> Zonas { get; set; } = null!;
}
