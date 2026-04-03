using Api.Core.Otros;
using Xunit;

namespace Api.TestsUnitarios;

public class PartidoResultadoValidadorTests
{
    [Fact]
    public void ValidarPenalesSegunZonaYResultado_EliminacionDirecta_EmpateNumerico_PenalesValidos_NoLanza()
    {
        PartidoResultadoValidador.ValidarPenalesSegunZonaYResultado(true, "2", "2", "4", "5");
    }

    [Fact]
    public void ValidarPenalesSegunZonaYResultado_EliminacionDirecta_EmpateNumerico_FaltaPenal_Lanza()
    {
        Assert.Throws<ExcepcionControlada>(() =>
            PartidoResultadoValidador.ValidarPenalesSegunZonaYResultado(true, "1", "1", "3", null));
    }

    [Fact]
    public void ValidarPenalesSegunZonaYResultado_EliminacionDirecta_EmpateNumerico_PenalesIguales_Lanza()
    {
        Assert.Throws<ExcepcionControlada>(() =>
            PartidoResultadoValidador.ValidarPenalesSegunZonaYResultado(true, "0", "0", "3", "3"));
    }

    [Fact]
    public void ValidarPenalesSegunZonaYResultado_EliminacionDirecta_EmpateNumerico_PenalCero_Lanza()
    {
        Assert.Throws<ExcepcionControlada>(() =>
            PartidoResultadoValidador.ValidarPenalesSegunZonaYResultado(true, "1", "1", "0", "1"));
    }

    [Fact]
    public void ValidarPenalesSegunZonaYResultado_NoEliminacionDirecta_EmpateNumerico_PenalesNull_NoLanza()
    {
        PartidoResultadoValidador.ValidarPenalesSegunZonaYResultado(false, "2", "2", null, null);
    }

    [Fact]
    public void ValidarPenalesSegunZonaYResultado_EliminacionDirecta_ResultadoDistinto_PenalesNull_NoLanza()
    {
        PartidoResultadoValidador.ValidarPenalesSegunZonaYResultado(true, "2", "3", null, null);
    }

    [Fact]
    public void ValidarPenalesSegunZonaYResultado_EliminacionDirecta_NoTodoNumerico_PenalesNull_NoLanza()
    {
        PartidoResultadoValidador.ValidarPenalesSegunZonaYResultado(true, "NP", "NP", null, null);
    }
}
