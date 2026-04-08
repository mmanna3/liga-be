using Api.Core.Enums;

namespace Api.Core.DTOs.AppCarnetDigital;

public class InformacionInicialAgrupadorDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    /// <summary>Mismo criterio que TorneoAgrupadorDTO: nombre del color predefinido (ej. Negro, Azul).</summary>
    public string Color { get; set; } = string.Empty;
    public List<InformacionInicialTorneoDTO> Torneos { get; set; } = [];
}

public class InformacionInicialTorneoDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public List<InformacionInicialFaseDTO> Fases { get; set; } = [];
}

public class InformacionInicialFaseDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string TipoDeFase { get; set; } = string.Empty;
    public List<InformacionInicialZonaDTO> Zonas { get; set; } = [];
}

public class InformacionInicialZonaDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
