using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades;

public class SponsorWebPublica : Entidad
{
    [MaxLength(200)]
    public required string Nombre { get; set; }

    public int Orden { get; set; }
}
