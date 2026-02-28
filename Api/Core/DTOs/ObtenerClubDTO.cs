namespace Api.Core.DTOs;

/// <summary>
/// Respuesta del endpoint obtener-club. Incluye clubId para el fichaje de delegados.
/// </summary>
public class ObtenerClubDTO
{
    public bool HayError { get; set; }
    public string? MensajeError { get; set; }
    public int? ClubId { get; set; }
    public string? ClubNombre { get; set; }

    public static ObtenerClubDTO Exito(int clubId, string clubNombre)
    {
        return new ObtenerClubDTO
        {
            HayError = false,
            ClubId = clubId,
            ClubNombre = clubNombre
        };
    }

    public static ObtenerClubDTO Error(string mensajeError)
    {
        return new ObtenerClubDTO
        {
            HayError = true,
            MensajeError = mensajeError
        };
    }
}
