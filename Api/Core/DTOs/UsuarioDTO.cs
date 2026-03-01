namespace Api.Core.DTOs;

public class UsuarioDTO : DTO
{
    public string NombreUsuario { get; set; } = string.Empty;
    public int? DelegadoId { get; set; }
}
