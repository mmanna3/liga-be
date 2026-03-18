using System.ComponentModel.DataAnnotations.Schema;
using Api.Core.Entidades.EntidadesConValoresPredefinidos;

namespace Api.Core.Entidades;

public class TorneoFecha : Entidad
{
    public required DateOnly Dia { get; set; }
    public required int Numero { get; set; }
    public virtual TorneoZona Zona { get; set; } = null!;

    [ForeignKey("Zona")]
    public required int ZonaId { get; set; }
    public int? InstanciaEliminacionDirectaId { get; set; }
    public virtual InstanciaEliminacionDirecta? InstanciaEliminacionDirecta { get; set; }
    public required bool EsVisibleEnApp { get; set; }
    public virtual ICollection<Jornada> Jornadas { get; set; } = [];
}