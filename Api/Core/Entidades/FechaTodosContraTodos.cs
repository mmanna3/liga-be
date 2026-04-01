namespace Api.Core.Entidades;

public class FechaTodosContraTodos : Fecha
{
    public required int Numero { get; set; }

    public virtual ZonaTodosContraTodos Zona { get; set; } = null!;
}
