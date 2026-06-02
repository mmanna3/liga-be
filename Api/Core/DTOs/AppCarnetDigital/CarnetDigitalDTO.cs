namespace Api.Core.DTOs.AppCarnetDigital;

public class CarnetDigitalDTO : JugadorBaseDTO
{
    public string FotoCarnet { get; set; } = string.Empty;
    public string Equipo { get; set; } = string.Empty;
    public string Torneo { get; set; } = string.Empty;
    /// <summary>Color del agrupador del torneo (Verde, Rojo, Azul, etc.).</summary>
    public string Color { get; set; } = string.Empty;
    public int Estado { get; set; }
    
    public bool EsDelegado { get; set; }

    public int TarjetasAmarillas { get; set; }

    public int TarjetasRojas { get; set; }
}
