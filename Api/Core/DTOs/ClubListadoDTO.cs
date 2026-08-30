namespace Api.Core.DTOs;

/// <summary>
/// DTO plano para el listado de clubes (home). No incluye el escudo en base64,
/// ni las colecciones de equipos y delegados: sólo lo que la tabla del front y
/// el combo de selección de club necesitan.
/// </summary>
public class ClubListadoDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Localidad { get; set; }
    public int CanchaTipoId { get; set; }
    public string CanchaTipo { get; set; } = string.Empty;
}
