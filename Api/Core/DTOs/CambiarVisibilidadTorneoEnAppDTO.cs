namespace Api.Core.DTOs;

/// <summary>
/// Request para cambiar solo la visibilidad del torneo en la app (sin tocar nombre, fases, etc.).
/// </summary>
public class CambiarVisibilidadTorneoEnAppDTO
{
    public bool EsVisibleEnApp { get; set; }
}
