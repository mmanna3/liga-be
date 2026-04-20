namespace Api.Core.Otros;

/// <summary>
/// Texto del bando interzonal en la app carnet (fixture, jornadas con resultados, eliminación directa).
/// </summary>
public static class InterzonalAppEtiqueta
{
    /// <summary>
    /// Número 1 (o menor): solo la palabra reservada; mayor a 1: palabra + espacio + número.
    /// </summary>
    public static string Equipo(int numero) =>
        numero <= 1 ? "INTERZONAL" : $"INTERZONAL {numero}";
}
