using Api.Core.Entidades.EntidadesConValoresPredefinidos;

namespace Api.Core.Entidades;

public class FechaEliminacionDirecta : Fecha
{
    public required int InstanciaId { get; set; }
    public virtual InstanciaEliminacionDirecta Instancia { get; set; } = null!;

    public virtual ZonaEliminacionDirecta Zona { get; set; } = null!;
}
