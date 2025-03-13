namespace Api.Core.DTOs.CambiosDeEstadoJugador;

public class RechazarJugadorDTO : JugadorBaseDTO
{
    public int JugadorId { get; set; }
    public int JugadorEquipoId { get; set; }
    public string? Motivo { get; set; }
}