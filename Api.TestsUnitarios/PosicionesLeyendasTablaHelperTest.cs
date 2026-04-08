using Api.Core.Entidades;
using Api.Core.Logica;

namespace Api.TestsUnitarios;

public class PosicionesLeyendasTablaHelperTest
{
    [Fact]
    public void ConcatenarTextos_ListaVacia_DevuelveNull()
    {
        Assert.Null(PosicionesLeyendasTablaHelper.ConcatenarTextos(Array.Empty<LeyendaTablaPosiciones>()));
    }

    [Fact]
    public void ConcatenarTextos_SoloCadenasVaciasOBlancas_DevuelveNull()
    {
        var leyendas = new[]
        {
            new LeyendaTablaPosiciones { Id = 1, Leyenda = "", ZonaId = 1 },
            new LeyendaTablaPosiciones { Id = 2, Leyenda = "  \t ", ZonaId = 1 }
        };
        Assert.Null(PosicionesLeyendasTablaHelper.ConcatenarTextos(leyendas));
    }

    [Fact]
    public void ConcatenarTextos_OmiteVaciasYUneConSaltoDeLinea()
    {
        var leyendas = new[]
        {
            new LeyendaTablaPosiciones { Id = 10, Leyenda = "", ZonaId = 1 },
            new LeyendaTablaPosiciones { Id = 5, Leyenda = "Primera", ZonaId = 1 },
            new LeyendaTablaPosiciones { Id = 7, Leyenda = "   ", ZonaId = 1 },
            new LeyendaTablaPosiciones { Id = 2, Leyenda = "Segunda", ZonaId = 1 }
        };
        var r = PosicionesLeyendasTablaHelper.ConcatenarTextos(leyendas);
        Assert.Equal("Segunda\nPrimera", r);
    }

    [Fact]
    public void ConcatenarTextos_OrdenEstablePorId()
    {
        var leyendas = new[]
        {
            new LeyendaTablaPosiciones { Id = 3, Leyenda = "C", ZonaId = 1 },
            new LeyendaTablaPosiciones { Id = 1, Leyenda = "A", ZonaId = 1 },
            new LeyendaTablaPosiciones { Id = 2, Leyenda = "B", ZonaId = 1 }
        };
        Assert.Equal("A\nB\nC", PosicionesLeyendasTablaHelper.ConcatenarTextos(leyendas));
    }

    [Fact]
    public void ConcatenarTextos_UnSoloTexto_DevuelveEseTexto()
    {
        var leyendas = new[]
        {
            new LeyendaTablaPosiciones { Id = 1, Leyenda = "Sola", ZonaId = 1 }
        };
        Assert.Equal("Sola", PosicionesLeyendasTablaHelper.ConcatenarTextos(leyendas));
    }
}
