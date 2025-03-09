using System.ComponentModel.DataAnnotations;
using Api.Core.Enums;

namespace Api.Core.DTOs;

public class GestionarJugadorDTO : JugadorBaseDTO
{
    public EstadoJugadorEnum Estado { get; set; }
    public int JugadorEquipoId { get; set; }
    public string? MotivoRechazo { get; set; }
}