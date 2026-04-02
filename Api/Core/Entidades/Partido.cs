namespace Api.Core.Entidades;

// VALORES PERMITIDOS EN LOS RESULTADOS:
// @"(^[0-9]*$)|(NP)|(S)|(P)|(GP)|(PP)"
// Lógica en AppDbContext
public class Partido : Entidad
{
    public virtual TorneoCategoria Categoria { get; set; } = null!;
    public required int CategoriaId { get; set; }
    public required int JornadaId { get; set; }
    public virtual Jornada Jornada { get; set; } = null!;
    public required string ResultadoLocal { get; set; } = string.Empty;
    public required string ResultadoVisitante { get; set; } = string.Empty;
}