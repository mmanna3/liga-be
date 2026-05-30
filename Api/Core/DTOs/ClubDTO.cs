using System.ComponentModel.DataAnnotations;
using Api.Core.Enums;

namespace Api.Core.DTOs;

public class ClubDTO : DTO
{
    [Required]
    public required string Nombre { get; set; }

    /// <summary>
    /// Escudo del club en base64 con prefijo data:image (ej. data:image/jpeg;base64,...).
    /// </summary>
    public string Escudo { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Direccion { get; set; }

    public int CanchaTipoId { get; set; } = (int)CanchaTipoEnum.Consultar;
    public string CanchaTipo { get; set; } = nameof(CanchaTipoEnum.Consultar);

    [MaxLength(100)]
    public string? Localidad { get; set; }
    
    public ICollection<EquipoDTO>? Equipos { get; set; }
    public ICollection<DelegadoDTO>? Delegados { get; set; }
}