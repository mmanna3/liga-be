using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class TorneoFaseDTO : DTO
{
    [Required]
    public required int Numero { get; set; }

    public int TorneoId { get; set; }
    public int FaseFormatoId { get; set; }
    public string FaseFormatoNombre { get; set; } = string.Empty;
    public int? InstanciaEliminacionDirectaId { get; set; }
    public string? InstanciaEliminacionDirectaNombre { get; set; }
    public int FaseTipoDeVueltaId { get; set; }
    public string FaseTipoDeVueltaNombre { get; set; } = string.Empty;
    public int EstadoFaseId { get; set; }
    public string EstadoFaseNombre { get; set; } = string.Empty;
    public bool EsVisibleEnApp { get; set; }
}
