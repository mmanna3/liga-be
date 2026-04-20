using Api.Core.Otros;
using Xunit;

namespace Api.TestsUnitarios;

public class InterzonalAppEtiquetaTests
{
    [Theory]
    [InlineData(0, "INTERZONAL")]
    [InlineData(1, "INTERZONAL")]
    [InlineData(2, "INTERZONAL 2")]
    [InlineData(3, "INTERZONAL 3")]
    [InlineData(10, "INTERZONAL 10")]
    public void Equipo_Numero_FormateaSegunRegla(int numero, string esperado)
    {
        Assert.Equal(esperado, InterzonalAppEtiqueta.Equipo(numero));
    }
}
