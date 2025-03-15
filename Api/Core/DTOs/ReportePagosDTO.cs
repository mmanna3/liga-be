namespace Api.Core.DTOs;

public class ReportePagosDTO
{
    public string NombreEquipo { get; set; } = string.Empty;
    public int Mes { get; set; }
    public int Anio { get; set; }
    public int CantidadJugadoresPagados { get; set; }
} 