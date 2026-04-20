using Api.Core.Enums;

namespace Api.Core.DTOs;

/// <summary>
/// DTO para Jornada. Soporta los tipos: Normal, Libre, Interzonal, SinEquipos.
/// </summary>
public class JornadaDTO : DTO
{
    public required string Tipo { get; set; } // "Normal", "Libre", "Interzonal", "SinEquipos"

    public required bool ResultadosVerificados { get; set; }

    public int FechaId { get; set; }

    // JornadaNormal
    public int? LocalId { get; set; }
    public int? VisitanteId { get; set; }
    public string? Local { get; set; }
    public string? Visitante { get; set; }

    // JornadaInterzonal (número de jornada interzonal en la fecha)
    public int? Numero { get; set; }

    // JornadaLibre e Interzonal (equipo único + lado en el fixture)
    public int? EquipoId { get; set; }
    public string? Equipo { get; set; }
    public LocalVisitanteEnum? LocalOVisitante { get; set; }

    public List<PartidoDTO>? Partidos { get; set; }
}
