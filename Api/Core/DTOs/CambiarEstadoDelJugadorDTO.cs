namespace Api.Core.DTOs;

public class CambiarEstadoDelJugadorDTO
{
    public int JugadorId { get; set; }
    public int JugadorEquipoId { get; set; }
    public string? Motivo { get; set; }
}