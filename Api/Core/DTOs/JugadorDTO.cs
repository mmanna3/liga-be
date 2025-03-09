using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class JugadorDTO : JugadorBaseDTO
{
    public int EquipoInicialId { get; set; }
    public string CodigoAlfanumerico { get; set; }
    public ICollection<EquipoDelJugadorDTO> Equipos { get; set; } = new List<EquipoDelJugadorDTO>();
    
    public string FotoCarnet { get; set; } = string.Empty;
    public string FotoDNIFrente { get; set; } = string.Empty;
    public string FotoDNIDorso { get; set; } = string.Empty;
}