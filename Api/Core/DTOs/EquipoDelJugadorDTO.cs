using Api.Core.Enums;

namespace Api.Core.DTOs;

public class EquipoDelJugadorDTO : DTO
{
    public int EquipoId { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public string Club { get; set; } = string.Empty;

    public string Torneo { get; set; } = string.Empty;
    
    public EstadoJugadorEnum Estado { get; set; }
    
    public string? Motivo { get; set; } = string.Empty;
    public DateTime? FechaPagoDeFichaje { get; set; }
    
}