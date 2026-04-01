using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

/// <summary>
/// DTO para crear un torneo con fase inicial y categorías opcionales.
/// </summary>
public class CrearTorneoDTO : TorneoDTO
{
    /// <summary>
    /// Fase inicial del torneo. Si es null, se crea la fase por defecto (todos contra todos, zona única).
    /// </summary>
    public FaseDTO? PrimeraFase { get; set; }

    /// <summary>
    /// Categorías a crear junto con el torneo. Puede ser null o vacío.
    /// </summary>
    public new List<TorneoCategoriaDTO>? Categorias { get; set; }
}
