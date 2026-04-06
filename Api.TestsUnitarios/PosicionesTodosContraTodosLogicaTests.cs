using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Logica;
using Xunit;
using static Api.Core.Logica.PosicionesTodosContraTodosLogica;

namespace Api.TestsUnitarios;

/// <summary>
/// Reglas de negocio de <see cref="Api.Core.Logica.PosicionesTodosContraTodosLogica.AcumularPartido"/>:
/// - Partidos jugados: solo si el resultado propio (mi) no es S ni P (partido suspendido/postergado sin tabla).
/// - Partidos NP / GP / PP: contadores dedicados; GP implica ganado; PP implica perdido.
/// - Goles: solo cuando el valor es numérico (dígitos).
/// - Victoria numérica: ambos goles numéricos y mi &gt; rival.
/// - Empate: ambos numéricos e iguales.
/// - Derrota numérica: ambos numéricos y mi &lt; rival.
/// - Victoria por ausencia del rival: rival NP o PP (sin par numérico completo en la rama final).
/// - Derrota por GP del rival: rival == GP.
/// </summary>
public class PosicionesTodosContraTodosLogicaTests
{
    private static EstadisticasPosicionEquipo Acum(string mi, string rival)
    {
        var s = new EstadisticasPosicionEquipo();
        AcumularPartido(ref s, mi, rival);
        return s;
    }

    [Fact]
    public void AcumularPartido_PartidosJugados_SiMiEsS_NoCuentaPartidoNiRestoDeEstadisticas()
    {
        var s = Acum("S", "S");
        Assert.Equal(0, s.PartidosJugados);
        Assert.Equal(0, s.PartidosGanados);
        Assert.Equal(0, s.GolesAFavor);
    }

    [Fact]
    public void AcumularPartido_PartidosJugados_SiMiEsP_NoCuentaPartidoNiRestoDeEstadisticas()
    {
        var s = Acum("P", "P");
        Assert.Equal(0, s.PartidosJugados);
        Assert.Equal(0, s.PartidosGanados);
    }

    [Fact]
    public void AcumularPartido_PartidosJugados_CuentaCuandoMiNoEsSNiP_InclusoSiRivalEsS()
    {
        var s = Acum("3", "S");
        Assert.Equal(1, s.PartidosJugados);
    }

    [Fact]
    public void AcumularPartido_PartidosNoPresento_SoloCuandoMiEsNP_IncrementaNoPresentoYJugadosPeroNoGolesNiResultado()
    {
        var s = Acum("NP", "0");
        Assert.Equal(1, s.PartidosJugados);
        Assert.Equal(1, s.PartidosNoPresento);
        Assert.Equal(0, s.GolesAFavor);
        Assert.Equal(0, s.GolesEnContra);
        Assert.Equal(0, s.PartidosGanados);
        Assert.Equal(0, s.PartidosPerdidos);
        Assert.Equal(0, s.PartidosEmpatados);
    }

    [Fact]
    public void AcumularPartido_PartidosGanoPuntos_CuandoMiEsGP_CuentaGanoPuntosYGanados()
    {
        var s = Acum("GP", "0");
        Assert.Equal(1, s.PartidosJugados);
        Assert.Equal(1, s.PartidosGanoPuntos);
        Assert.Equal(1, s.PartidosGanados);
        Assert.Equal(0, s.GolesAFavor);
    }

    [Fact]
    public void AcumularPartido_PartidosPerdioPuntos_CuandoMiEsPP_CuentaPerdioPuntosYPerdidos()
    {
        var s = Acum("PP", "0");
        Assert.Equal(1, s.PartidosJugados);
        Assert.Equal(1, s.PartidosPerdioPuntos);
        Assert.Equal(1, s.PartidosPerdidos);
        Assert.Equal(0, s.GolesEnContra);
    }

    [Fact]
    public void AcumularPartido_GolesAFavor_SumaSoloDigitosDeMi()
    {
        var s = Acum("4", "1");
        Assert.Equal(4, s.GolesAFavor);
    }

    [Fact]
    public void AcumularPartido_GolesEnContra_SumaSoloDigitosDeRival()
    {
        var s = Acum("1", "5");
        Assert.Equal(5, s.GolesEnContra);
    }

    [Fact]
    public void AcumularPartido_GolesDiferencia_Implicita_GFmenosGC_enPartidoNumerico()
    {
        var s = Acum("3", "1");
        Assert.Equal(3, s.GolesAFavor);
        Assert.Equal(1, s.GolesEnContra);
        Assert.Equal(2, s.GolesAFavor - s.GolesEnContra);
    }

    [Fact]
    public void AcumularPartido_PartidosEmpatados_AmbosResultadosNumericosIguales()
    {
        var s = Acum("2", "2");
        Assert.Equal(1, s.PartidosJugados);
        Assert.Equal(1, s.PartidosEmpatados);
        Assert.Equal(0, s.PartidosGanados);
        Assert.Equal(0, s.PartidosPerdidos);
    }

    [Fact]
    public void AcumularPartido_PartidosGanados_ResultadoNumericoMayorQueRival()
    {
        var s = Acum("3", "1");
        Assert.Equal(1, s.PartidosGanados);
        Assert.Equal(0, s.PartidosPerdidos);
        Assert.Equal(0, s.PartidosEmpatados);
    }

    [Fact]
    public void AcumularPartido_PartidosPerdidos_ResultadoNumericoMenorQueRival()
    {
        var s = Acum("0", "2");
        Assert.Equal(1, s.PartidosPerdidos);
        Assert.Equal(0, s.PartidosGanados);
    }

