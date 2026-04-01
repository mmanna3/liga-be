namespace Api.Core.Entidades;

public class ZonaEliminacionDirecta : TorneoZona
{
    public virtual FaseEliminacionDirecta Fase { get; set; } = null!;
    public virtual ICollection<FechaEliminacionDirecta> Fechas { get; set; } = null!;
}
