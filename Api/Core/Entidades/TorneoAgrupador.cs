using Api.Core.Entidades.EntidadesConValoresPredefinidos;

namespace Api.Core.Entidades;

public class TorneoAgrupador : Entidad, IEsVisibleEnApp
{
    public required string Nombre { get; set; }
    public required bool EsVisibleEnApp { get; set; }

    /// <summary>
    /// Habilita, para los torneos de este agrupador, la acción de limpiar (poner en cero)
    /// las tarjetas amarillas de todos los jugadores del torneo. Se usa para Futsal.
    /// </summary>
    public bool PermiteLimpiarAmarillas { get; set; }

    public virtual ICollection<Torneo> Torneos { get; set; } = null!;
    public virtual ICollection<ArbitroTorneoAgrupador> Arbitros { get; set; } = new List<ArbitroTorneoAgrupador>();
    public virtual Color Color { get; set; } = null!;
    public required int ColorId { get; set; }
}