using Api.Core;

namespace Api.Core.Entidades;

public abstract class Fecha : Entidad, IEsVisibleEnApp
{
    public DateOnly? Dia { get; set; }
    public required int ZonaId { get; set; }
    public required bool EsVisibleEnApp { get; set; }
    public virtual ICollection<Jornada> Jornadas { get; set; } = [];
}
