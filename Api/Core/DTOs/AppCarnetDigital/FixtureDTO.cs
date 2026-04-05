namespace Api.Core.DTOs.AppCarnetDigital;

public class FixtureDTO
{
    public ICollection<FixtureFechaDTO> Fechas { get; set; } = [];
}

public class FixtureFechaDTO
{
    public ICollection<FixturePartidoDTO> Partidos { get; set; } = [];
    public string Titulo { get; set; } = string.Empty;
    public string Dia { get; set; } = string.Empty;
}

public class FixturePartidoDTO
{
    public string LocalEscudo { get; set; } = string.Empty;
    public string VisitanteEscudo { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;
    public string Visitante { get; set; } = string.Empty;

}
