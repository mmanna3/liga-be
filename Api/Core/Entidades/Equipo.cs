using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Entidades;

[Index(nameof(Nombre), nameof(TorneoId), IsUnique = true, Name = "IX_Equipo_Nombre_TorneoId")]
public class Equipo : Entidad
{
    public required string Nombre { get; set; }
    
    [ForeignKey("Club")] 
    public required int ClubId { get; set; }
    public virtual Club? Club { get; set; }
    
    [ForeignKey("Torneo")]
    public int? TorneoId { get; set; }
    public virtual Torneo? Torneo { get; set; }
    
    public virtual required ICollection<JugadorEquipo> Jugadores { get; set; }
}