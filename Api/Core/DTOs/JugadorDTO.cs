using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class JugadorDTO : JugadorBaseDTO, IFotosDTO
{
    public int EquipoInicialId { get; set; }
    public string CodigoAlfanumerico { get; set; }
    public int? DelegadoId { get; set; }
    public ICollection<EquipoDelJugadorDTO> Equipos { get; set; } = new List<EquipoDelJugadorDTO>();
    [Required]
    public string FotoCarnet { get; set; } = string.Empty;
    [Required]
    public string FotoDNIFrente { get; set; } = string.Empty;
    [Required]
    public string FotoDNIDorso { get; set; } = string.Empty;
}