namespace Api.Core.Entidades;

public class ZonaEliminacionDirecta : Zona
{
    public virtual TorneoCategoria Categoria { get; set; } = null!;
    public required int CategoriaId { get; set; }
    public virtual FaseEliminacionDirecta Fase { get; set; } = null!;
    public virtual ICollection<FechaEliminacionDirecta> Fechas { get; set; } = null!;
}
