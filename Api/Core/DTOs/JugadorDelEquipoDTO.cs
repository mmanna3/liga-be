using System.ComponentModel.DataAnnotations;
using Api.Core.Enums;

namespace Api.Core.DTOs;

public class JugadorDelEquipoDTO : DTO
{
    public string DNI { get; set; } = string.Empty;
    
    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public EstadoJugadorEnum Estado { get; set; }
    public int JugadorEquipoId { get; set; }
    
    public string? Motivo { get; set; }
}