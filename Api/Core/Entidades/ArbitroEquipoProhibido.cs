using Microsoft.EntityFrameworkCore;

namespace Api.Core.Entidades;

[Index(nameof(ArbitroId), nameof(EquipoId), IsUnique = true)]
public class ArbitroEquipoProhibido : Entidad
{
    public int ArbitroId { get; set; }
    public virtual Arbitro Arbitro { get; set; } = null!;
    public int EquipoId { get; set; }
    public virtual Equipo Equipo { get; set; } = null!;
}
