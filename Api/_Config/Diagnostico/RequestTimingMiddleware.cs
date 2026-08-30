using System.Diagnostics;

namespace Api._Config.Diagnostico;

/// <summary>
/// Registra, para cada request a <c>/api/...</c>, el momento de entrada
/// (REQ START) y el de salida (REQ END) con el tiempo total transcurrido y los
/// bytes enviados. Es el primer middleware del pipeline, así que el tiempo
/// medido incluye TODO: autorización, controller, EF, AutoMapper, serialización
/// JSON y compresión.
///
/// Comparar este total con las líneas <c>SQL</c> (mismo <c>trace</c>) dice si el
/// cuello de botella está en la base de datos o en el procesamiento en memoria.
/// </summary>
public class RequestTimingMiddleware
{
    private const int UmbralWarnMs = 3000;
    private readonly RequestDelegate _next;

    public RequestTimingMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (!path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var traceId = PerfLog.TraceId;
        var metodo = context.Request.Method;
        var query = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;
        var horaInicio = DateTime.Now;
        var sw = Stopwatch.StartNew();

        PerfLog.Log.Info("REQ START  trace={0} {1} {2}{3}  ({4:HH:mm:ss.fff})",
            traceId, metodo, path, query, horaInicio);

        var bodyOriginal = context.Response.Body;
        var contador = new StreamContador(bodyOriginal);
        context.Response.Body = contador;

        try
        {
            await _next(context);
        }
        finally
        {
            context.Response.Body = bodyOriginal;
            sw.Stop();
            var ms = sw.Elapsed.TotalMilliseconds;
            var nivel = ms >= UmbralWarnMs ? NLog.LogLevel.Warn : NLog.LogLevel.Info;
            PerfLog.Log.Log(nivel,
                "REQ END    trace={0} {1} {2}{3} -> HTTP {4}  |  TOTAL {5:N0} ms  |  {6:N0} bytes enviados  ({7:HH:mm:ss.fff} -> {8:HH:mm:ss.fff})",
                traceId, metodo, path, query, context.Response.StatusCode, ms, contador.BytesEscritos,
                horaInicio, DateTime.Now);
        }
    }
}
