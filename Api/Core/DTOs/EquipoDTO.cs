namespace Api.Core.DTOs;

public class EquipoDTO : DTO
{
    public required string Nombre { get; set; }
    
    public virtual required int ClubId { get; set; }
    public string? CodigoAlfanumerico { get; set; }
    public string? ClubNombre { get; set; }
    
    public ZonaDTO? ZonaExcluyente { get; set; }
    public List<ZonaDTO> ZonasNoExcluyentes { get; set; } = [];

    public ICollection<JugadorDelEquipoDTO>? Jugadores { get; set; }
}