using Api.Core.Entidades;

namespace Api.Core.Logica;

/// <summary>
/// Texto mostrado en la app para leyendas de tabla de posiciones (todos contra todos).
/// </summary>
public static class PosicionesLeyendasTablaHelper
{
    /// <summary>
    /// Une textos con salto de línea; omite cadenas vacías o solo espacios. Orden estable por <see cref="Entidad.Id"/>.
    /// </summary>
    public static string? ConcatenarTextos(IEnumerable<LeyendaTablaPosiciones> leyendas)
    {
        var partes = leyendas
            .OrderBy(x => x.Id)
            .Select(l => l.Leyenda)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
        return partes.Count == 0 ? null : string.Join("\n", partes);
    }
}
