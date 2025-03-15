using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Core.Entidades;

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