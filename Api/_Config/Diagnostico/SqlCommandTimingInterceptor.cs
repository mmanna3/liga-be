using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Api._Config.Diagnostico;

/// <summary>
/// Interceptor de EF Core que registra el tiempo de EJECUCIÓN en SQL Server de
/// cada comando (SELECT/INSERT/UPDATE/DELETE), con el mismo <c>trace</c> que el
/// request que lo originó.
///
/// OJO: <c>eventData.Duration</c> es sólo el tiempo hasta que SQL Server
/// devuelve el cursor. La lectura de filas y la materialización de entidades por
/// parte de EF (que con grafos grandes + tracking puede ser lo más caro) NO
/// entra acá: eso se ve como la diferencia entre el tiempo del repo
/// (líneas <c>CORE</c>) y la suma de estas líneas <c>SQL</c>.
/// </summary>
public sealed class SqlCommandTimingInterceptor : DbCommandInterceptor
{
    private const int UmbralWarnMs = 1000;
    private static readonly Regex EspaciosMultiples = new(@"\s+", RegexOptions.Compiled);

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        Registrar(command, eventData, "reader");
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData,
        DbDataReader result, CancellationToken cancellationToken = default)
    {
        Registrar(command, eventData, "reader");
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
    {
        Registrar(command, eventData, "scalar");
        return base.ScalarExecuted(command, eventData, result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData eventData,
        object? result, CancellationToken cancellationToken = default)
    {
        Registrar(command, eventData, "scalar");
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
    {
        Registrar(command, eventData, "nonquery");
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData,
        int result, CancellationToken cancellationToken = default)
    {
        Registrar(command, eventData, "nonquery");
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    private static void Registrar(DbCommand command, CommandExecutedEventData eventData, string tipo)
    {
        var ms = eventData.Duration.TotalMilliseconds;
        var traceId = PerfLog.TraceId;

        // Sin request asociado (arranque, migraciones, jobs) sólo se registra si
        // tardó algo relevante: evita el ruido de la creación del esquema.
        if (traceId == "-" && ms < 200)
            return;

        var nivel = ms >= UmbralWarnMs ? NLog.LogLevel.Warn : NLog.LogLevel.Info;
        PerfLog.Log.Log(nivel, "SQL   trace={0} {1,8:N0} ms  {2,-8}  {3}",
            traceId, ms, tipo, Resumir(command.CommandText));
    }

    private static string Resumir(string sql)
    {
        var limpio = EspaciosMultiples.Replace(sql, " ").Trim();
        return limpio.Length <= 400 ? limpio : limpio[..400] + " […]";
    }
}
