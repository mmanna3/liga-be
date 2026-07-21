namespace Api.Core.DTOs;

public class BusquedaLogsDTO
{
    public string Texto { get; set; } = string.Empty;
    public int Dias { get; set; }
    public int MaxResultados { get; set; }
    public bool Truncado { get; set; }
    public IList<string> Advertencias { get; set; } = new List<string>();
    public IList<LogHitDTO> Resultados { get; set; } = new List<LogHitDTO>();
}
