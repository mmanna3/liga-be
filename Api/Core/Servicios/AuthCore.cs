using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Api.Core.Servicios;

public class AuthCore : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthCore(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponseDTO> Login(LoginDTO dto)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == dto.Usuario);

        if (usuario == null)
        {
            return new LoginResponseDTO
            {
                Exito = false,
                Error = "Usuario o contraseña incorrectos"
            };
        }

        // Verificar la contraseña usando BCrypt
        if (!VerificarPasswordHash(dto.Password, usuario.Password))
        {
            return new LoginResponseDTO
            {
                Exito = false,
                Error = "Usuario o contraseña incorrectos"
            };
        }

        // Generar el token JWT
        var token = GenerarToken(usuario);

        return new LoginResponseDTO
        {
            Exito = true,
            Token = token
        };
    }

    private string GenerarToken(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, usuario.NombreUsuario),
            new (ClaimTypes.NameIdentifier, usuario.Id.ToString())
        };

        // Obtener la clave secreta de la configuración o usar una clave por defecto
        string claveSecreta = _configuration.GetSection("AppSettings:Token").Value ?? "clave_secreta_por_defecto_para_desarrollo_con_longitud_suficiente_para_hmac_sha512";
        
        // Asegurar que la clave tenga al menos 64 bytes (512 bits) para HMAC-SHA512
        if (Encoding.UTF8.GetByteCount(claveSecreta) < 64)
        {
            // Extender la clave hasta alcanzar al menos 64 bytes
            claveSecreta = claveSecreta.PadRight(64, '_');
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveSecreta));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(1),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    private bool VerificarPasswordHash(string password, string passwordHash)
    {
        // Verificar la contraseña usando BCrypt
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            // Si hay un error (por ejemplo, el hash no está en formato BCrypt),
            // devolver false para indicar que la contraseña es incorrecta
            return false;
        }
    }
    
    // Método auxiliar para generar hash de contraseñas (útil para crear usuarios)
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }
} 