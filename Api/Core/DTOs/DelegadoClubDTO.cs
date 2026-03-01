namespace Api.Core.DTOs;

public class DelegadoClubDTO : DTO
{
    public int ClubId { get; set; }
    public string ClubNombre { get; set; } = string.Empty;
    public int EstadoDelegadoId { get; set; }
    public EstadoDelegadoDTO? EstadoDelegado { get; set; }
    public List<string> EquiposDelClub { get; set; } = new();
}
