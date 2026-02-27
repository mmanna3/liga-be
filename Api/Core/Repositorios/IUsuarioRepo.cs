namespace Api.Core.Repositorios;

public interface IUsuarioRepo
{
    Task<bool> ExisteNombreUsuario(string nombreUsuario);
}
