namespace Api.Core.Logica;

public interface IJugadorAuditLogger
{
    void Log(
        string op,
        string? dni,
        int? jugadorId = null,
        int? equipoId = null,
        int? equipoOrigenId = null,
        int? equipoDestinoId = null,
        bool? unicoEquipo = null,
        int? clubId = null,
        string? resultado = null);
}
