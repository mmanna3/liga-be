namespace Api.Core.Entidades;

public class ZonaTodosContraTodos : TorneoZona
{
    public virtual FaseTodosContraTodos Fase { get; set; } = null!;
    public virtual ICollection<FechaTodosContraTodos> Fechas { get; set; } = null!;
}
