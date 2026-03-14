namespace Api.Core.DTOs;

public class ZonaDTO
{
    public int? Id { get; set; }
    public string? Nombre { get; set; }
    public int? TorneoId { get; set; }
    public string Torneo { get; set; } = string.Empty;
    public string Agrupador { get; set; } = string.Empty;
    public int? AgrupadorId { get; set; }
    public string? Fase { get; set; }
    public int? FaseId { get; set; }
}
