using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class CambiarPasswordDTO
{
    [Required]
    public required string Usuario { get; set; }

    public string? PasswordActual { get; set; }
    
    [Required]
    public required string PasswordNuevo { get; set; }
} 