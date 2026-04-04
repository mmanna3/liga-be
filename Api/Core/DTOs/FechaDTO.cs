using Api.Core;

namespace Api.Core.DTOs;

public abstract class FechaDTO : DTO, IEsVisibleEnApp
{
    public DateOnly? Dia { get; set; }

    public int ZonaId { get; set; }

    public required bool EsVisibleEnApp { get; set; }

    /// <summary>
    /// Null = no modificar. Lista vacía = borrar todas. Con items = crear/modificar/eliminar según ids.
    /// </summary>
    public List<JornadaDTO>? Jornadas { get; set; }
}
