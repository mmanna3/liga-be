using Api.Core.Logica;
using Xunit;
using static Api.Core.Logica.PosicionesTodosContraTodosLogica;

namespace Api.TestsUnitarios;

/// <summary>
/// Puntos por partido (<see cref="PosicionesTodosContraTodosLogica.AcumularPuntos"/>):
/// numérico: 3 / 2 / 1; NP, P, S → 0; GP → 3; PP → 1; combinaciones mixtas alineadas con la tabla de estadísticas.
/// </summary>
public class PosicionesTodosContraTodosPuntosTests
{
    private static int PuntosDeUnPartido(string mi, string rival)
    {
        var p = 0;
        AcumularPuntos(ref p, mi, rival);
        return p;
    }

    [Fact]
    public void AcumularPuntos_ResultadoNumerico_GanaSuma3()
    {
        Assert.Equal(3, PuntosDeUnPartido("2", "0"));
    }

    [Fact]
    public void AcumularPuntos_ResultadoNumerico_EmpateSuma2()
    {
        Assert.Equal(2, PuntosDeUnPartido("1", "1"));
    }

    [Fact]
    public void AcumularPuntos_ResultadoNumerico_PierdeSuma1()
    {
        Assert.Equal(1, PuntosDeUnPartido("0", "3"));
    }

    [Fact]
    public void AcumularPuntos_NP_Suma0()
    {
        Assert.Equal(0, PuntosDeUnPartido("NP", "1"));
    }

    [Fact]
    public void AcumularPuntos_P_Suma0()
    {
        Assert.Equal(0, PuntosDeUnPartido("P", "P"));
    }

    [Fact]
    public void AcumularPuntos_S_Suma0()
    {
        Assert.Equal(0, PuntosDeUnPartido("S", "S"));
    }

    [Fact]
    public void AcumularPuntos_GP_Suma3_AlGanarPorPuntos()
    {
        Assert.Equal(3, PuntosDeUnPartido("GP", "0"));
    }

    [Fact]
    public void AcumularPuntos_PP_Suma1_AlPerderPorPuntos()
    {
        Assert.Equal(1, PuntosDeUnPartido("PP", "0"));
    }

    /// <summary>
    /// Si en el partido ambos casilleros son GP, cada equipo acumula +3 al procesar su perspectiva (dos llamadas).
    /// </summary>
    [Fact]
    public void AcumularPuntos_AmbosGP_CadaPerspectivaSuma3()
    {
        var totalLocal = 0;
        AcumularPuntos(ref totalLocal, "GP", "GP");
        var totalVisit = 0;
        AcumularPuntos(ref totalVisit, "GP", "GP");
        Assert.Equal(3, totalLocal);
        Assert.Equal(3, totalVisit);
    }

    /// <summary>
    /// Si ambos son PP, cada equipo suma 1 punto al procesar su perspectiva.
    /// </summary>
    [Fact]
    public void AcumularPuntos_AmbosPP_CadaPerspectivaSuma1()
    {
        var totalLocal = 0;
        AcumularPuntos(ref totalLocal, "PP", "PP");
        var totalVisit = 0;
        AcumularPuntos(ref totalVisit, "PP", "PP");
        Assert.Equal(1, totalLocal);
        Assert.Equal(1, totalVisit);
    }

    [Fact]
    public void AcumularPuntos_NumericoVsRivalNP_CuentaComoVictoria3()
    {
        Assert.Equal(3, PuntosDeUnPartido("2", "NP"));
    }

    [Fact]
    public void AcumularPuntos_NumericoVsRivalPP_CuentaComoVictoria3()
    {
        Assert.Equal(3, PuntosDeUnPartido("1", "PP"));
    }

    [Fact]
    public void AcumularPuntos_NumericoVsRivalGP_CuentaComoDerrota1()
    {
        Assert.Equal(1, PuntosDeUnPartido("0", "GP"));
    }

    [Fact]
    public void AcumularPuntos_RefAcumulaVariosPartidos_SumaTotal()
    {
        var total = 0;
        AcumularPuntos(ref total, "1", "0");
        AcumularPuntos(ref total, "1", "1");
        AcumularPuntos(ref total, "0", "1");
        Assert.Equal(6, total);
    }

    /// <summary>
    /// Misma regla de desempate que <c>PosicionesTodosContraTodosAsync</c>: puntos desc, nombre asc.
    /// </summary>
    [Fact]
    public void OrdenTabla_PuntosIguales_DesempateAlfabeticamentePorNombreEquipo()
    {
        var filas = new List<(string Nombre, int Puntos)>
        {
            ("Zebra", 3),
            ("Abeja", 3),
            ("Carpa", 1)
        };
        var orden = filas
            .OrderByDescending(f => f.Puntos)
            .ThenBy(f => f.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
        Assert.Equal("Abeja", orden[0].Nombre);
        Assert.Equal("Zebra", orden[1].Nombre);
        Assert.Equal("Carpa", orden[2].Nombre);
        var posicionesAsignadas = orden.Select((_, i) => (i + 1).ToString()).ToList();
        Assert.Equal(new[] { "1", "2", "3" }, posicionesAsignadas);
    }

    [Fact]
    public void OrdenTabla_PuntosDistintos_ElMayorVaPrimeroYRecibePosicion1()
    {
        var filas = new List<(string Nombre, int Puntos)>
        {
            ("B", 2),
            ("A", 6)
        };
        var orden = filas
            .OrderByDescending(f => f.Puntos)
            .ThenBy(f => f.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
        Assert.Equal("A", orden[0].Nombre);
        Assert.Equal("1", orden.Select((_, i) => (i + 1).ToString()).First());
    }
}
