namespace Api.Core.DTOs.AppCarnetDigital;

public class EliminacionDirectaDTO
{
    public ICollection<InstanciasDTO> Instancias { get; set; } = [];
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

