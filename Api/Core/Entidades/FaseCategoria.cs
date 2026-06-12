using Microsoft.EntityFrameworkCore;

namespace Api.Core.Entidades;

[Index(nameof(FaseId), nameof(Orden), IsUnique = true)]
public class FaseCategoria : Entidad
{
    public required string Nombre { get; set; }
    public required int AnioDesde { get; set; }
    public required int AnioHasta { get; set; }
    public required int Orden { get; set; }
    public virtual Fase Fase { get; set; } = null!;
    public required int FaseId { get; set; }

    public virtual ICollection<LeyendaTablaPosiciones> LeyendasTablaPosiciones { get; set; } = new List<LeyendaTablaPosiciones>();
}
