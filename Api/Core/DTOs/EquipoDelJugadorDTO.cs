using System.ComponentModel.DataAnnotations;
using Api.Core.Enums;

namespace Api.Core.DTOs;

public class EquipoDelJugadorDTO : DTO
{
    public string Nombre { get; set; } = string.Empty;

    public string Club { get; set; } = string.Empty;

    public EstadoJugadorEnum Estado { get; set; }
    
    public string? Motivo { get; set; } = string.Empty;
}