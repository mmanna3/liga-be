namespace Api.Core.DTOs;

public class FixtureAlgoritmoDTO : DTO
{
    public required int FixtureAlgoritmoId { get; set; }
    public required int CantidadDeEquipos { get; set; }
    public List<FixtureAlgoritmoFechaDTO> Fechas { get; set; } = [];
}

public class FixtureAlgoritmoFechaDTO : DTO
{
    public required int Fecha { get; set; }
    public required int EquipoLocal { get; set; }
    public required int EquipoVisitante { get; set; }
}
