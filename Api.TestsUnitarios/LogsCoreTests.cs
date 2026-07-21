using Api.Core.Otros;
using Api.Core.Servicios;

namespace Api.TestsUnitarios;

public class LogsCoreTests : IDisposable
{
    private readonly string _tempDir;
    private readonly LogsCore _sut;

    public LogsCoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "liga-logs-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _sut = new LogsCore(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void Buscar_TextoCorto_LanzaExcepcionControlada()
    {
        var ex = Assert.Throws<ExcepcionControlada>(() => _sut.Buscar("12"));
        Assert.Contains("al menos 3", ex.Message);
    }

    [Fact]
    public void Buscar_PorDni_DevuelveLineasCoincidentes()
    {
        var hoy = DateTime.Today.ToString("yyyy-MM-dd");
        File.WriteAllText(
            Path.Combine(_tempDir, $"nlog-all-{hoy}.log"),
            $"{hoy} 10:00:00.0000|ERROR|Api|Jugador DNI 30111222 borrado{Environment.NewLine}" +
            $"{hoy} 10:01:00.0000|ERROR|Api|Otra cosa{Environment.NewLine}" +
            $"{hoy} 10:02:00.0000|ERROR|Api|Error con dni 30111222 otra vez{Environment.NewLine}");

        var resultado = _sut.Buscar("30111222", dias: 7);

        Assert.Equal(2, resultado.Resultados.Count);
        Assert.All(resultado.Resultados, h => Assert.Contains("30111222", h.Contenido));
        Assert.False(resultado.Truncado);
        Assert.NotNull(resultado.Resultados[0].Fecha);
    }

    [Fact]
    public void Buscar_CaseInsensitive()
    {
        var hoy = DateTime.Today.ToString("yyyy-MM-dd");
        File.WriteAllText(
            Path.Combine(_tempDir, $"nlog-api-{hoy}.log"),
            $"{hoy} 10:00:00.0000|ERROR|Api|Error DNI AbC123{Environment.NewLine}");

        var resultado = _sut.Buscar("abc123");

        Assert.Single(resultado.Resultados);
    }

    [Fact]
    public void Buscar_IgnoraArchivosFueraDeRangoDeDias()
    {
        var viejo = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
        var hoy = DateTime.Today.ToString("yyyy-MM-dd");
        File.WriteAllText(
            Path.Combine(_tempDir, $"nlog-all-{viejo}.log"),
            $"{viejo} 10:00:00.0000|ERROR|Api|DNI 99988877 viejo{Environment.NewLine}");
        File.WriteAllText(
            Path.Combine(_tempDir, $"nlog-all-{hoy}.log"),
            $"{hoy} 10:00:00.0000|ERROR|Api|DNI 99988877 reciente{Environment.NewLine}");

        var resultado = _sut.Buscar("99988877", dias: 7);

        Assert.Single(resultado.Resultados);
        Assert.Contains("reciente", resultado.Resultados[0].Contenido);
    }

    [Fact]
    public void Buscar_NoLeeArchivosQueNoSeanLogsPermitidos()
    {
        var hoy = DateTime.Today.ToString("yyyy-MM-dd");
        File.WriteAllText(Path.Combine(_tempDir, "secreto.txt"), "DNI 11223344 secreto");
        File.WriteAllText(
            Path.Combine(_tempDir, $"nlog-all-{hoy}.log"),
            $"{hoy} 10:00:00.0000|ERROR|Api|sin match{Environment.NewLine}");

        var resultado = _sut.Buscar("11223344");

        Assert.Empty(resultado.Resultados);
    }

    [Fact]
    public void Buscar_RespetaMaxResultadosYMarcaTruncado()
    {
        var hoy = DateTime.Today.ToString("yyyy-MM-dd");
        var lineas = string.Join(Environment.NewLine,
            Enumerable.Range(1, 10).Select(i => $"{hoy} 10:00:0{i % 10}.0000|ERROR|Api|hit DNI555 {i}"));
        File.WriteAllText(Path.Combine(_tempDir, $"nlog-all-{hoy}.log"), lineas + Environment.NewLine);

        var resultado = _sut.Buscar("DNI555", dias: 1, maxResultados: 3);

        Assert.Equal(3, resultado.Resultados.Count);
        Assert.True(resultado.Truncado);
    }

    [Fact]
    public void Buscar_CarpetaInexistente_DevuelveAdvertenciaSinExplotar()
    {
        var sut = new LogsCore(Path.Combine(_tempDir, "no-existe"));

        var resultado = sut.Buscar("cualquier");

        Assert.Empty(resultado.Resultados);
        Assert.Contains(resultado.Advertencias, a => a.Contains("no existe", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ListarArchivos_SoloDevuelveLogsPermitidos()
    {
        var hoy = DateTime.Today.ToString("yyyy-MM-dd");
        File.WriteAllText(Path.Combine(_tempDir, $"nlog-all-{hoy}.log"), "x");
        File.WriteAllText(Path.Combine(_tempDir, "stdout_123.log"), "y");
        File.WriteAllText(Path.Combine(_tempDir, "otro.txt"), "z");

        var archivos = _sut.ListarArchivos();

        Assert.Equal(2, archivos.Count);
        Assert.DoesNotContain(archivos, a => a.Nombre == "otro.txt");
    }
}
