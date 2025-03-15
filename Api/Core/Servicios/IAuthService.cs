using Api.Core.DTOs;

namespace Api.Core.Servicios;

public interface IAuthService
{
    Task<LoginResponseDTO> Login(LoginDTO dto);
} 