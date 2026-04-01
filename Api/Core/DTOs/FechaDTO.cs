namespace Api.Core.DTOs;

public class FechaDTO : DTO
{
    public DateOnly? Dia { get; set; }

    public required int Numero { get; set; }

    public int ZonaId { get; set; }

    public int? InstanciaEliminacionDirectaId { get; set; }
    public string? InstanciaEliminacionDirectaNombre { get; set; }

    public required bool EsVisibleEnApp { get; set; }

    /// <summary>
    /// Null = no modificar. Lista vacía = borrar todas. Con items = crear/modificar/eliminar según ids.
    /// </summary>
    public List<JornadaDTO>? Jornadas { get; set; }
}
