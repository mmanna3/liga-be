using System.Diagnostics;

namespace Api._Config.Diagnostico;

/// <summary>
/// Logger de diagnóstico de performance. Escribe con el nombre de logger
/// <c>Perf</c>, que en <c>nlog.config</c> va a su propio archivo
/// (<c>logs/nlog-perf-&lt;fecha&gt;.log</c>).
///
/// Todas las líneas incluyen <c>trace=&lt;id&gt;</c>: es el mismo identificador
/// para todas las líneas de un mismo request (REQ START, SQL…, REQ END), así se
/// puede seguir una request de punta a punta.
/// </summary>
public static class PerfLog
{
    public static readonly NLog.Logger Log = NLog.LogManager.GetLogger("Perf");

    /// <summary>Id de correlación del request en curso (W3C trace-id de ASP.NET).</summary>
    public static string TraceId => Activity.Current?.TraceId.ToString() ?? "-";
}
