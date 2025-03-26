namespace Api.Core.DTOs.AppCarnetDigital;

public class CarnetDigitalDTO : JugadorBaseDTO
{
    public string FotoCarnet { get; set; } = string.Empty;
    public string Equipo { get; set; } = string.Empty;
    public string Torneo { get; set; } = string.Empty;
    public int Estado { get; set; }
}
