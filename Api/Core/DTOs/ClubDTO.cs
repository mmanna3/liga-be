using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class ClubDTO : DTO
{
    [Required]
    public required string Nombre { get; set; }

    /// <summary>
    /// Ruta relativa del escudo del club (ej. /Imagenes/Escudos/1.jpg o /Imagenes/Escudos/default.jpg).
    /// </summary>
    public string Escudo { get; set; } = string.Empty;
    
    public ICollection<EquipoDTO>? Equipos { get; set; }
    public ICollection<DelegadoDTO>? Delegados { get; set; }
}