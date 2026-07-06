using System.Linq.Expressions;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class GrupoDeFasesRepo : RepositorioABMAnidado<GrupoDeFases, int>, IGrupoDeFasesRepo
{
    public GrupoDeFasesRepo(AppDbContext context) : base(context)
    {
    }

    protected override Expression<Func<GrupoDeFases, bool>> FiltroPorPadre(int padreId)
    {
        return x => x.TorneoId == padreId;
    }

    public override async Task<IEnumerable<GrupoDeFases>> ListarPorPadre(int padreId)
    {
        return await Set()
            .AsNoTracking()
            .Where(FiltroPorPadre(padreId))
            .OrderBy(x => x.GrupoDeFasesPadreId)
            .ThenBy(x => x.Numero)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<List<GrupoDeFases>> ListarPorPadreParaEditar(int torneoId)
    {
        return await Set()
            .Where(FiltroPorPadre(torneoId))
            .OrderBy(x => x.Numero)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<List<GrupoDeFases>> ListarTodosPorTorneoParaEditar(int torneoId)
    {
        return await Set()
            .Where(x => x.TorneoId == torneoId)
            .OrderBy(x => x.GrupoDeFasesPadreId)
            .ThenBy(x => x.Numero)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<int> ActualizarEsVisibleEnApp(int torneoId, int grupoId, bool esVisibleEnApp)
    {
        return await Set()
            .Where(g => g.TorneoId == torneoId && g.Id == grupoId)
            .ExecuteUpdateAsync(s => s.SetProperty(g => g.EsVisibleEnApp, esVisibleEnApp));
    }
}
