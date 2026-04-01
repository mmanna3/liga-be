namespace Api.Core.Entidades;

public class ZonaEliminacionDirecta : TorneoZona
{
    public virtual FaseEliminacionDirecta Fase { get; set; } = null!;
}
