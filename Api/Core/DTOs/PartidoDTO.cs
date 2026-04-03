namespace Api.Core.DTOs;

public class PartidoDTO : DTO
{
    /// <summary>Solo lectura al listar; en creación/modificación basta <see cref="CategoriaId"/>.</summary>
    public string? Categoria { get; set; }
    public required int CategoriaId { get; set; }
    public required string ResultadoLocal { get; set; } = string.Empty;
    public required string ResultadoVisitante { get; set; } = string.Empty;

    /// <summary>Opcional. Solo dígitos cuando tiene valor; null si no aplica.</summary>
    public string? PenalesLocal { get; set; }

    /// <summary>Opcional. Solo dígitos cuando tiene valor; null si no aplica.</summary>
    public string? PenalesVisitante { get; set; }
}