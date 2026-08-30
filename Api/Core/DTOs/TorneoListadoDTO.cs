using Api.Core.Enums;

namespace Api.Core.DTOs;

/// <summary>
/// DTO plano para el home de torneos y para los selectores de torneo.
/// Trae las fases y grupos de fases en forma resumida (para dibujar la
/// estructura), sin zonas, fechas, categorías ni estado de fase.
/// </summary>
public class TorneoListadoDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Anio { get; set; }
    public bool EsVisibleEnApp { get; set; }
    public string? HorarioDeJuego { get; set; }
    public int TorneoAgrupadorId { get; set; }
    public string TorneoAgrupadorNombre { get; set; } = string.Empty;
    public List<FaseResumenDTO> Fases { get; set; } = [];
    public List<GrupoDeFasesResumenDTO> GruposDeFases { get; set; } = [];
}

public class FaseResumenDTO
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int? GrupoDeFasesId { get; set; }
    public TipoDeFaseEnum TipoDeFase { get; set; }
    public string TipoDeFaseNombre { get; set; } = string.Empty;
    public bool SePuedeEditar { get; set; }
}

public class GrupoDeFasesResumenDTO
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int? GrupoDeFasesPadreId { get; set; }
}
