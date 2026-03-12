namespace Api.Core.DTOs;

public class EquipoParaZonasDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Club { get; set; } = string.Empty;
    public string CodigoAlfanumerico { get; set; } = string.Empty;
    public List<ZonaDTO> Zonas { get; set; } = [];
}
