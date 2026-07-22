using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Api.Core.Logica;

public class JugadorAuditLogger : IJugadorAuditLogger
{
    public const string LoggerName = "JugadorAudit";

    private readonly ILogger _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JugadorAuditLogger(ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor)
    {
        _logger = loggerFactory.CreateLogger(LoggerName);
        _httpContextAccessor = httpContextAccessor;
    }

    public void Log(
        string op,
        string? dni,
        int? jugadorId = null,
        int? equipoId = null,
        int? equipoOrigenId = null,
        int? equipoDestinoId = null,
        bool? unicoEquipo = null,
        int? clubId = null,
        string? resultado = null)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var usuario = user?.FindFirstValue(ClaimTypes.Name);
        var rol = user?.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(usuario))
            usuario = "anonimo";
        if (string.IsNullOrWhiteSpace(rol))
            rol = "-";

        var sb = new StringBuilder();
        sb.Append("JUGADOR_AUDIT");
        Append(sb, "op", op);
        Append(sb, "dni", NormalizarDni(dni));
        Append(sb, "usuario", usuario);
        Append(sb, "rol", rol);

        if (jugadorId.HasValue)
            Append(sb, "jugadorId", jugadorId.Value.ToString());
        if (equipoId.HasValue)
            Append(sb, "equipoId", equipoId.Value.ToString());
        if (equipoOrigenId.HasValue)
            Append(sb, "equipoOrigenId", equipoOrigenId.Value.ToString());
        if (equipoDestinoId.HasValue)
            Append(sb, "equipoDestinoId", equipoDestinoId.Value.ToString());
        if (unicoEquipo.HasValue)
            Append(sb, "unicoEquipo", unicoEquipo.Value ? "true" : "false");
        if (clubId.HasValue)
            Append(sb, "clubId", clubId.Value.ToString());
        if (!string.IsNullOrWhiteSpace(resultado))
            Append(sb, "resultado", resultado);

        _logger.LogInformation("{Message}", sb.ToString());
    }

    /// <summary>Construye la línea de auditoría sin escribir (útil para tests).</summary>
    public static string FormatearLinea(
        string op,
        string? dni,
        string usuario,
        string rol,
        int? jugadorId = null,
        int? equipoId = null,
        int? equipoOrigenId = null,
        int? equipoDestinoId = null,
        bool? unicoEquipo = null,
        int? clubId = null,
        string? resultado = null)
    {
        var sb = new StringBuilder();
        sb.Append("JUGADOR_AUDIT");
        Append(sb, "op", op);
        Append(sb, "dni", NormalizarDni(dni));
        Append(sb, "usuario", string.IsNullOrWhiteSpace(usuario) ? "anonimo" : usuario);
        Append(sb, "rol", string.IsNullOrWhiteSpace(rol) ? "-" : rol);

        if (jugadorId.HasValue)
            Append(sb, "jugadorId", jugadorId.Value.ToString());
        if (equipoId.HasValue)
            Append(sb, "equipoId", equipoId.Value.ToString());
        if (equipoOrigenId.HasValue)
            Append(sb, "equipoOrigenId", equipoOrigenId.Value.ToString());
        if (equipoDestinoId.HasValue)
            Append(sb, "equipoDestinoId", equipoDestinoId.Value.ToString());
        if (unicoEquipo.HasValue)
            Append(sb, "unicoEquipo", unicoEquipo.Value ? "true" : "false");
        if (clubId.HasValue)
            Append(sb, "clubId", clubId.Value.ToString());
        if (!string.IsNullOrWhiteSpace(resultado))
            Append(sb, "resultado", resultado);

        return sb.ToString();
    }

    public static string NormalizarDni(string? dni)
    {
        if (string.IsNullOrWhiteSpace(dni))
            return "-";
        var soloDigitos = new string(dni.Where(char.IsDigit).ToArray());
        return string.IsNullOrEmpty(soloDigitos) ? "-" : soloDigitos;
    }

    private static void Append(StringBuilder sb, string key, string value)
    {
        sb.Append(' ');
        sb.Append(key);
        sb.Append('=');
        sb.Append(value);
    }
}
