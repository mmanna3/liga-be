using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class TorneoCategoriaDTO : DTO
{
    [Required]
    public required string Nombre { get; set; }

    public required int AnioDesde { get; set; }
    public required int AnioHasta { get; set; }

    public int TorneoId { get; set; }
}
