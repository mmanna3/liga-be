namespace Api.Core.Entidades;

public class Club : Entidad
{
    public required string Nombre { get; set; }
    public virtual ICollection<Equipo> Equipos { get; set; } = null!;
    public virtual ICollection<DelegadoClub> DelegadoClubs { get; set; } = null!;
}