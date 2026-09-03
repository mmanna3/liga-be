using System.ComponentModel.DataAnnotations;
using Api.Core;

namespace Api.Core.DTOs;

public class TorneoAgrupadorDTO : DTO, IEsVisibleEnApp
{
    [Required]
    public required string Nombre { get; set; }
    public required string Color { get; set; }
    public required bool EsVisibleEnApp { get; set; }

    /// <summary>
    /// Habilita la acción de limpiar tarjetas amarillas en los torneos del agrupador (Futsal).
    /// </summary>
    public bool PermiteLimpiarAmarillas { get; set; }

    public int CantidadDeTorneos { get; set; }

    /// <summary>
    /// Lista de torneos del agrupador. Solo se incluye al obtener por ID.
    /// </summary>
    public ICollection<TorneoDTO>? Torneos { get; set; }
}
