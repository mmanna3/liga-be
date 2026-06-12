using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IGrupoDeFasesRepo : IRepositorioABMAnidado<GrupoDeFases, int>
{
    Task<List<GrupoDeFases>> ListarPorPadreParaEditar(int torneoId);
    Task<List<GrupoDeFases>> ListarTodosPorTorneoParaEditar(int torneoId);
}
