namespace Api.Core.DTOs;

/// <summary>
/// DTO plano para el listado de equipos (home). No incluye la colección de
/// jugadores (sólo su cantidad) ni el árbol de zonas/torneos: sólo lo que la
/// tabla del front necesita.
/// </summary>
public class EquipoListadoDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ClubNombre { get; set; } = string.Empty;
    public string CodigoAlfanumerico { get; set; } = string.Empty;
    public List<string> Torneos { get; set; } = [];
    public int CantidadJugadores { get; set; }
}
