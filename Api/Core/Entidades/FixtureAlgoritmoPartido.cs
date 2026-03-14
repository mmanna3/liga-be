using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Entidades;

[Index(nameof(FixtureAlgoritmoId), nameof(Fecha), nameof(EquipoLocal), nameof(EquipoVisitante), IsUnique = true)]
public class FixtureAlgoritmoPartido : Entidad
{
    [ForeignKey("FixtureAlgoritmo")]
    public required int FixtureAlgoritmoId { get; set; }
    public virtual FixtureAlgoritmo FixtureAlgoritmo { get; set; } = null!;
    public required int Fecha { get; set; }
    public required int EquipoLocal { get; set; }
    public required int EquipoVisitante { get; set; }
}
