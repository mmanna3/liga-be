namespace Api.Core.DTOs;

public class EquipoDTO : DTO
{
    public required string Nombre { get; set; }
    
    public virtual required int ClubId { get; set; }
    
    public string? ClubNombre { get; set; }
    
    public ICollection<string>? Jugadores { get; set; }
}