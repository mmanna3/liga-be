namespace Api.Core.Entidades;

public class FaseTodosContraTodos : Fase
{
    public virtual ICollection<ZonaTodosContraTodos> Zonas { get; set; } = null!;
}
