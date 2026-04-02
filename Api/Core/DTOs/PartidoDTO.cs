using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class PartidoDTO : DTO
{
    public string Categoria { get; set; } = null!;
    public required int CategoriaId { get; set; }
    public required string ResultadoLocal { get; set; } = string.Empty;
    public required string ResultadoVisitante { get; set; } = string.Empty;
}