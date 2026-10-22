using System.ComponentModel.DataAnnotations;
using Api.Core.Enums;

namespace Api.Core.DTOs;

/// <summary>
/// Alta pública de un árbitro desde la app. Queda pendiente de aprobación.
/// </summary>
public class RegistroArbitroDTO
{
    public const string FormatoTelefonoCelular = @"^\+54911\d{8}$";
    public const string MensajeFormatoTelefonoCelular = "El celular debe tener el formato +54911 seguido de 8 dígitos.";

    [Required, RegularExpression(@"^\d{7,9}$", ErrorMessage = "El DNI debe tener entre 7 y 9 dígitos.")]
    public string DNI { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Apellido { get; set; } = string.Empty;

    [Required, RegularExpression(FormatoTelefonoCelular, ErrorMessage = MensajeFormatoTelefonoCelular)]
    public string TelefonoCelular { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Direccion { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Localidad { get; set; } = string.Empty;

    [Required]
    public DateTime? FechaNacimiento { get; set; }

    [Required]
    public TipoMovilidadArbitroEnum? TipoMovilidad { get; set; }

    [Required, MinLength(1)]
    public List<int> TorneoAgrupadorIds { get; set; } = new();

    [Required, MinLength(1)]
    public List<FranjaHorariaArbitroEnum> FranjasHorarias { get; set; } = new();

    [Required]
    public bool? TieneRelacionInstitucional { get; set; }

    [MaxLength(100)]
    public string? Institucion { get; set; }
}
