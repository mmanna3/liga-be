namespace Api.Core.DTOs;

public class FechaEliminacionDirectaDTO : FechaDTO
{
    public required int InstanciaId { get; set; }

    /// <summary>
    /// Solo informativo al leer; no se persiste en creación/modificación.
    /// </summary>
    public string? InstanciaNombre { get; set; }
}
