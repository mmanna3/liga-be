using Api.Core;

namespace Api.Core.Entidades;

public class Torneo : Entidad, IEsVisibleEnApp
{
    public required string Nombre { get; set; }
    public required int Anio { get; set; }

    public required bool EsVisibleEnApp { get; set; }
    public required bool SeVenLosGolesEnTablaDePosiciones { get; set; }

    public int TorneoAgrupadorId { get; set; }
    public virtual TorneoAgrupador TorneoAgrupador { get; set; } = null!;

    public int? FaseAperturaId { get; set; }
    public virtual Fase? FaseApertura { get; set; }

    public int? FaseClausuraId { get; set; }
    public virtual Fase? FaseClausura { get; set; }

    public virtual ICollection<TorneoCategoria> Categorias { get; set; } = null!;
    public virtual ICollection<Fase> Fases { get; set; } = null!;
}