namespace Api.Core.Entidades;

public class ZonaTodosContraTodos : Zona
{
    public virtual FaseTodosContraTodos Fase { get; set; } = null!;
    public virtual ICollection<FechaTodosContraTodos> Fechas { get; set; } = null!;
}
