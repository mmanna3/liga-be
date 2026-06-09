using Microsoft.EntityFrameworkCore;

namespace Api.Core.Entidades;

[Index(nameof(ArbitroId), nameof(TorneoAgrupadorId), IsUnique = true)]
public class ArbitroTorneoAgrupador : Entidad
{
    public int ArbitroId { get; set; }
    public virtual Arbitro Arbitro { get; set; } = null!;
    public int TorneoAgrupadorId { get; set; }
    public virtual TorneoAgrupador TorneoAgrupador { get; set; } = null!;
}
