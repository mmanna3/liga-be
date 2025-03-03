using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class JugadorPendienteDeAprobacionDTO : JugadorDTO
{
    public required string FotoDNIFrente { get; set; }
    public required string FotoDNIDorso { get; set; }
}