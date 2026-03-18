namespace Api.Core.DTOs;

/// <summary>
/// DTO para Jornada. Soporta los tipos: Normal, Libre, Interzonal.
/// </summary>
public class JornadaDTO : DTO
{
    public required string Tipo { get; set; } // "Normal", "Libre", "Interzonal"

    public required bool ResultadosVerificados { get; set; }

    public int FechaId { get; set; }

    // JornadaNormal
    public int? LocalEquipoId { get; set; }
    public int? VisitanteEquipoId { get; set; }

    // JornadaLibre y JornadaInterzonal
    public int? EquipoId { get; set; }

    // JornadaInterzonal
    public int? LocalOVisitanteId { get; set; }
}
