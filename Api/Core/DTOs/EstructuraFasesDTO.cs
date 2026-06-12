using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class EstructuraFasesDTO
{
    [Required]
    public required List<EstructuraFasesItemDTO> Items { get; set; }
}

public class EstructuraFasesItemDTO
{
    [Required]
    public required string Tipo { get; set; }

    public int? FaseId { get; set; }

    public int? GrupoId { get; set; }

    public List<EstructuraFasesItemDTO>? Items { get; set; }
}
