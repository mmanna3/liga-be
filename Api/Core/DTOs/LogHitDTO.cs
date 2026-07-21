namespace Api.Core.DTOs;

public class LogHitDTO
{
    public string Archivo { get; set; } = string.Empty;
    public int Linea { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public DateTime? Fecha { get; set; }
}
