using System.ComponentModel.DataAnnotations;
using Api.Core;

namespace Api.Core.DTOs;

public class GrupoDeFasesDTO : DTO, IEsVisibleEnApp
{
    [Required, MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public required int Numero { get; set; }

    public int TorneoId { get; set; }

    public int? GrupoDeFasesPadreId { get; set; }

    public required bool EsVisibleEnApp { get; set; }
}
