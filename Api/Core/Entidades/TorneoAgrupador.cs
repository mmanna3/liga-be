namespace Api.Core.Entidades;

public class TorneoAgrupador : Entidad
{
    public required string Nombre { get; set; }
    public required bool VisibleEnApp { get; set; }
    public virtual ICollection<Torneo> Torneos { get; set; } = null!;
}