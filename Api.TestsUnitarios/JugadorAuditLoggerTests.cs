using Api.Core.Logica;

namespace Api.TestsUnitarios;

public class JugadorAuditLoggerTests
{
    [Fact]
    public void FormatearLinea_ContienePrefijoYCamposClave()
    {
        var linea = JugadorAuditLogger.FormatearLinea(
            op: "Desvincular",
            dni: "30.111.222",
            usuario: "foo",
            rol: "Delegado",
            jugadorId: 12,
            equipoId: 3,
            unicoEquipo: true,
            resultado: "Eliminado");

        Assert.StartsWith("JUGADOR_AUDIT ", linea);
        Assert.Contains("op=Desvincular", linea);
        Assert.Contains("dni=30111222", linea);
        Assert.Contains("usuario=foo", linea);
        Assert.Contains("rol=Delegado", linea);
        Assert.Contains("jugadorId=12", linea);
        Assert.Contains("equipoId=3", linea);
        Assert.Contains("unicoEquipo=true", linea);
        Assert.Contains("resultado=Eliminado", linea);
    }

    [Fact]
    public void NormalizarDni_DejaSoloDigitos()
    {
        Assert.Equal("30111222", JugadorAuditLogger.NormalizarDni("30.111.222"));
        Assert.Equal("30111222", JugadorAuditLogger.NormalizarDni("30111222"));
        Assert.Equal("-", JugadorAuditLogger.NormalizarDni(null));
        Assert.Equal("-", JugadorAuditLogger.NormalizarDni(""));
        Assert.Equal("-", JugadorAuditLogger.NormalizarDni("abc"));
    }

    [Fact]
    public void FormatearLinea_UsuarioVacio_UsaAnonimo()
    {
        var linea = JugadorAuditLogger.FormatearLinea(
            op: "ReFichajeRechazado",
            dni: "123",
            usuario: "",
            rol: "",
            jugadorId: 1,
            resultado: "Eliminado");

        Assert.Contains("usuario=anonimo", linea);
        Assert.Contains("rol=-", linea);
    }
}
