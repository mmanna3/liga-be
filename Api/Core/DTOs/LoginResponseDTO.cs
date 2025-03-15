namespace Api.Core.DTOs;

public class LoginResponseDTO
{
    public bool Exito { get; set; }
    public string? Token { get; set; }
    public string? Error { get; set; }
} 