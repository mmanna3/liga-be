using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class UsuarioAdminDTO : DTO
{
    [Required, MaxLength(14)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    public int RolId { get; set; }

    public string RolNombre { get; set; } = string.Empty;

    public bool BlanqueoPendiente { get; set; }
}
