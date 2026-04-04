namespace Api.Core.DTOs;

/// <summary>
/// Request para cambiar solo la visibilidad en la app (torneo o fase), sin tocar el resto del recurso.
/// </summary>
public class CambiarVisibilidadEnAppDTO
{
    public bool EsVisibleEnApp { get; set; }
}
