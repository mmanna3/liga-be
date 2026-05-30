namespace Api.Core.DTOs;

public class ReporteJugadoresHabilitadosFilaDTO
{
    public int TorneoId { get; set; }
    public string NombreTorneo { get; set; } = string.Empty;
    public int Enero { get; set; }
    public int Febrero { get; set; }
    public int Marzo { get; set; }
    public int Abril { get; set; }
    public int Mayo { get; set; }
    public int Junio { get; set; }
    public int Julio { get; set; }
    public int Agosto { get; set; }
    public int Septiembre { get; set; }
    public int Octubre { get; set; }
    public int Noviembre { get; set; }
    public int Diciembre { get; set; }
    public int TotalEnElAnio { get; set; }
}

public class ReporteJugadoresHabilitadosPorAgrupadorDeTorneoDTO
{
    public string NombreAgrupador { get; set; } = string.Empty;
    public List<ReporteJugadoresHabilitadosFilaDTO> Torneos { get; set; } = [];
}
