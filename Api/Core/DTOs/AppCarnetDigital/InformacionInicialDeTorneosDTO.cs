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
    /// <summary>Lista plana de fases para la app móvil (compatibilidad con clientes que no conocen grupos).</summary>
    public List<InformacionInicialFaseDTO> Fases { get; set; } = [];
    /// <summary>Árbol de fases y grupos para clientes que soportan la estructura anidada.</summary>
    public List<InformacionInicialElementoTorneoDTO> Elementos { get; set; } = [];
}

public class InformacionInicialElementoTorneoDTO
{
    public string Tipo { get; set; } = string.Empty;

    public int? Id { get; set; }
    public string? Nombre { get; set; }
    public string? TipoDeFase { get; set; }
    public List<InformacionInicialZonaDTO>? Zonas { get; set; }

    public int? GrupoId { get; set; }
    public string? NombreGrupo { get; set; }
    public List<InformacionInicialElementoTorneoDTO>? Elementos { get; set; }
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
    public int Orden { get; set; }
}
