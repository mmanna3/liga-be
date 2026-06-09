using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class UsuarioRepo : RepositorioABM<Usuario>, IUsuarioRepo
{
    public UsuarioRepo(AppDbContext context) : base(context)
    {
    }

    protected override IQueryable<Usuario> Set()
    {
        return Context.Usuarios
            .Include(u => u.Rol)
            .Where(u => u.DelegadoId == null);
    }

    public async Task<bool> ExisteNombreUsuario(string nombreUsuario)
    {
        return await Context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario);
    }

    public async Task<bool> ExisteNombreUsuarioExceptoId(string nombreUsuario, int id)
    {
        return await Context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario && u.Id != id);
    }

    public async Task<int> ContarUsuariosConRoles(IEnumerable<int> rolIds)
    {
        var ids = rolIds.ToList();
        return await Context.Usuarios.CountAsync(u => u.DelegadoId == null && ids.Contains(u.RolId));
    }
}
