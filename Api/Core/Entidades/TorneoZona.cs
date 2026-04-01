namespace Api.Core.Entidades;

public class TorneoZona : Entidad
{
    public required string Nombre { get; set; }
    public virtual FaseTodosContraTodos TorneoFase { get; set; } = null!;
    public required int TorneoFaseId { get; set; }
    public virtual ICollection<EquipoZona> EquiposZona { get; set; } = null!;
    public virtual ICollection<TorneoFecha> Fechas { get; set; } = null!;
}