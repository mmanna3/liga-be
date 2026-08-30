namespace Api.Core.DTOs;

/// <summary>
/// DTO plano para el listado de delegados (home + modal de selección para PDF).
/// No incluye fotos, ni datos de contacto, ni el grafo de
/// club → equipos → zonas → torneos: sólo lo que las tablas muestran.
/// </summary>
public class DelegadoListadoDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? NombreUsuario { get; set; }
    public int? JugadorId { get; set; }
    public bool BlanqueoPendiente { get; set; }
    public List<DelegadoClubResumenDTO> Clubs { get; set; } = [];
}

public class DelegadoClubResumenDTO
{
    public string ClubNombre { get; set; } = string.Empty;
    public int EstadoDelegadoId { get; set; }
    public string EstadoDelegado { get; set; } = string.Empty;
}
