namespace Api.Core.Entidades;

public class Delegado : Entidad
{
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public required int ClubId { get; set; }
    public virtual Club Club { get; set; }
    
    public required int UsuarioId { get; set; }
    public virtual Usuario Usuario { get; set; }
}