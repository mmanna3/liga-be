using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class UsuarioRepo : IUsuarioRepo
{
    private readonly AppDbContext _context;

    public UsuarioRepo(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteNombreUsuario(string nombreUsuario)
    {
        return await _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario);
    }
}
