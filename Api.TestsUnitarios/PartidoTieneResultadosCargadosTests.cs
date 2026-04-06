using Api.Core.Entidades;
using Xunit;
using static Api.Core.Logica.PosicionesTodosContraTodosLogica;

namespace Api.TestsUnitarios;

/// <summary>
/// Solo se acumulan estadísticas/puntos cuando <see cref="PartidoTieneResultadosCargados"/> es true
/// (ambos resultados del partido tienen valor; evita contar fechas sin cargar resultados).
/// </summary>
public class PartidoTieneResultadosCargadosTests
{
    private static Partido P(string local, string visitante) =>
        new()
        {
            Id = 1,
            CategoriaId = 1,
            JornadaId = 1,
            Jornada = null!,
            Categoria = null!,
            ResultadoLocal = local,
            ResultadoVisitante = visitante
        };

    [Fact]
    public void AmbosVacios_NoTieneResultadosCargados()
    {
        Assert.False(PartidoTieneResultadosCargados(P("", "")));
    }

    [Fact]
    public void SoloLocalCargado_NoTieneResultadosCargados()
    {
        Assert.False(PartidoTieneResultadosCargados(P("1", "")));
    }

    [Fact]
    public void SoloVisitanteCargado_NoTieneResultadosCargados()
    {
        Assert.False(PartidoTieneResultadosCargados(P("", "2")));
    }

    [Fact]
    public void AmbosConValor_TieneResultadosCargados_InclusoCeros()
    {
        Assert.True(PartidoTieneResultadosCargados(P("0", "0")));
    }

    [Fact]
    public void AmbosConValor_TieneResultadosCargados_InclusoS_Suspendido()
    {
        Assert.True(PartidoTieneResultadosCargados(P("S", "S")));
    }

    [Fact]
    public void EspaciosEnBlanco_CuentanComoVacio()
    {
        Assert.False(PartidoTieneResultadosCargados(P("1", "   ")));
        Assert.False(PartidoTieneResultadosCargados(P("   ", "1")));
    }
}
