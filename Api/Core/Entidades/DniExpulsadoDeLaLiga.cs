using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades;

public class DniExpulsadoDeLaLiga : Entidad
{
    [MaxLength(1000)]
    public string Explicacion { get; set; } = string.Empty;

    public required int DNI { get; set; }
}