using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Core.Servicios.Interfaces;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Api.Core.Servicios;

public class AuthCore : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IPermisoServicio _permisoServicio;

    public AuthCore(AppDbContext context, IConfiguration configuration, IPermisoServicio permisoServicio)
    {
        _context = context;
        _configuration = configuration;
        _permisoServicio = permisoServicio;
    }

    public async Task<LoginResponseDTO> Login(LoginDTO dto)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.AccesosModulo)
            .FirstOrDefaultAsync(u => u.NombreUsuario == dto.Usuario);

        if (usuario == null)
        {
            return new LoginResponseDTO
            {
                Exito = false,
                Error = "Usuario o contraseña incorrectos"
            };
        }

        if (usuario.Password == null)
        {
            return new LoginResponseDTO
            {
                Exito = false,
                Error = "El usuario debe cambiar la contraseña"
            };
        }

        if (!VerificarPasswordHash(dto.Password, usuario.Password))
        {
            return new LoginResponseDTO
            {
                Exito = false,
                Error = "Usuario o contraseña incorrectos"
            };
        }

        var permisos = MapearPermisos(usuario);
        var token = GenerarToken(usuario, permisos);

        return new LoginResponseDTO
        {
            Exito = true,
            Token = token,
            Permisos = permisos
        };
    }

    public async Task<LoginResponseDTO> CambiarPassword(CambiarPasswordDTO dto)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.AccesosModulo)
            .FirstOrDefaultAsync(u => u.NombreUsuario == dto.Usuario);

        if (usuario == null)
        {
            return new LoginResponseDTO
            {
                Exito = false,
                Error = "Usuario no encontrado"
            };
        }

        if (usuario.Password != null)
        {
            if (string.IsNullOrEmpty(dto.PasswordActual))
            {
                return new LoginResponseDTO
                {
                    Exito = false,
                    Error = "Debe ingresar la contraseña actual"
                };
            }

            if (!VerificarPasswordHash(dto.PasswordActual, usuario.Password))
            {
                return new LoginResponseDTO
                {
                    Exito = false,
                    Error = "La contraseña actual es incorrecta"
                };
            }
        }

        usuario.Password = HashPassword(dto.PasswordNuevo);
        await _context.SaveChangesAsync();

        var permisos = MapearPermisos(usuario);
        var token = GenerarToken(usuario, permisos);

        return new LoginResponseDTO
        {
            Exito = true,
            Token = token,
            Permisos = permisos
        };
    }

    public Task<IReadOnlyList<UsuarioAccesoModuloDTO>> ObtenerPermisosDelUsuarioAutenticado()
    {
        IReadOnlyList<UsuarioAccesoModuloDTO> permisos = _permisoServicio.EsSuperAdministrador()
            ? []
            : _permisoServicio.ObtenerPermisosDelUsuario();
        return Task.FromResult(permisos);
    }

    internal static List<UsuarioAccesoModuloDTO> MapearPermisos(Usuario usuario)
    {
        if (usuario.RolId == (int)RolEnum.SuperAdministrador)
            return [];

        return usuario.AccesosModulo
            .Select(a => new UsuarioAccesoModuloDTO { Modulo = a.Modulo, Nivel = a.Nivel })
            .ToList();
    }

    private string GenerarToken(Usuario usuario, IReadOnlyList<UsuarioAccesoModuloDTO> permisos)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, usuario.NombreUsuario),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Role, usuario.Rol.Nombre)
        };

        if (permisos.Count > 0)
            claims.Add(new Claim(PermisosClaimHelper.ClaimType, PermisosClaimHelper.Serializar(permisos)));

        string claveSecreta = _configuration.GetSection("AppSettings:Token").Value ?? "clave_secreta_por_defecto_para_desarrollo_con_longitud_suficiente_para_hmac_sha512";

        if (Encoding.UTF8.GetByteCount(claveSecreta) < 64)
            claveSecreta = claveSecreta.PadRight(64, '_');

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
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }

    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }
}
