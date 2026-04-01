using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class EquipoDeLaZonaDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string Club { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
}

public class ZonaDTO : DTO
{
    [Required]
    public required string Nombre { get; set; }

    public int FaseId { get; set; }

    /// <summary>
    /// Null = no modificar. Lista vacía = borrar todas. Con items = reemplazar.
    /// </summary>
    public List<EquipoDeLaZonaDTO>? Equipos { get; set; }
}
