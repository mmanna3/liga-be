using System.ComponentModel.DataAnnotations;
using Api.Core;

namespace Api.Core.DTOs;

public class FaseCategoriaDTO : DTO
{
    [Required]
    public required string Nombre { get; set; }

    public required int AnioDesde { get; set; }
    public required int AnioHasta { get; set; }

    [Required]
    public required int Orden { get; set; }

    public int FaseId { get; set; }
}
