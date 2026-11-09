namespace Api.Core.DTOs.AppCarnetDigital;

public class EliminacionDirectaDTO
{
    public ICollection<InstanciasDTO> Instancias { get; set; } = [];

    /// <summary>
    /// Solo para zonas de un único equipo: el fixture no tiene partidos y el equipo es campeón directo
    /// (en la base es una Final con una jornada libre). En ese caso <see cref="Instancias"/> no incluye la Final.
    /// </summary>
    public CampeonEliminacionDirectaDTO? CampeonSinJugar { get; set; }
}

public class CampeonEliminacionDirectaDTO
{
    public string Equipo { get; set; } = string.Empty;
    public string Escudo { get; set; } = string.Empty;
}

public class InstanciasDTO
{
    public string Titulo { get; set; } = string.Empty;
    public string Dia { get; set; } = string.Empty;
    public ICollection<PartidoEliminacionDirectaDTO> Partidos { get; set; } = [];
}

public class PartidoEliminacionDirectaDTO
{
    public string EscudoLocal { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;
    public string ResultadoLocal { get; set; } = string.Empty;
    public string PenalesLocal { get; set; } = string.Empty;
    public string EscudoVisitante { get; set; } = string.Empty;
    public string Visitante { get; set; } = string.Empty;
    public string ResultadoVisitante { get; set; } = string.Empty;
    public string PenalesVisitante { get; set; } = string.Empty;
}

