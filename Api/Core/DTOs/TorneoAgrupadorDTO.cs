using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class TorneoAgrupadorDTO : DTO
{
    [Required]
    public required string Nombre { get; set; }

    public bool VisibleEnApp { get; set; }

    public int CantidadDeTorneos { get; set; }
}
