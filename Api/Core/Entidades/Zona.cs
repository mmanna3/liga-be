using Microsoft.EntityFrameworkCore;

namespace Api.Core.Entidades;

[Index(nameof(FaseId), nameof(Orden), IsUnique = true)]
public abstract class Zona : Entidad
{
    public required string Nombre { get; set; }
    public required int FaseId { get; set; }
    public required int Orden { get; set; }
    public virtual ICollection<EquipoZona> EquiposZona { get; set; } = null!;
    public virtual ICollection<LeyendaTablaPosiciones> LeyendasTablaPosiciones { get; set; } = [];
}
