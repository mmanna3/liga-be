namespace Api.Core.Entidades;

public class FaseTodosContraTodos : TorneoFase
{
    public virtual ICollection<TorneoZona> Zonas { get; set; } = null!;
}
