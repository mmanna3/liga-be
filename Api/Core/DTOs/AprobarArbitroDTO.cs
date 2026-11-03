namespace Api.Core.DTOs;

public class AprobarArbitroDTO
{
    public List<int> TorneoAgrupadorIds { get; set; } = new();
    public List<int> EquipoProhibidoIds { get; set; } = new();
}
