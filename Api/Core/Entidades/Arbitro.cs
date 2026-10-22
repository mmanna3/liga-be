using System.ComponentModel.DataAnnotations;
using Api.Core.Entidades.EntidadesConValoresPredefinidos;
using Api.Core.Enums;

namespace Api.Core.Entidades;

public class Arbitro : Entidad
{
    [Required, MaxLength(9)] 
    public string DNI { get; set; } = string.Empty;

    [MaxLength(50), Required] 
    public required string Nombre { get; set; }
    
    [MaxLength(50), Required]
    public required string Apellido { get; set; }

    [MaxLength(20)]
    public string? TelefonoCelular { get; set; }

    [MaxLength(150)]
    public string? Direccion { get; set; }

    [MaxLength(100)]
    public string? Localidad { get; set; }

    public DateTime? FechaNacimiento { get; set; }

    public int? TipoMovilidadArbitroId { get; set; }
    public virtual TipoMovilidadArbitro? TipoMovilidadArbitro { get; set; }

    public bool? TieneRelacionInstitucional { get; set; }

    [MaxLength(100)]
    public string? Institucion { get; set; }

    public int EstadoArbitroId { get; set; } = (int)EstadoArbitroEnum.Activo;
    public virtual EstadoArbitro EstadoArbitro { get; set; } = null!;

    public virtual ICollection<ArbitroTorneoAgrupador> ArbitroTorneoAgrupadores { get; set; } = new List<ArbitroTorneoAgrupador>();
    public virtual ICollection<ArbitroEquipoProhibido> ArbitroEquiposProhibidos { get; set; } = new List<ArbitroEquipoProhibido>();
    public virtual ICollection<ArbitroFranjaHoraria> ArbitroFranjasHorarias { get; set; } = new List<ArbitroFranjaHoraria>();
}
