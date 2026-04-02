using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class CargarResultadosDTO : DTO
{
    [Required]
    public int JornadaId { get; set; }
    public ICollection<PartidoDTO> Partidos { get; set; } = new List<PartidoDTO>();

    [Required]
    public bool ResultadosVerificados { get; set; } = false;
}