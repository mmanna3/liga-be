using System.ComponentModel.DataAnnotations;
using Api.Core.Enums;

namespace Api.Core.DTOs;

public class GestionarJugadorDTO
{
    public EstadoJugadorEnum Estado { get; set; }
    public int JugadorEquipoId { get; set; }
    public required string DNI { get; set; }
    
    public string? MotivoRechazo { get; set; }
}