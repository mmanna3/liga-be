namespace Api.Core.Entidades;

public class Torneo : Entidad
{
    public required string Nombre { get; set; }
    public required int Anio { get; set; }

    public int TorneoAgrupadorId { get; set; }
    public virtual TorneoAgrupador TorneoAgrupador { get; set; } = null!;

    public virtual ICollection<TorneoCategoria> Categorias { get; set; } = null!;
    public virtual ICollection<TorneoFase> Fases { get; set; } = null!;
}