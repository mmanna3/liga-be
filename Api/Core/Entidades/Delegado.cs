using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades;

public class Delegado : Entidad
{
    [MaxLength(14)] 
    public required string Nombre { get; set; }
    
    [MaxLength(14)]
    public required string Apellido { get; set; }
    
    public required int ClubId { get; set; }
    public virtual Club Club { get; set; } = null!;

    public required int UsuarioId { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;
}