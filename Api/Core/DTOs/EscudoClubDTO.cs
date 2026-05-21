namespace Api.Core.DTOs;

public class EscudoClubDTO
{
    public int ClubId { get; set; }
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Ruta relativa HTTP del escudo (p. ej. <c>/Imagenes/Escudos/1.jpg</c> o el escudo por defecto).
    /// </summary>
    public string Escudo { get; set; } = string.Empty;
}
