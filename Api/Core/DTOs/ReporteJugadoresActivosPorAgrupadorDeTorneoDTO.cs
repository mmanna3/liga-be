namespace Api.Core.DTOs;

public class ReporteJugadoresActivosEquipoDTO
{
    public int EquipoId { get; set; }
    public string NombreEquipo { get; set; } = string.Empty;
    public int CantidadJugadoresActivos { get; set; }
}

public class ReporteJugadoresActivosTorneoDTO
{
    public int TorneoId { get; set; }
    public string NombreTorneo { get; set; } = string.Empty;
    public int CantidadJugadoresActivos { get; set; }
    public List<ReporteJugadoresActivosEquipoDTO> Equipos { get; set; } = [];
}

public class ReporteJugadoresActivosPorAgrupadorDeTorneoDTO
{
    public string NombreAgrupador { get; set; } = string.Empty;
    public List<ReporteJugadoresActivosTorneoDTO> Torneos { get; set; } = [];
}
