namespace Api.Core.Entidades;

public abstract class Zona : Entidad
{
    public required string Nombre { get; set; }
    public required int FaseId { get; set; }
    public virtual ICollection<EquipoZona> EquiposZona { get; set; } = null!;
    public virtual ICollection<LeyendaTablaPosiciones> LeyendasTablaPosiciones { get; set; } = new List<LeyendaTablaPosiciones>();
}
