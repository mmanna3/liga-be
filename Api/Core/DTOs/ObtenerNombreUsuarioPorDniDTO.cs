namespace Api.Core.DTOs;

/// <summary>
/// Respuesta del endpoint obtener-nombre-usuario-por-dni para el flujo de fichaje "ya registrado".
/// </summary>
public class ObtenerNombreUsuarioPorDniDTO
{
    public bool HayError { get; set; }
    public string? MensajeError { get; set; }
    public string? NombreUsuario { get; set; }

    public static ObtenerNombreUsuarioPorDniDTO Exito(string nombreUsuario)
    {
        return new ObtenerNombreUsuarioPorDniDTO
        {
            HayError = false,
            NombreUsuario = nombreUsuario
        };
    }

    public static ObtenerNombreUsuarioPorDniDTO Error(string mensajeError)
    {
        return new ObtenerNombreUsuarioPorDniDTO
        {
            HayError = true,
            MensajeError = mensajeError
        };
    }
}
