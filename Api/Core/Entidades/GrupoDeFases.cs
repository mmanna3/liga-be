using System.ComponentModel.DataAnnotations;
using Api.Core;

namespace Api.Core.Entidades;

public class GrupoDeFases : Entidad, IEsVisibleEnApp
{
    [Required, MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    public required int Numero { get; set; }

    public required bool EsVisibleEnApp { get; set; }

    public required int TorneoId { get; set; }
    public virtual Torneo Torneo { get; set; } = null!;

    public int? GrupoDeFasesPadreId { get; set; }
    public virtual GrupoDeFases? GrupoDeFasesPadre { get; set; }

    public virtual ICollection<GrupoDeFases> SubGrupos { get; set; } = [];
    public virtual ICollection<Fase> Fases { get; set; } = [];
}
