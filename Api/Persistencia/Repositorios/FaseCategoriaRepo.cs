using System.Linq.Expressions;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class FaseCategoriaRepo : RepositorioABMAnidado<FaseCategoria, int>, IFaseCategoriaRepo
{
    public FaseCategoriaRepo(AppDbContext context) : base(context)
    {
    }

    protected override Expression<Func<FaseCategoria, bool>> FiltroPorPadre(int padreId)
    {
        return x => x.FaseId == padreId;
    }

    public override async Task<IEnumerable<FaseCategoria>> ListarPorPadre(int padreId)
    {
        return await Set()
            .AsNoTracking()
            .Where(FiltroPorPadre(padreId))
            .OrderBy(x => x.Orden)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<List<FaseCategoria>> ListarPorPadreOrdenadasParaEditar(int faseId)
    {
        return await Set()
            .Where(FiltroPorPadre(faseId))
            .OrderBy(x => x.Orden)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<bool> AlgunaTienePartidosOZonasOLeyendas(IEnumerable<int> categoriaIds)
    {
        var ids = categoriaIds as IList<int> ?? categoriaIds.ToList();
        if (ids.Count == 0)
            return false;

        if (await Context.Partidos.AsNoTracking().AnyAsync(p => ids.Contains(p.CategoriaId)))
            return true;

        if (await Context.Zonas
                .OfType<ZonaEliminacionDirecta>()
                .AsNoTracking()
                .AnyAsync(z => ids.Contains(z.CategoriaId)))
            return true;

        return await Context.LeyendaTablaPosiciones
            .AsNoTracking()
            .AnyAsync(l => l.CategoriaId != null && ids.Contains(l.CategoriaId.Value));
    }

    public async Task<List<FaseCategoria>> ListarPorFaseIds(IEnumerable<int> faseIds)
    {
        var ids = faseIds.ToList();
        if (ids.Count == 0)
            return [];

        return await Set()
            .AsNoTracking()
            .Where(c => ids.Contains(c.FaseId))
            .OrderBy(c => c.FaseId)
            .ThenBy(c => c.Orden)
            .ThenBy(c => c.Id)
            .ToListAsync();
    }
}
