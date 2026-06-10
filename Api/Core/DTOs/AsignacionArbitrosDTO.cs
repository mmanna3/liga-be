namespace Api.Core.DTOs;

public class AsignacionArbitrosPorAgrupadorDTO
{
    public required List<ArbitroElegibleAsignacionDTO> ArbitrosElegibles { get; set; }
    public required List<TorneoAsignacionDTO> Torneos { get; set; }
    public required List<ArbitroConJornadasAsignacionDTO> ArbitrosConJornadas { get; set; }
}

public class ArbitroElegibleAsignacionDTO
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public required List<JornadaAsignadaResumenDTO> JornadasAsignadasEnProximasFechas { get; set; }
}

public class ArbitroConJornadasAsignacionDTO
{
    public required int ArbitroId { get; set; }
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public required List<JornadaAsignadaResumenDTO> JornadasProximaFecha { get; set; }
}

public class JornadaAsignadaResumenDTO
{
    public required int JornadaId { get; set; }
    public required DateOnly Dia { get; set; }
    public required string DiaSemana { get; set; }
    public required string TorneoNombre { get; set; }
    public required string FaseNombre { get; set; }
    public required string ZonaNombre { get; set; }
    public required string Local { get; set; }
    public required string Visitante { get; set; }
    public string? LocalidadLocal { get; set; }
    public int? FechaNumero { get; set; }
    public string? InstanciaNombre { get; set; }
    public required int Orden { get; set; }
}

public class TorneoAsignacionDTO
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public required List<FaseAsignacionDTO> Fases { get; set; }
}

public class FaseAsignacionDTO
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public required List<ZonaAsignacionDTO> Zonas { get; set; }
}

public class ZonaAsignacionDTO
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public ProximaFechaAsignacionDTO? ProximaFecha { get; set; }
}

public class ProximaFechaAsignacionDTO
{
    public required int FechaId { get; set; }
    public required DateOnly Dia { get; set; }
    public required string DiaSemana { get; set; }
    public int? Numero { get; set; }
    public string? InstanciaNombre { get; set; }
    public required List<JornadaAsignacionDTO> Jornadas { get; set; }
}

public class JornadaAsignacionDTO
{
    public required int Id { get; set; }
    public required DateOnly Dia { get; set; }
    public required string DiaSemana { get; set; }
    public required string Local { get; set; }
    public required string Visitante { get; set; }
    public string? LocalidadLocal { get; set; }
    public required List<ArbitroAsignadoDTO> ArbitrosAsignados { get; set; }
}

public class ArbitroAsignadoDTO
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public required int Orden { get; set; }
}

public class AsignarArbitrosJornadaDTO
{
    public required List<int> ArbitroIds { get; set; }
}
