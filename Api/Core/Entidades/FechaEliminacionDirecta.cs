using Api.Core.Entidades.EntidadesConValoresPredefinidos;

namespace Api.Core.Entidades;

public class FechaEliminacionDirecta : TorneoFecha
{
    public required int InstanciaEliminacionDirectaId { get; set; }
    public virtual InstanciaEliminacionDirecta InstanciaEliminacionDirecta { get; set; } = null!;

    public virtual ZonaEliminacionDirecta Zona { get; set; } = null!;
}
