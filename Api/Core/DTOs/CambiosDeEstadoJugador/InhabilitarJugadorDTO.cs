namespace Api.Core.DTOs.CambiosDeEstadoJugador;

public class InhabilitarJugadorDTO
{
    public int JugadorId { get; set; }
    public int JugadorEquipoId { get; set; }
    public string? Motivo { get; set; }
}