    [Fact]
    public void AcumularPartido_PartidosGanados_RivalNP_SinParNumericoCompleto_GanaPorReglaDeRivalAusente()
    {
        var s = Acum("2", "NP");
        Assert.Equal(1, s.PartidosGanados);
        Assert.Equal(0, s.PartidosPerdidos);
        Assert.Equal(2, s.GolesAFavor);
        Assert.Equal(0, s.GolesEnContra);
    }

    [Fact]
    public void AcumularPartido_PartidosGanados_RivalPP_GanaPorReglaDeRivalPerdioPuntos()
    {
        var s = Acum("1", "PP");
        Assert.Equal(1, s.PartidosGanados);
    }

    [Fact]
    public void AcumularPartido_PartidosPerdidos_RivalGP_PierdePorGanarPuntosElRival()
    {
        var s = Acum("1", "GP");
        Assert.Equal(1, s.PartidosPerdidos);
        Assert.Equal(0, s.PartidosGanados);
    }

    [Fact]
    public void AcumularPartido_Trim_EnMiYRival_IgnoraEspacios()
    {
        var s = Acum(" 3 ", " 3 ");
        Assert.Equal(1, s.PartidosEmpatados);
    }

    [Fact]
    public void AcumularPartido_EsSoloDigitos_ParaGoles_UsaMismaReglaQueLaLogica()
    {
        Assert.True(EsSoloDigitos("10"));
        Assert.False(EsSoloDigitos("NP"));
    }

    [Fact]
    public void IntentarObtenerMiResultadoYRival_JornadaNormal_EquipoLocalVeResultadoLocalComoMi()
    {
        var j = new JornadaNormal
        {
            Id = 1,
            FechaId = 1,
            ResultadosVerificados = false,
            LocalEquipoId = 10,
            VisitanteEquipoId = 20,
            LocalEquipo = null!,
            VisitanteEquipo = null!,
            Partidos = []
        };
        var p = new Partido
        {
            Id = 1,
            CategoriaId = 1,
            JornadaId = 1,
            Jornada = j,
            Categoria = null!,
            ResultadoLocal = "2",
            ResultadoVisitante = "1"
        };

        var ok = IntentarObtenerMiResultadoYRival(p, j, 10, out var mi, out var rival);
        Assert.True(ok);
        Assert.Equal("2", mi);
        Assert.Equal("1", rival);
    }

    [Fact]
    public void IntentarObtenerMiResultadoYRival_JornadaNormal_EquipoVisitanteVeResultadoVisitanteComoMi()
    {
        var j = new JornadaNormal
        {
            Id = 1,
            FechaId = 1,
            ResultadosVerificados = false,
            LocalEquipoId = 10,
            VisitanteEquipoId = 20,
            LocalEquipo = null!,
            VisitanteEquipo = null!,
            Partidos = []
        };
        var p = new Partido
        {
            Id = 1,
            CategoriaId = 1,
            JornadaId = 1,
            Jornada = j,
            Categoria = null!,
            ResultadoLocal = "2",
            ResultadoVisitante = "5"
        };

        var ok = IntentarObtenerMiResultadoYRival(p, j, 20, out var mi, out var rival);
        Assert.True(ok);
        Assert.Equal("5", mi);
        Assert.Equal("2", rival);
    }

    [Fact]
    public void IntentarObtenerMiResultadoYRival_JornadaLibre_SoloEquipoLocalTienePerspectiva()
    {
        var j = new JornadaLibre
        {
            Id = 2,
            FechaId = 1,
            ResultadosVerificados = false,
            EquipoLocalId = 7,
            EquipoLocal = null!,
            Partidos = []
        };
        var p = new Partido
        {
            Id = 1,
            CategoriaId = 1,
            JornadaId = 1,
            Jornada = j,
            Categoria = null!,
            ResultadoLocal = "1",
            ResultadoVisitante = "NP"
        };

        Assert.True(IntentarObtenerMiResultadoYRival(p, j, 7, out _, out _));
        Assert.False(IntentarObtenerMiResultadoYRival(p, j, 99, out _, out _));
    }

    [Fact]
    public void IntentarObtenerMiResultadoYRival_JornadaInterzonal_EquipoDeLaZonaComoLocal()
    {
        var j = new JornadaInterzonal
        {
            Id = 3,
            FechaId = 1,
            ResultadosVerificados = false,
            EquipoId = 50,
            Equipo = null!,
            LocalOVisitanteId = (int)LocalVisitanteEnum.Local,
            LocalVisitante = null!,
            Partidos = []
        };
        var p = new Partido
        {
            Id = 1,
            CategoriaId = 1,
            JornadaId = 1,
            Jornada = j,
            Categoria = null!,
            ResultadoLocal = "1",
            ResultadoVisitante = "0"
        };

        Assert.True(IntentarObtenerMiResultadoYRival(p, j, 50, out var mi, out var rival));
        Assert.Equal("1", mi);
        Assert.Equal("0", rival);
    }

    [Fact]
    public void IntentarObtenerMiResultadoYRival_JornadaInterzonal_EquipoDeLaZonaComoVisitante()
    {
        var j = new JornadaInterzonal
        {
            Id = 4,
            FechaId = 1,
            ResultadosVerificados = false,
            EquipoId = 50,
            Equipo = null!,
            LocalOVisitanteId = (int)LocalVisitanteEnum.Visitante,
            LocalVisitante = null!,
            Partidos = []
        };
        var p = new Partido
        {
            Id = 1,
            CategoriaId = 1,
            JornadaId = 1,
            Jornada = j,
            Categoria = null!,
            ResultadoLocal = "1",
            ResultadoVisitante = "2"
        };

        Assert.True(IntentarObtenerMiResultadoYRival(p, j, 50, out var mi, out var rival));
        Assert.Equal("2", mi);
        Assert.Equal("1", rival);
    }
}
