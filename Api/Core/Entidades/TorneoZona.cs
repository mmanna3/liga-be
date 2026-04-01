namespace Api.Core.Entidades;

public abstract class TorneoZona : Entidad
{
    public required string Nombre { get; set; }
    public required int TorneoFaseId { get; set; }
    public virtual ICollection<EquipoZona> EquiposZona { get; set; } = null!;
}
