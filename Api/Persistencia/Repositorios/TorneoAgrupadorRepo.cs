using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class TorneoAgrupadorRepo : RepositorioABM<TorneoAgrupador>, ITorneoAgrupadorRepo
{
    public TorneoAgrupadorRepo(AppDbContext context) : base(context)
    {
    }

    protected override IQueryable<TorneoAgrupador> Set()
    {
        return Context.Set<TorneoAgrupador>()
            .Include(x => x.Torneos)
            .AsQueryable();
    }
}
