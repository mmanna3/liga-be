using Microsoft.EntityFrameworkCore;

namespace Api.Core.Entidades;

[Index(nameof(TorneoId), nameof(Orden), IsUnique = true)]
public class TorneoCategoria : Entidad
{
    public required string Nombre { get; set; }
    public required int AnioDesde { get; set; }
    public required int AnioHasta { get; set; }    
    public required int Orden { get; set; }
    public virtual Torneo Torneo { get; set; } = null!;
    public required int TorneoId { get; set; }

}