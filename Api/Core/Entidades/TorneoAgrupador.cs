using Api.Core.Entidades.EntidadesConValoresPredefinidos;

namespace Api.Core.Entidades;

public class TorneoAgrupador : Entidad, IEsVisibleEnApp
{
    public required string Nombre { get; set; }
    public required bool EsVisibleEnApp { get; set; }
    public virtual ICollection<Torneo> Torneos { get; set; } = null!;
    public virtual Color Color { get; set; } = null!;
    public required int ColorId { get; set; }
}