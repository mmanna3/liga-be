using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class LoginDTO
{
    [Required]
    public required string Usuario { get; set; }
    
    [Required]
    public required string Password { get; set; }
} 