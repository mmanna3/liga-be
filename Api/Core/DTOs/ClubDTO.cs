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

    public bool? EsTechado { get; set; }

    /// <summary>
    /// Id en la tabla lookup <c>_CanchaTipo</c>. Usar este valor al crear o modificar un club.
    /// </summary>
    public int CanchaTipoId { get; set; } = (int)CanchaTipoEnum.Consultar;

    /// <summary>
    /// Nombre del tipo de cancha (desde la lookup), útil para mostrar en listados y consultas.
    /// No se usa para persistir: enviar <see cref="CanchaTipoId"/> en crear/modificar.
    /// </summary>
    public string CanchaTipo { get; set; } = nameof(CanchaTipoEnum.Consultar);

    [MaxLength(100)]
    public string? Localidad { get; set; }
    
    public ICollection<EquipoDTO>? Equipos { get; set; }
    public ICollection<DelegadoDTO>? Delegados { get; set; }
}