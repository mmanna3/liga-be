using System.Globalization;
using System.Text.RegularExpressions;

namespace Api.Core.Otros;

/// <summary>
/// Valida resultados de partido según el mismo criterio que el CHECK en BD y las reglas de negocio de la API.
/// </summary>
public static class PartidoResultadoValidador
{
    // Equivalente a validar el string completo con: (^[0-9]*$)|(NP)|(S)|(P)|(GP)|(PP), sin permitir vacío.
    private static readonly Regex PatronResultado = new(
        @"^(?:[0-9]+|NP|GP|PP|S|P)$",
        RegexOptions.Compiled);

    private static readonly Regex PatronSoloDigitos = new(@"^[0-9]+$", RegexOptions.Compiled);

    public static void ValidarResultado(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ExcepcionControlada("Los resultados no pueden estar vacíos.");

        if (!PatronResultado.IsMatch(valor.Trim()))
            throw new ExcepcionControlada(
                "El resultado no es válido. Se permiten números o los valores NP, S, P, GP o PP.");
    }

    public static void ValidarParResultados(string resultadoLocal, string resultadoVisitante)
    {
        ValidarResultado(resultadoLocal);
        ValidarResultado(resultadoVisitante);

        var local = resultadoLocal.Trim();
        var visitante = resultadoVisitante.Trim();

        if (local is "S" or "P" || visitante is "S" or "P")
        {
            if (local != visitante)
                throw new ExcepcionControlada(
                    "Si el resultado local o visitante es S o P, el otro debe ser el mismo.");
        }
    }

    /// <summary>Null o vacío es válido; si hay texto, debe ser solo dígitos.</summary>
    public static void ValidarPenalesOpcional(string? valor)
    {
        if (valor == null || string.IsNullOrWhiteSpace(valor))
            return;

        if (!PatronSoloDigitos.IsMatch(valor.Trim()))
            throw new ExcepcionControlada("Los penales solo pueden contener números.");
    }

    /// <summary>
    /// En zona de eliminación directa, si el resultado es empate numérico, los penales son obligatorios,
    /// enteros distintos y mayores que cero. En el resto de los casos aplica <see cref="ValidarPenalesOpcional"/>.
    /// </summary>
    public static void ValidarPenalesSegunZonaYResultado(
        bool zonaEsEliminacionDirecta,
        string resultadoLocal,
        string resultadoVisitante,
        string? penalesLocal,
        string? penalesVisitante)
    {
        if (zonaEsEliminacionDirecta && EsEmpateSoloDigitosIgual(resultadoLocal, resultadoVisitante))
        {
            ValidarPenalesObligatoriosEmpateEliminacionDirecta(penalesLocal, penalesVisitante);
            return;
        }

        ValidarPenalesOpcional(penalesLocal);
        ValidarPenalesOpcional(penalesVisitante);
    }

    private static bool EsEmpateSoloDigitosIgual(string resultadoLocal, string resultadoVisitante)
    {
        var a = resultadoLocal.Trim();
        var b = resultadoVisitante.Trim();
        if (!PatronSoloDigitos.IsMatch(a) || !PatronSoloDigitos.IsMatch(b))
            return false;

        if (!int.TryParse(a, NumberStyles.Integer, CultureInfo.InvariantCulture, out var va) ||
            !int.TryParse(b, NumberStyles.Integer, CultureInfo.InvariantCulture, out var vb))
            return false;

        return va == vb;
    }

    private static void ValidarPenalesObligatoriosEmpateEliminacionDirecta(string? penalesLocal, string? penalesVisitante)
    {
        if (string.IsNullOrWhiteSpace(penalesLocal) || string.IsNullOrWhiteSpace(penalesVisitante))
            throw new ExcepcionControlada(
                "En eliminación directa, si el resultado es empate numérico, debe cargar los penales de ambos equipos.");

        var pl = penalesLocal.Trim();
        var pv = penalesVisitante.Trim();

        if (!PatronSoloDigitos.IsMatch(pl) || !PatronSoloDigitos.IsMatch(pv))
            throw new ExcepcionControlada("Los penales solo pueden contener números.");

        if (!int.TryParse(pl, NumberStyles.Integer, CultureInfo.InvariantCulture, out var nL) ||
            !int.TryParse(pv, NumberStyles.Integer, CultureInfo.InvariantCulture, out var nV))
            throw new ExcepcionControlada("Los penales solo pueden contener números.");

        if (nL <= 0 || nV <= 0)
            throw new ExcepcionControlada(
                "En eliminación directa, si el resultado es empate numérico, los penales deben ser mayores que cero.");

        if (nL == nV)
            throw new ExcepcionControlada(
                "En eliminación directa, si el resultado es empate numérico, los penales deben ser distintos.");
    }
}
