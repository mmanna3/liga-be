namespace Api.Core.Entidades;

public class FaseEliminacionDirecta : Fase
{
    public virtual ICollection<ZonaEliminacionDirecta> Zonas { get; set; } = null!;
}
