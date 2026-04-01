namespace Api.Core.Entidades;

public class FechaTodosContraTodos : TorneoFecha
{
    public required int Numero { get; set; }

    public virtual ZonaTodosContraTodos Zona { get; set; } = null!;
}
