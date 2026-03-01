using Microsoft.EntityFrameworkCore;

namespace Api.Core.Entidades;

[Index(nameof(DelegadoId), nameof(ClubId), IsUnique = true)]
public class DelegadoClub : Entidad
{
    public int DelegadoId { get; set; }
    public virtual Delegado Delegado { get; set; } = null!;
    public int ClubId { get; set; }
    public virtual Club Club { get; set; } = null!;
    public int EstadoDelegadoId { get; set; }
    public virtual EstadoDelegado EstadoDelegado { get; set; } = null!;
}
