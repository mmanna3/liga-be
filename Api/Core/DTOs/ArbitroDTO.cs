using System.ComponentModel.DataAnnotations;
using Api.Core.Enums;

namespace Api.Core.DTOs;

public class ArbitroDTO : DTO
{
    [Required, MaxLength(9)]
    public string DNI { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public required string Nombre { get; set; }

    [Required, MaxLength(50)]
    public required string Apellido { get; set; }

    [MaxLength(20), RegularExpression(RegistroArbitroDTO.FormatoTelefonoCelular, ErrorMessage = RegistroArbitroDTO.MensajeFormatoTelefonoCelular)]
    public string? TelefonoCelular { get; set; }

    [MaxLength(150)]
    public string? Direccion { get; set; }

    [MaxLength(100)]
    public string? Localidad { get; set; }

    public DateTime? FechaNacimiento { get; set; }

    public TipoMovilidadArbitroEnum? TipoMovilidad { get; set; }

    public List<FranjaHorariaArbitroEnum> FranjasHorarias { get; set; } = new();

    public bool? TieneRelacionInstitucional { get; set; }

    [MaxLength(100)]
    public string? Institucion { get; set; }

    public List<int> TorneoAgrupadorIds { get; set; } = new();
    public List<ArbitroTorneoAgrupadorDTO> TorneoAgrupadores { get; set; } = new();
    public List<int> EquipoProhibidoIds { get; set; } = new();
    public List<ArbitroEquipoProhibidoDTO> EquiposProhibidos { get; set; } = new();
}
