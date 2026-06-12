using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IFaseCategoriaRepo : IRepositorioABMAnidado<FaseCategoria, int>
{
    Task<List<FaseCategoria>> ListarPorPadreOrdenadasParaEditar(int faseId);

    Task<bool> AlgunaTienePartidosOZonasOLeyendas(IEnumerable<int> categoriaIds);

    Task<List<FaseCategoria>> ListarPorFaseIds(IEnumerable<int> faseIds);
}
