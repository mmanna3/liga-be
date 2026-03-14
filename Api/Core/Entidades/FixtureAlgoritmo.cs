namespace Api.Core.Entidades;

public class FixtureAlgoritmo : Entidad
{
    public required int CantidadDeEquipos { get; set; }
    public virtual ICollection<FixtureAlgoritmoFecha> Fechas { get; set; } = [];
}
