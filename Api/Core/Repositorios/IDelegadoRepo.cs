using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IDelegadoRepo : IRepositorioABM<Delegado>
{
    Task<Delegado> ObtenerPorUsuario(string usuario);
}