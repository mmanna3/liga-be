namespace Api.Core.DTOs.AppCarnetDigital;
public class PlanillaDeJuegoDTO
{
    public required ICollection<JugadoresPorCategoriaDTO> Planillas { get; set; }
    public required string Torneo { get; set; }
    public required string Equipo { get; set; }
}

public class JugadoresPorCategoriaDTO
{
    public required string Categoria { get; set; }
    public required ICollection<JugadorDatosPlanillaDTO> Jugadores { get; set; }
}

public class JugadorDatosPlanillaDTO
{
    public required string DNI { get; set; }
    public required string Nombre { get; set; }
    public required string Estado { get; set; }
}