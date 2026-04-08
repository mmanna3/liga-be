using System.Globalization;
using System.Text.RegularExpressions;
using Api.Core.Entidades;
using Api.Core.Enums;

namespace Api.Core.Logica;

/// <summary>
/// Cálculo de estadísticas de tabla de posiciones (todos contra todos, app carnet digital).
/// </summary>
public static class PosicionesTodosContraTodosLogica
{
    private static readonly Regex PatronSoloDigitos = new(@"^[0-9]+$", RegexOptions.Compiled);

    public static bool EsSoloDigitos(string s) => PatronSoloDigitos.IsMatch(s.Trim());

    /// <summary>
    /// El partido tiene resultados cargados cuando ambos casilleros (local y visitante) tienen valor distinto de vacío.
    /// Si no, no se contabiliza para estadísticas ni puntos.
    /// </summary>
    public static bool PartidoTieneResultadosCargados(Partido partido)
    {
        var rl = partido.ResultadoLocal?.Trim() ?? string.Empty;
        var rv = partido.ResultadoVisitante?.Trim() ?? string.Empty;
        return rl.Length > 0 && rv.Length > 0;
    }

    /// <summary>
    /// Acumula un partido desde la perspectiva del equipo (mi = nuestro casillero, rival = el otro).
    /// Debe invocarse solo si <see cref="PartidoTieneResultadosCargados"/> ya es true para ese <see cref="Partido"/>.
    /// <see cref="EstadisticasPosicionEquipo.PartidosJugados"/> cuenta solo partidos cargados donde el resultado propio no es S ni P.
    /// </summary>
    public static void AcumularPartido(ref EstadisticasPosicionEquipo ac, string mi, string rival)
    {
        mi = mi.Trim();
        rival = rival.Trim();

        if (mi is "S" or "P")
            return;

        ac.PartidosJugados++;

        if (mi == "NP")
        {
            ac.PartidosNoPresento++;
            return;
        }

        if (mi == "GP")
        {
            ac.PartidosGanados++;
            return;
        }

        if (mi == "PP")
        {
            ac.PartidosPerdidos++;
            return;
        }

        if (EsSoloDigitos(mi))
            ac.GolesAFavor += int.Parse(mi, NumberStyles.Integer, CultureInfo.InvariantCulture);
        if (EsSoloDigitos(rival))
            ac.GolesEnContra += int.Parse(rival, NumberStyles.Integer, CultureInfo.InvariantCulture);

        if (EsSoloDigitos(mi) && EsSoloDigitos(rival))
        {
            var a = int.Parse(mi, NumberStyles.Integer, CultureInfo.InvariantCulture);
            var b = int.Parse(rival, NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (a > b)
                ac.PartidosGanados++;
            else if (a < b)
                ac.PartidosPerdidos++;
            else
                ac.PartidosEmpatados++;
            return;
        }

        if (rival is "NP" or "PP")
        {
            ac.PartidosGanados++;
            return;
        }

        if (rival == "GP")
            ac.PartidosPerdidos++;
    }

    /// <summary>
    /// Suma puntos de un partido desde la perspectiva del equipo (mi / rival).
    /// Reglas: numérico gana 3, empate 2, pierde 1; NP/P/S → 0; GP → 3; PP → 1;
    /// si ambos son GP cada uno suma 3; si ambos PP cada uno suma 1.
    /// </summary>
    public static void AcumularPuntos(ref int puntos, string mi, string rival)
    {
        mi = mi.Trim();
        rival = rival.Trim();

        if (mi is "S" or "P")
            return;

        if (mi == "NP")
            return;

        if (mi == "GP")
        {
            puntos += 3;
            return;
        }

        if (mi == "PP")
        {
            puntos += 1;
            return;
        }

        if (EsSoloDigitos(mi) && EsSoloDigitos(rival))
        {
            var a = int.Parse(mi, NumberStyles.Integer, CultureInfo.InvariantCulture);
            var b = int.Parse(rival, NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (a > b)
                puntos += 3;
            else if (a == b)
                puntos += 2;
            else
                puntos += 1;
            return;
        }

        if (EsSoloDigitos(mi))
        {
            if (rival is "NP" or "PP")
                puntos += 3;
            else if (rival == "GP")
                puntos += 1;
            return;
        }

        if (rival is "NP" or "PP")
            puntos += 3;
        else if (rival == "GP")
            puntos += 1;
    }

    /// <summary>
    /// Obtiene el resultado del equipo y el del rival en un <see cref="Partido"/> según el tipo de jornada.
    /// </summary>
    public static bool IntentarObtenerMiResultadoYRival(Partido partido, Jornada jornada, int equipoId,
        out string mi, out string rival)
    {
        mi = string.Empty;
        rival = string.Empty;

        switch (jornada)
        {
            case JornadaNormal n:
                if (n.LocalEquipoId == equipoId)
                {
                    mi = partido.ResultadoLocal;
                    rival = partido.ResultadoVisitante;
                    return true;
                }

                if (n.VisitanteEquipoId == equipoId)
                {
                    mi = partido.ResultadoVisitante;
                    rival = partido.ResultadoLocal;
                    return true;
                }

                return false;
            case JornadaLibre l:
                if (l.EquipoLocalId != equipoId)
                    return false;
                mi = partido.ResultadoLocal;
                rival = partido.ResultadoVisitante;
                return true;
            case JornadaInterzonal i:
                if (i.EquipoId != equipoId)
                    return false;
                if (i.LocalOVisitanteId == (int)LocalVisitanteEnum.Local)
                {
                    mi = partido.ResultadoLocal;
                    rival = partido.ResultadoVisitante;
                }
                else
                {
                    mi = partido.ResultadoVisitante;
                    rival = partido.ResultadoLocal;
                }

                return true;
            default:
                return false;
        }
    }
}

/// <summary>Contadores enteros para una fila de posiciones (una categoría, un equipo).</summary>
public struct EstadisticasPosicionEquipo
{
    public int PartidosJugados;
    public int PartidosGanados;
    public int PartidosEmpatados;
    public int PartidosPerdidos;
    public int GolesAFavor;
    public int GolesEnContra;
    public int PartidosNoPresento;

    public static EstadisticasPosicionEquipo Sumar(EstadisticasPosicionEquipo a, EstadisticasPosicionEquipo b)
    {
        return new EstadisticasPosicionEquipo
        {
            PartidosJugados = a.PartidosJugados + b.PartidosJugados,
            PartidosGanados = a.PartidosGanados + b.PartidosGanados,
            PartidosEmpatados = a.PartidosEmpatados + b.PartidosEmpatados,
            PartidosPerdidos = a.PartidosPerdidos + b.PartidosPerdidos,
            GolesAFavor = a.GolesAFavor + b.GolesAFavor,
            GolesEnContra = a.GolesEnContra + b.GolesEnContra,
            PartidosNoPresento = a.PartidosNoPresento + b.PartidosNoPresento
        };
    }
}
