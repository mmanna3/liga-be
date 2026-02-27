using System.Text;

namespace Api.Core.Logica;

/// <summary>
/// Genera nombres de usuario únicos a partir de nombre y apellido.
/// Estrategia: 1ª letra + apellido, si existe 2ª letra, etc. Si el nombre completo existe, agrega número.
/// </summary>
public static class GeneradorNombreUsuario
{
    /// <summary>
    /// Obtiene el primer nombre de usuario disponible. El delegado existeNombreUsuario debe devolver true si ya existe.
    /// </summary>
    public static async Task<string> ObtenerDisponible(string nombre, string apellido, Func<string, Task<bool>> existeNombreUsuario)
    {
        var nombreNormalizado = NormalizarTexto(nombre);
        var apellidoNormalizado = NormalizarTexto(apellido);

        if (string.IsNullOrEmpty(nombreNormalizado) || string.IsNullOrEmpty(apellidoNormalizado))
            throw new ArgumentException("El nombre y el apellido no pueden estar vacíos");

        foreach (var candidato in GenerarCandidatos(nombreNormalizado, apellidoNormalizado))
        {
            if (!await existeNombreUsuario(candidato))
                return candidato;
        }

        throw new InvalidOperationException("No se pudo generar un nombre de usuario único");
    }

    /// <summary>
    /// Genera candidatos en orden: j+perez, jo+perez, jos+perez, jose+perez, joseperez2, joseperez3...
    /// </summary>
    public static IEnumerable<string> GenerarCandidatos(string nombreNormalizado, string apellidoNormalizado)
    {
        for (var i = 1; i <= nombreNormalizado.Length; i++)
        {
            yield return nombreNormalizado[..i] + apellidoNormalizado;
        }
        var baseCompleto = nombreNormalizado + apellidoNormalizado;
        var n = 2;
        while (true)
        {
            yield return baseCompleto + n;
            n++;
        }
    }

    private static string NormalizarTexto(string texto)
    {
        texto = texto.Replace(" ", "");
        var normalizedString = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalizedString)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).ToLower();
    }
}
