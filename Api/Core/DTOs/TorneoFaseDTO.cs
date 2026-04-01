using System.ComponentModel.DataAnnotations;
using Api.Core.Enums;

namespace Api.Core.DTOs;

public class ZonaDeFaseDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CantidadDeEquipos { get; set; }
}

public class TorneoFaseDTO : DTO
{
    [Required, MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;
    [Required]
    public required int Numero { get; set; }

    public int TorneoId { get; set; }
    [Required]
    public TipoDeFaseEnum TipoDeFase { get; set; }
    public string TipoDeFaseNombre { get; set; } = string.Empty;
    public int? InstanciaEliminacionDirectaId { get; set; }
    public string? InstanciaEliminacionDirectaNombre { get; set; }
    public int EstadoFaseId { get; set; }
    public string EstadoFaseNombre { get; set; } = string.Empty;
    public bool EsVisibleEnApp { get; set; }
    public bool SePuedeEditar { get; set; }
    public List<ZonaDeFaseDTO> Zonas { get; set; } = [];
}
