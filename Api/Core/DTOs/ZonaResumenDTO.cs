namespace Api.Core.DTOs;

/// <summary>
/// Vista resumida de una zona de torneo (p. ej. equipos del delegado).
/// </summary>
public class ZonaResumenDTO
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
