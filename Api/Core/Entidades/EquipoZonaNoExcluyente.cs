using Api.Core.Entidades.EntidadesConValoresPredefinidos;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Entidades;

[Index(nameof(EquipoId), nameof(ZonaNoExcluyenteId), IsUnique = true)]
public class EquipoZonaNoExcluyente : Entidad
{
    public int EquipoId { get; set; }
    public virtual Equipo Equipo { get; set; } = null!;
    public int ZonaNoExcluyenteId { get; set; }
    public virtual TorneoZona ZonaNoExcluyente { get; set; } = null!;
}
