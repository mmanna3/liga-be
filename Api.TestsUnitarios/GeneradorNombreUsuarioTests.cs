using Api.Core.Logica;

namespace Api.TestsUnitarios;

public class GeneradorNombreUsuarioTests
{
    /// <summary>
    /// Juan Pérez: no existe ningún usuario -> genera "jperez"
    /// </summary>
    [Fact]
    public async Task JuanPerez_SinColision_GeneraJperez()
    {
        var resultado = await GeneradorNombreUsuario.ObtenerDisponible("Juan", "Pérez", _ => Task.FromResult(false));
        Assert.Equal("jperez", resultado);
    }

    /// <summary>
    /// José Pérez: "jperez" ya existe (ej. Juan Pérez) -> genera "joperez"
    /// </summary>
    [Fact]
    public async Task JosePerez_JperezExiste_GeneraJoperez()
    {
        var resultado = await GeneradorNombreUsuario.ObtenerDisponible("José", "Pérez", n => Task.FromResult(n == "jperez"));
        Assert.Equal("joperez", resultado);
    }

    /// <summary>
    /// José Pérez: "jperez" y "joperez" existen -> genera "josperez"
    /// </summary>
    [Fact]
    public async Task JosePerez_JperezYJoperezExisten_GeneraJosperez()
    {
        var existentes = new HashSet<string> { "jperez", "joperez" };
        var resultado = await GeneradorNombreUsuario.ObtenerDisponible("José", "Pérez", n => Task.FromResult(existentes.Contains(n)));
        Assert.Equal("josperez", resultado);
    }

    /// <summary>
    /// José Pérez: jperez, joperez, josperez existen -> genera "joseperez"
    /// </summary>
    [Fact]
    public async Task JosePerez_TresPrimerosExisten_GeneraJoseperez()
    {
        var existentes = new HashSet<string> { "jperez", "joperez", "josperez" };
        var resultado = await GeneradorNombreUsuario.ObtenerDisponible("José", "Pérez", n => Task.FromResult(existentes.Contains(n)));
        Assert.Equal("joseperez", resultado);
    }

    /// <summary>
    /// José Pérez: "joseperez" ya existe (dos con mismo nombre completo) -> genera "joseperez2"
    /// </summary>
    [Fact]
    public async Task JosePerez_JoseperezExiste_GeneraJoseperez2()
    {
        var existentes = new HashSet<string> { "jperez", "joperez", "josperez", "joseperez" };
        var resultado = await GeneradorNombreUsuario.ObtenerDisponible("José", "Pérez", n => Task.FromResult(existentes.Contains(n)));
        Assert.Equal("joseperez2", resultado);
    }

    /// <summary>
    /// José Pérez: joseperez y joseperez2 existen -> genera "joseperez3"
    /// </summary>
    [Fact]
    public async Task JosePerez_JoseperezYJoseperez2Existen_GeneraJoseperez3()
    {
        var existentes = new HashSet<string> { "jperez", "joperez", "josperez", "joseperez", "joseperez2" };
        var resultado = await GeneradorNombreUsuario.ObtenerDisponible("José", "Pérez", n => Task.FromResult(existentes.Contains(n)));
        Assert.Equal("joseperez3", resultado);
    }

    /// <summary>
    /// Nombre vacío -> ArgumentException
    /// </summary>
    [Fact]
    public async Task NombreVacio_ArrojaArgumentException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            GeneradorNombreUsuario.ObtenerDisponible("", "Pérez", _ => Task.FromResult(false)));
        Assert.Contains("nombre y el apellido", ex.Message);
    }

    /// <summary>
    /// Apellido vacío -> ArgumentException
    /// </summary>
    [Fact]
    public async Task ApellidoVacio_ArrojaArgumentException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            GeneradorNombreUsuario.ObtenerDisponible("Juan", "", _ => Task.FromResult(false)));
        Assert.Contains("nombre y el apellido", ex.Message);
    }

    /// <summary>
    /// Verifica que GenerarCandidatos produce la secuencia esperada para "jose" + "perez"
    /// </summary>
    [Fact]
    public void GenerarCandidatos_ProduceSecuenciaCorrecta()
    {
        var candidatos = GeneradorNombreUsuario.GenerarCandidatos("jose", "perez").Take(6).ToList();
        Assert.Equal(["jperez", "joperez", "josperez", "joseperez", "joseperez2", "joseperez3"], candidatos);
    }

    /// <summary>
    /// María García: sin colisión -> "mgarcia"
    /// </summary>
    [Fact]
    public async Task MariaGarcia_SinColision_GeneraMgarcia()
    {
        var resultado = await GeneradorNombreUsuario.ObtenerDisponible("María", "García", _ => Task.FromResult(false));
        Assert.Equal("mgarcia", resultado);
    }
}
