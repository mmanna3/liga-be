using Api.Core.Enums;
using Api.Core.Logica;
using Xunit;
using static Api.Core.Logica.EliminacionDirectaLogica;

namespace Api.TestsUnitarios;

public class EliminacionDirectaLogicaTests
{
    private static EntradaDecisionEliminacionDirecta Normal(
        string rl, string rv, string? pl, string? pv, int localId, int visitanteId) =>
        new(
            TipoJornadaEliminacion.Normal,
            rl,
            rv,
            pl,
            pv,
            localId,
            visitanteId,
            null,
            null,
            null);

    private static EntradaDecisionEliminacionDirecta Libre(
        string rl, string rv, string? pl, string? pv, int equipoLocalId) =>
        new(
            TipoJornadaEliminacion.Libre,
            rl,
            rv,
            pl,
            pv,
            null,
            null,
            equipoLocalId,
            null,
            null);

    private static EntradaDecisionEliminacionDirecta Interzonal(
        string rl, string rv, string? pl, string? pv, int equipoId, LocalVisitanteEnum lado) =>
        new(
            TipoJornadaEliminacion.Interzonal,
            rl,
            rv,
            pl,
            pv,
            null,
            null,
            null,
            equipoId,
            lado);

    [Fact]
    public void Normal_ResultadoNumericoMayor_PasaLocal()
    {
        var e = Normal("3", "1", null, null, 10, 20);
        Assert.Equal(10, DecidirQueEquipoPasaALaSiguienteInstancia(e));
    }

    [Fact]
    public void Normal_ResultadoNumericoMayor_PasaVisitante()
    {
        var e = Normal("0", "2", null, null, 10, 20);
        Assert.Equal(20, DecidirQueEquipoPasaALaSiguienteInstancia(e));
    }

    [Fact]
    public void Normal_EmpateNumerico_PenalesMayor_PasaVisitante()
    {
        var e = Normal("2", "2", "4", "5", 10, 20);
        Assert.Equal(20, DecidirQueEquipoPasaALaSiguienteInstancia(e));
    }

    [Fact]
    public void Normal_NP_vs_Numero_PasaNumero()
    {
        var e = Normal("NP", "3", null, null, 10, 20);
        Assert.Equal(20, DecidirQueEquipoPasaALaSiguienteInstancia(e));
    }

    [Fact]
    public void Normal_GP_PasaEseEquipo()
    {
        var e = Normal("GP", "0", null, null, 10, 20);
        Assert.Equal(10, DecidirQueEquipoPasaALaSiguienteInstancia(e));
    }

    [Fact]
    public void Normal_PP_PasaElOtro()
    {
        var e = Normal("PP", "1", null, null, 10, 20);
        Assert.Equal(20, DecidirQueEquipoPasaALaSiguienteInstancia(e));
    }

    [Fact]
    public void Normal_AmbosSoloS_NP_P_SinGanador()
    {
        var e = Normal("S", "S", null, null, 10, 20);
        Assert.Null(DecidirQueEquipoPasaALaSiguienteInstancia(e));
    }

    [Fact]
    public void Normal_NP_y_NP_SinGanador()
    {
        var e = Normal("NP", "NP", null, null, 10, 20);
        Assert.Null(DecidirQueEquipoPasaALaSiguienteInstancia(e));
    }

    [Fact]
    public void Libre_SiempreDevuelveEquipoLocalId_InclusoSiEnResultadoGanaVisitante()
    {
        var e = Libre("0", "3", null, null, 99);
        Assert.Equal(99, DecidirQueEquipoPasaALaSiguienteInstancia(e));
    }

    [Fact]
    public void Libre_SiempreDevuelveMismoId_ConOtrosResultados()
    {
        var e = Libre("2", "1", null, null, 77);
        Assert.Equal(77, DecidirQueEquipoPasaALaSiguienteInstancia(e));
    }

    [Fact]
    public void Interzonal_EquipoEsLocalYGanaLocal_PasaEquipo()
    {
        var e = Interzonal("2", "1", null, null, 50, LocalVisitanteEnum.Local);
        Assert.Equal(50, DecidirQueEquipoPasaALaSiguienteInstancia(e));
    }

    [Fact]
    public void Interzonal_EquipoEsVisitanteYGanaVisitante_PasaEquipo()
    {
        var e = Interzonal("0", "3", null, null, 50, LocalVisitanteEnum.Visitante);
        Assert.Equal(50, DecidirQueEquipoPasaALaSiguienteInstancia(e));
    }

    [Fact]
    public void Interzonal_EquipoEsLocalPeroGanaVisitante_NoPasa()
    {
        var e = Interzonal("0", "2", null, null, 50, LocalVisitanteEnum.Local);
        Assert.Null(DecidirQueEquipoPasaALaSiguienteInstancia(e));
    }

    [Fact]
    public void SinEquipos_SiempreNull()
    {
        var e = new EntradaDecisionEliminacionDirecta(
            TipoJornadaEliminacion.SinEquipos,
            "1",
            "0",
            null,
            null,
            null,
            null,
            null,
            null,
            null);
        Assert.Null(DecidirQueEquipoPasaALaSiguienteInstancia(e));
    }

    [Fact]
    public void Normal_NumeroVsSimboloNoNumerico_GanaNumero()
    {
        var e = Normal("2", "S", null, null, 5, 6);
        Assert.Equal(5, DecidirQueEquipoPasaALaSiguienteInstancia(e));
    }
}
