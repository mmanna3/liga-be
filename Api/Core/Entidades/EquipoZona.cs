using Api.Core.Entidades.EntidadesConValoresPredefinidos;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Entidades;

[Index(nameof(EquipoId), nameof(ZonaId), IsUnique = true)]
public class EquipoZona : Entidad
{
    public int EquipoId { get; set; }
    public virtual Equipo Equipo { get; set; } = null!;
    public int ZonaId { get; set; }
    public virtual Zona Zona { get; set; } = null!;
}
