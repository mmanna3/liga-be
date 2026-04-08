using System.ComponentModel.DataAnnotations;
using Api.Core;

namespace Api.Core.DTOs;

public class TorneoDTO : DTO, IEsVisibleEnApp
{
    [Required]
    public required string Nombre { get; set; }

    public required int Anio { get; set; }
    public required bool EsVisibleEnApp { get; set; }
    public required bool SeVenLosGolesEnTablaDePosiciones { get; set; }
    public int TorneoAgrupadorId { get; set; }
    public int? FaseAperturaId { get; set; }
    public string? FaseAperturaNombre { get; set; }
    public int? FaseClausuraId { get; set; }
    public string? FaseClausuraNombre { get; set; }
    public string TorneoAgrupadorNombre { get; set; } = string.Empty;
    public bool SePuedeEditar { get; set; }
    /// <summary>
    /// Null = no modificar. Lista vacía = borrar todas. Con items = reemplazar.
    /// </summary>
    public List<FaseDTO>? Fases { get; set; }
    /// <summary>
    /// Null = no modificar. Lista vacía = borrar todas. Con items = reemplazar.
    /// </summary>
    public List<TorneoCategoriaDTO>? Categorias { get; set; }
}