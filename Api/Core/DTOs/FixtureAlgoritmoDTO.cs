namespace Api.Core.DTOs;

public class FixtureAlgoritmoDTO : DTO
{
    public required int FixtureAlgoritmoId { get; set; }
    public required int CantidadDeFechas { get; set; }
    public List<FixtureAlgoritmoPartidoDTO> Partidos { get; set; } = [];
}

public class FixtureAlgoritmoPartidoDTO : DTO
{
    public required int Fecha { get; set; }
    public required int EquipoLocal { get; set; }
    public required int EquipoVisitante { get; set; }
}