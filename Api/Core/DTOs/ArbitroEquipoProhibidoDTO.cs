namespace Api.Core.DTOs;

public class ArbitroEquipoProhibidoDTO
{
    public required int EquipoId { get; set; }
    public required string Nombre { get; set; }
    public string? ClubNombre { get; set; }
    public string? CodigoAlfanumerico { get; set; }
    public List<string> TorneosActuales { get; set; } = new();
}
