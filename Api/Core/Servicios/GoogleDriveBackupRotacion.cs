namespace Api.Core.Servicios;

/// <summary>
/// Lógica pura para rotar backups en Drive: agrupa por día (un "par" = backup-bd + backup-imagenes de esa fecha)
/// y conserva capas GFS: 2 diarios + 1 lunes semanal + 1 día 1 mensual, sin solapar slots.
/// </summary>
public static class GoogleDriveBackupRotacion
{
    /// <summary>
    /// Devuelve el sufijo yyyy-MM-dd-HH-mm de un nombre de backup reconocido, o null.
    /// </summary>
    public static string? ExtraerTimestamp(string nombre)
    {
        string[] prefijos = ["backup-bd-", "backup-imagenes-"];
        foreach (var prefijo in prefijos)
            if (nombre.StartsWith(prefijo) && nombre.EndsWith(".zip"))
                return nombre[prefijo.Length..^4];
        return null;
    }

    /// <summary>
    /// Fecha calendario yyyy-MM-dd para agrupar el par bd + imágenes del mismo día aunque corran a distinta hora/minuto.
    /// </summary>
    public static string? ExtraerFechaDesdeNombreBackup(string nombre)
    {
        var ts = ExtraerTimestamp(nombre);
        if (string.IsNullOrEmpty(ts) || ts.Length < 10)
            return null;
        return ts[..10];
    }

    /// <summary>
    /// Fechas a conservar según retención por capas anclada a <paramref name="fechaReferencia"/>.
    /// Diario: hoy y ayer. Semanal/mensual: lunes y día 1 estrictamente anteriores a la ventana diaria.
    /// </summary>
    public static HashSet<string> CalcularFechasAConservar(DateOnly fechaReferencia)
    {
        var ayer = fechaReferencia.AddDays(-1);

        var keepers = new HashSet<string>(StringComparer.Ordinal)
        {
            FormatearFecha(fechaReferencia),
            FormatearFecha(ayer)
        };

        // Ventana diaria = [ayer, hoy]. Semanal/mensual deben ser estrictamente anteriores a ayer.
        keepers.Add(FormatearFecha(UltimoLunesAntesDe(ayer)));
        keepers.Add(FormatearFecha(UltimoDia1AntesDe(ayer)));

        return keepers;
    }

    public static RotacionBackupsDriveResult Calcular(
        IReadOnlyList<(string Id, string Name)> archivosEnCarpeta,
        DateOnly fechaReferencia)
    {
        var todosLosNombres = archivosEnCarpeta
            .Select(a => a.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        var conFecha = archivosEnCarpeta
            .Select(a => (a.Id, a.Name, Fecha: ExtraerFechaDesdeNombreBackup(a.Name)))
            .Where(x => x.Fecha != null)
            .Select(x => (x.Id, x.Name, Fecha: x.Fecha!))
            .ToList();

        var gruposPorFecha = conFecha
            .GroupBy(x => x.Fecha)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .ToList();

        var fechasAConservar = CalcularFechasAConservar(fechaReferencia);

        var idsABorrar = new List<string>();
        var nombresBorrados = new List<string>();

        foreach (var g in gruposPorFecha)
        {
            if (fechasAConservar.Contains(g.Key))
                continue;

            foreach (var item in g.OrderBy(x => x.Name, StringComparer.Ordinal))
            {
                idsABorrar.Add(item.Id);
                nombresBorrados.Add(item.Name);
            }
        }

        return new RotacionBackupsDriveResult
        {
            BackupsEnDrive = todosLosNombres,
            ParesDetectados = gruposPorFecha.Count,
            IdsABorrar = idsABorrar,
            ArchivosBorrados = nombresBorrados
        };
    }

    /// <summary>
    /// Último lunes con fecha estrictamente anterior a <paramref name="limiteExclusivo"/>
    /// (inicio de la ventana diaria).
    /// </summary>
    internal static DateOnly UltimoLunesAntesDe(DateOnly limiteExclusivo)
    {
        var candidato = limiteExclusivo.AddDays(-1);
        var diasDesdeLunes = ((int)candidato.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return candidato.AddDays(-diasDesdeLunes);
    }

    /// <summary>
    /// Último día 1 del mes con fecha estrictamente anterior a <paramref name="limiteExclusivo"/>.
    /// </summary>
    internal static DateOnly UltimoDia1AntesDe(DateOnly limiteExclusivo)
    {
        var candidato = limiteExclusivo.AddDays(-1);
        return new DateOnly(candidato.Year, candidato.Month, 1);
    }

    private static string FormatearFecha(DateOnly fecha) => fecha.ToString("yyyy-MM-dd");
}

public sealed class RotacionBackupsDriveResult
{
    public required IReadOnlyList<string> BackupsEnDrive { get; init; }
    public int ParesDetectados { get; init; }
    /// <summary>Ids a pasar a la API de Drive para borrar.</summary>
    public required IReadOnlyList<string> IdsABorrar { get; init; }
    public required IReadOnlyList<string> ArchivosBorrados { get; init; }
}
