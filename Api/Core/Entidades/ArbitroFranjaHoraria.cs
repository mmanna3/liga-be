using Api.Core.Entidades.EntidadesConValoresPredefinidos;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Entidades;

[Index(nameof(ArbitroId), nameof(FranjaHorariaArbitroId), IsUnique = true)]
public class ArbitroFranjaHoraria : Entidad
{
    public int ArbitroId { get; set; }
    public virtual Arbitro Arbitro { get; set; } = null!;
    public int FranjaHorariaArbitroId { get; set; }
    public virtual FranjaHorariaArbitro FranjaHorariaArbitro { get; set; } = null!;
}
