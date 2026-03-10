namespace Api.Core.Entidades;

public class TorneoCategoria : Entidad
{
    public required string Nombre { get; set; }
    public required int AnioDesde { get; set; }
    public required int AnioHasta { get; set; }
    public virtual Torneo Torneo { get; set; } = null!;
    public required int TorneoId { get; set; }
}