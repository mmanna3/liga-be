using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IDelegadoRepo : IRepositorioABM<Delegado>
{
    Task<Delegado?> ObtenerPorDNI(string dni);
    Task<Delegado> ObtenerPorUsuario(string usuario);
    void Eliminar(Delegado delegado);
}