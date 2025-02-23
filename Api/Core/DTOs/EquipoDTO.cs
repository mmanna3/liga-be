namespace Api.Core.DTOs;

public class EquipoDTO : DTO
{
    public required string Nombre { get; set; }
    public virtual required ClubDTO Club { get; set; }
}