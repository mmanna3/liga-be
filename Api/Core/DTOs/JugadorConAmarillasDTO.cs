namespace Api.Core.DTOs;

/// <summary>
/// Jugador de un torneo que tiene al menos una tarjeta amarilla en su ficha (JugadorEquipo).
/// Usado por la pantalla de limpieza de amarillas de torneos Futsal.
/// </summary>
public class JugadorConAmarillasDTO
{
    public int JugadorEquipoId { get; set; }
    public int JugadorId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public int EquipoId { get; set; }
    public string NombreEquipo { get; set; } = string.Empty;
    public int ClubId { get; set; }
    public string NombreClub { get; set; } = string.Empty;
    public int TarjetasAmarillas { get; set; }
}
