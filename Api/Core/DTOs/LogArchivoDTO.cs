namespace Api.Core.DTOs;

public class LogArchivoDTO
{
    public string Nombre { get; set; } = string.Empty;
    public long TamanioBytes { get; set; }
    public DateTime UltimaModificacion { get; set; }
}
