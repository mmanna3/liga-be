using Api.Core;

namespace Api.Core.Entidades;

public class TorneoAgrupador : Entidad, IEsVisibleEnApp
{
    public required string Nombre { get; set; }
    public required bool EsVisibleEnApp { get; set; }
    public virtual ICollection<Torneo> Torneos { get; set; } = null!;
}