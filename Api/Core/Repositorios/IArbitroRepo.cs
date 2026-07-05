using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IArbitroRepo : IRepositorioABM<Arbitro>
{
    Task EliminarAgrupadoresDelArbitro(int arbitroId);
    Task EliminarEquiposProhibidosDelArbitro(int arbitroId);
}
