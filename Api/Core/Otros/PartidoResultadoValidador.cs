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
}
