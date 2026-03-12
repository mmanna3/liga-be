namespace Api.Core.Entidades;

public class TorneoZona : Entidad
{
    public required string Nombre { get; set; }
    public virtual TorneoFase TorneoFase { get; set; } = null!;
    public required int TorneoFaseId { get; set; }
    public virtual ICollection<Equipo> Equipos { get; set; } = null!;
    public virtual ICollection<EquipoZonaNoExcluyente> EquiposZonaNoExcluyente { get; set; } = null!;
    public virtual ICollection<TorneoFecha> Fechas { get; set; } = null!;
}