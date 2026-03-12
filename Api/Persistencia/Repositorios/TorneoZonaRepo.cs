using System.Linq.Expressions;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class TorneoZonaRepo : RepositorioABMAnidado<TorneoZona, int>, ITorneoZonaRepo
{
    public TorneoZonaRepo(AppDbContext context) : base(context)
    {
    }

    protected override IQueryable<TorneoZona> Set()
    {
        return Context.Set<TorneoZona>()
            .Include(x => x.Equipos)
                .ThenInclude(e => e.Club)
            .Include(x => x.Fechas)
            .AsQueryable();
    }

    protected override Expression<Func<TorneoZona, bool>> FiltroPorPadre(int padreId)
    {
        return x => x.TorneoFaseId == padreId;
    }

    public async Task<IEnumerable<int>> ListarIdsPorPadre(int padreId)
    {
        return await Context.Set<TorneoZona>()
            .Where(x => x.TorneoFaseId == padreId)
            .Select(x => x.Id)
            .ToListAsync();
    }
}
