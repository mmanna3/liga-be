namespace Api.Core.Entidades;

public class FaseTodosContraTodos : TorneoFase
{
    public virtual ICollection<ZonaTodosContraTodos> Zonas { get; set; } = null!;
}
