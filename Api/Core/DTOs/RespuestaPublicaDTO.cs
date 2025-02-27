namespace Api.Core.DTOs;

public abstract class RespuestaPublicaDTO<TSelf, T> where TSelf : RespuestaPublicaDTO<TSelf, T>
{
    public bool HayError { get; private set; }
    public string? MensajeError { get; private set; }
    public virtual T? Respuesta { get; private set; }

    protected RespuestaPublicaDTO(bool hayError, T? respuesta, string? mensajeError = null)
    {
        HayError = hayError;
        Respuesta = respuesta;
        MensajeError = mensajeError;
    }
    
    public static TSelf Exito(T respuesta)
    {
        return (TSelf)Activator.CreateInstance(typeof(TSelf), false, respuesta, null)!;
    }
    
    public static TSelf Error(string mensajeError)
    {
        return (TSelf)Activator.CreateInstance(typeof(TSelf), true, default(T), mensajeError)!;
    }
}


public class ObtenerNombreEquipoDTO : RespuestaPublicaDTO<ObtenerNombreEquipoDTO, string>
{
    public ObtenerNombreEquipoDTO(bool hayError, string? respuesta, string? mensajeError = null)
        : base(hayError, respuesta, mensajeError)
    {
    }
}