using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class ReordenarFasesDTO
{
    [Required, MinLength(1)]
    public required List<int> FaseIds { get; set; }
}
