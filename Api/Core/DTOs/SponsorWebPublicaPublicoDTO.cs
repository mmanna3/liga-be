namespace Api.Core.DTOs;

public class SponsorWebPublicaPublicoDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Ruta relativa HTTP del logo (p. ej. <c>/api/publico/sponsor-logo/1</c>).
    /// </summary>
    public string LogoUrl { get; set; } = string.Empty;
}
