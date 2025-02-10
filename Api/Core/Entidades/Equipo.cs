namespace Api.Core.Entidades;

public class Equipo : Entidad
{
    public required string Nombre { get; set; }
    public virtual Club Club { get; set; }
}