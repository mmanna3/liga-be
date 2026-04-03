using System.Globalization;
using System.Text.RegularExpressions;
using Api.Core.Entidades;
using Api.Core.Enums;

namespace Api.Core.Logica;

/// <summary>
/// Decide qué equipo avanza a la siguiente instancia en eliminación directa según resultados y tipo de jornada.
/// </summary>
public static class EliminacionDirectaLogica
{
    private static readonly Regex PatronSoloDigitos = new(@"^[0-9]+$", RegexOptions.Compiled);

    private enum Lado
    {
        Local,
        Visitante,
        Ninguno
    }

    /// <summary>Entrada para <see cref="DecidirQueEquipoPasaALaSiguienteInstancia"/>.</summary>
    public record EntradaDecisionEliminacionDirecta(
        TipoJornadaEliminacion Tipo,
        string ResultadoLocal,
        string ResultadoVisitante,
        string? PenalesLocal,
        string? PenalesVisitante,
        int? LocalEquipoId,
        int? VisitanteEquipoId,
        int? JornadaLibreEquipoLocalId,
        int? InterzonalEquipoId,
        LocalVisitanteEnum? InterzonalEquipoEsLocalOVisitante);

    public enum TipoJornadaEliminacion
    {
        Normal,
        Libre,
        Interzonal,
        SinEquipos
    }

    public static EntradaDecisionEliminacionDirecta CrearEntrada(Jornada jornada, Partido partido)
    {
        return jornada switch
        {
            JornadaNormal n => new EntradaDecisionEliminacionDirecta(
                TipoJornadaEliminacion.Normal,
                partido.ResultadoLocal,
                partido.ResultadoVisitante,
                partido.PenalesLocal,
                partido.PenalesVisitante,
                n.LocalEquipoId,
                n.VisitanteEquipoId,
                null,
                null,
                null),
            JornadaLibre l => new EntradaDecisionEliminacionDirecta(
                TipoJornadaEliminacion.Libre,
                partido.ResultadoLocal,
                partido.ResultadoVisitante,
                partido.PenalesLocal,
                partido.PenalesVisitante,
                null,
                null,
                l.EquipoLocalId,
                null,
                null),
            JornadaInterzonal i => new EntradaDecisionEliminacionDirecta(
                TipoJornadaEliminacion.Interzonal,
                partido.ResultadoLocal,
                partido.ResultadoVisitante,
                partido.PenalesLocal,
                partido.PenalesVisitante,
                null,
                null,
                null,
                i.EquipoId,
                (LocalVisitanteEnum)i.LocalOVisitanteId),
            JornadaSinEquipos => new EntradaDecisionEliminacionDirecta(
                TipoJornadaEliminacion.SinEquipos,
                partido.ResultadoLocal,
                partido.ResultadoVisitante,
                partido.PenalesLocal,
                partido.PenalesVisitante,
                null,
                null,
                null,
                null,
                null),
            _ => throw new ArgumentOutOfRangeException(nameof(jornada), jornada, null)
        };
    }

    /// <summary>
    /// Devuelve el id del equipo que pasa de jornada, o null si no corresponde pasar a nadie.
    /// Jornada libre: siempre avanza el equipo local (<see cref="EntradaDecisionEliminacionDirecta.JornadaLibreEquipoLocalId"/>), sin mirar resultados.
    /// </summary>
    public static int? DecidirQueEquipoPasaALaSiguienteInstancia(EntradaDecisionEliminacionDirecta e)
    {
        if (e.Tipo == TipoJornadaEliminacion.SinEquipos)
            return null;

        if (e.Tipo == TipoJornadaEliminacion.Libre)
            return e.JornadaLibreEquipoLocalId;

        var lado = DecidirLadoGanador(e.ResultadoLocal, e.ResultadoVisitante, e.PenalesLocal, e.PenalesVisitante);

        if (lado == Lado.Ninguno)
            return null;

        return EquipoIdSegunTipoYLado(e, lado);
    }

    private static int? EquipoIdSegunTipoYLado(EntradaDecisionEliminacionDirecta e, Lado lado)
    {
        return e.Tipo switch
        {
            TipoJornadaEliminacion.Normal => lado == Lado.Local ? e.LocalEquipoId : e.VisitanteEquipoId,
            TipoJornadaEliminacion.Interzonal => EquipoInterzonalSiGana(e, lado),
            _ => null
        };
    }

    private static int? EquipoInterzonalSiGana(EntradaDecisionEliminacionDirecta e, Lado lado)
    {
        if (e.InterzonalEquipoId is not { } id || e.InterzonalEquipoEsLocalOVisitante is not { } lv)
            return null;

        if (lv == LocalVisitanteEnum.Local && lado == Lado.Local)
            return id;
        if (lv == LocalVisitanteEnum.Visitante && lado == Lado.Visitante)
            return id;
        return null;
    }

    private static Lado DecidirLadoGanador(string resultadoLocal, string resultadoVisitante, string? penalesLocal,
        string? penalesVisitante)
    {
        var l = resultadoLocal.Trim();
        var v = resultadoVisitante.Trim();

        if (l == "GP")
            return Lado.Local;
        if (v == "GP")
            return Lado.Visitante;

        if (l == "PP")
            return Lado.Visitante;
        if (v == "PP")
            return Lado.Local;

        if (EsSoloSimboloSinGanador(l) && EsSoloSimboloSinGanador(v))
            return Lado.Ninguno;

        var numL = EsSoloDigitos(l);
        var numV = EsSoloDigitos(v);

        if (numL && !numV)
            return Lado.Local;
        if (!numL && numV)
            return Lado.Visitante;

        if (numL && numV)
        {
            var gL = int.Parse(l, NumberStyles.Integer, CultureInfo.InvariantCulture);
            var gV = int.Parse(v, NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (gL != gV)
                return gL > gV ? Lado.Local : Lado.Visitante;

            if (string.IsNullOrWhiteSpace(penalesLocal) || string.IsNullOrWhiteSpace(penalesVisitante))
                return Lado.Ninguno;

            var pL = int.Parse(penalesLocal.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture);
            var pV = int.Parse(penalesVisitante.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (pL != pV)
                return pL > pV ? Lado.Local : Lado.Visitante;
            return Lado.Ninguno;
        }

        return Lado.Ninguno;
    }

    private static bool EsSoloSimboloSinGanador(string r) => r is "S" or "NP" or "P";

    private static bool EsSoloDigitos(string s) => PatronSoloDigitos.IsMatch(s.Trim());
}
