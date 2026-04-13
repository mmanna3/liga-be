namespace Api.Core.DTOs.AppCarnetDigital;

public class ClubesDTO
{
    public string Equipo { get; set; } = string.Empty;
    public string Escudo { get; set; } = string.Empty;
    public string Localidad { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string EsTechado { get; set; } = string.Empty;

    /// <summary>
    /// Texto del tipo de cancha (mismo nombre que <c>CanchaTipoEnum</c>, ej. Cubierta, Descubierta).
    /// </summary>
    public string TipoCancha { get; set; } = string.Empty;
}
