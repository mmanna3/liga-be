using Api.Core.DTOs;
using Api.Core.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDTO>> Login(LoginDTO dto)
    {
        var response = await _authService.Login(dto);
        
        if (!response.Exito)
        {
            return BadRequest(response);
        }
        
        return Ok(response);
    }
} 