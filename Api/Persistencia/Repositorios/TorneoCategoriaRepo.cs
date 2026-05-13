using System.Linq;
using System.Linq.Expressions;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class TorneoCategoriaRepo : RepositorioABMAnidado<TorneoCategoria, int>, ITorneoCategoriaRepo
{
    public TorneoCategoriaRepo(AppDbContext context) : base(context)
    {
    }

    protected override Expression<Func<TorneoCategoria, bool>> FiltroPorPadre(int padreId)
    {
        return x => x.TorneoId == padreId;
    }

    public override async Task<IEnumerable<TorneoCategoria>> ListarPorPadre(int padreId)
    {
        return await Set()
            .AsNoTracking()
            .Where(FiltroPorPadre(padreId))
            .OrderBy(x => x.Orden)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<List<TorneoCategoria>> ListarPorPadreOrdenadasParaEditar(int padreId)
    {
        return await Set()
            .Where(FiltroPorPadre(padreId))
            .OrderBy(x => x.Orden)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<bool> AlgunaTienePartidosOZonas(IEnumerable<int> categoriaIds)
    {
        var ids = categoriaIds as IList<int> ?? categoriaIds.ToList();
        if (ids.Count == 0)
            return false;

        var tienePartidos = await Context.Partidos
            .AsNoTracking()
            .AnyAsync(p => ids.Contains(p.CategoriaId));

        if (tienePartidos)
            return true;

        return await Context.Zonas
            .OfType<ZonaEliminacionDirecta>()
            .AsNoTracking()
            .AnyAsync(z => ids.Contains(z.CategoriaId));
    }
}
