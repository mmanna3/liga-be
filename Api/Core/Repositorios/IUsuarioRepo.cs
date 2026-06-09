using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IUsuarioRepo : IRepositorioABM<Usuario>
{
    Task<bool> ExisteNombreUsuario(string nombreUsuario);
    Task<bool> ExisteNombreUsuarioExceptoId(string nombreUsuario, int id);
    Task<int> ContarUsuariosConRoles(IEnumerable<int> rolIds);
}
