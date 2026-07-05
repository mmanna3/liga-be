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
    public string? TelefonoCelular { get; set; }
    public required List<JornadaAsignadaResumenDTO> JornadasAsignadasEnProximasFechas { get; set; }
    public List<int> EquiposProhibidosIds { get; set; } = new();
    public List<JornadaAsignadaRecienteDTO> JornadasEnUltimasFechas { get; set; } = new();
}

public class JornadaAsignadaRecienteDTO
{
    public required int ZonaId { get; set; }
    public required int JornadaId { get; set; }
    public int? FechaNumero { get; set; }
    public string? InstanciaNombre { get; set; }
    public required int LocalEquipoId { get; set; }
    public required int VisitanteEquipoId { get; set; }
    public required string Local { get; set; }
    public required string Visitante { get; set; }
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
    public WhatsappAsignacionDTO? Whatsapp { get; set; }
}

public class TorneoAsignacionDTO
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public string? HorarioDeJuego { get; set; }
    public required List<FaseAsignacionDTO> Fases { get; set; }
}

public class FaseAsignacionDTO
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public required List<FaseCategoriaDTO> Categorias { get; set; }
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
    public required string TorneoNombre { get; set; }
    public required string FaseNombre { get; set; }
    public required string ZonaNombre { get; set; }
    public required int ZonaId { get; set; }
    public required string Local { get; set; }
    public required string Visitante { get; set; }
    public required int LocalEquipoId { get; set; }
    public required int VisitanteEquipoId { get; set; }
    public required string NombreClubLocal { get; set; }
    public string? DireccionLocal { get; set; }
    public string? LocalidadLocal { get; set; }
    public required List<ArbitroAsignadoDTO> ArbitrosAsignados { get; set; }
}

public class ArbitroAsignadoDTO
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public string? TelefonoCelular { get; set; }
    public required int Orden { get; set; }
    public required bool WhatsappEnviado { get; set; }
    public WhatsappAsignacionDTO? Whatsapp { get; set; }
}

public class WhatsappAsignacionDTO
{
    public required bool Enviado { get; set; }
    public string? HorarioInicio { get; set; }
    public string? Observaciones { get; set; }
    public required List<string> CategoriasNombres { get; set; }
    public DateTime? EnviadoEn { get; set; }
}

public class WhatsappCategoriaSnapshotDTO
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
}

public class MarcarWhatsappEnviadoArbitroJornadaDTO
{
    public string? HorarioInicio { get; set; }
    public string? Observaciones { get; set; }
    public List<WhatsappCategoriaSnapshotDTO> Categorias { get; set; } = [];
}

public class AsignarArbitrosJornadaDTO
{
    public required List<int> ArbitroIds { get; set; }
}

public class AsignacionHistoricaArbitrosPorAgrupadorDTO
{
    public required List<ArbitroElegibleAsignacionDTO> ArbitrosElegibles { get; set; }
    public required List<TorneoAsignacionHistoricaDTO> Torneos { get; set; }
    public required List<ArbitroConJornadasHistoricasDTO> ArbitrosConJornadas { get; set; }
}

public class TorneoAsignacionHistoricaDTO
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public string? HorarioDeJuego { get; set; }
    public required List<FaseAsignacionHistoricaDTO> Fases { get; set; }
}

public class FaseAsignacionHistoricaDTO
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public required List<FaseCategoriaDTO> Categorias { get; set; }
    public required List<ZonaAsignacionHistoricaDTO> Zonas { get; set; }
}

public class ZonaAsignacionHistoricaDTO
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public required List<FechaHistoricaAsignacionDTO> FechasHistoricas { get; set; }
}

public class FechaHistoricaAsignacionDTO
{
    public required int FechaId { get; set; }
    public required DateOnly Dia { get; set; }
    public required string DiaSemana { get; set; }
    public int? Numero { get; set; }
    public string? InstanciaNombre { get; set; }
    public required List<JornadaAsignacionDTO> Jornadas { get; set; }
}

public class ArbitroConJornadasHistoricasDTO
{
    public required int ArbitroId { get; set; }
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public required List<JornadaAsignadaResumenDTO> JornadasHistoricas { get; set; }
}
