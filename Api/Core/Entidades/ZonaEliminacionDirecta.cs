using Microsoft.EntityFrameworkCore;

namespace Api.Core.Entidades;

[Index(nameof(FaseId), nameof(CategoriaId), IsUnique = true)]
public class ZonaEliminacionDirecta : Zona
{
    public virtual TorneoCategoria Categoria { get; set; } = null!;
    public required int CategoriaId { get; set; }
    public virtual FaseEliminacionDirecta Fase { get; set; } = null!;
    public virtual ICollection<FechaEliminacionDirecta> Fechas { get; set; } = null!;
}
