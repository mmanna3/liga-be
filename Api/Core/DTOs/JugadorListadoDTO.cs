using Api.Core.Enums;

namespace Api.Core.DTOs;

/// <summary>
/// DTO plano para el listado de jugadores (home). Una fila por
/// <c>JugadorEquipo</c> (igual que el listado anterior), sin el árbol de
/// entidades relacionadas: sólo lo que la tabla del front necesita.
/// </summary>
public class JugadorListadoDTO
{
    /// <summary>Id del jugador.</summary>
    public int Id { get; set; }
    public string DNI { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public int? DelegadoId { get; set; }

    public int JugadorEquipoId { get; set; }
    public int EquipoId { get; set; }
    public string EquipoNombre { get; set; } = string.Empty;
    public string Club { get; set; } = string.Empty;
    public string Torneo { get; set; } = string.Empty;
    public EstadoJugadorEnum Estado { get; set; }
}
