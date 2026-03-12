using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Core.Entidades;

public class Equipo : Entidad
{
    public required string Nombre { get; set; }

    [ForeignKey("Club")]
    public required int ClubId { get; set; }
    public virtual Club Club { get; set; } = null!;

    public virtual required ICollection<JugadorEquipo> Jugadores { get; set; }

    public int? ZonaExcluyenteId { get; set; }
    public virtual TorneoZona? ZonaExcluyente { get; set; }
    public virtual ICollection<EquipoZonaNoExcluyente> ZonasNoExcluyentes { get; set; } = null!;
}