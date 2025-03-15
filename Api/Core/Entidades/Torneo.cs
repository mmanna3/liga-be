namespace Api.Core.Entidades;

public class Torneo : Entidad
{
    public required string Nombre { get; set; }
    public virtual ICollection<Equipo> Equipos { get; set; } = null!;
} 