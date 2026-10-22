using Api.Core.Entidades;
using Api.Core.Enums;

namespace Api.Core.Repositorios;

public interface IArbitroRepo : IRepositorioABM<Arbitro>
{
    Task<IEnumerable<Arbitro>> ListarPorEstado(EstadoArbitroEnum estado);
    Task<bool> ExisteConDni(string dni);
    Task EliminarAgrupadoresDelArbitro(int arbitroId);
    Task EliminarEquiposProhibidosDelArbitro(int arbitroId);
    Task EliminarFranjasHorariasDelArbitro(int arbitroId);
}
