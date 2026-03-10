using System.Linq.Expressions;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class TorneoFechaRepo : RepositorioABMAnidado<TorneoFecha, int>, ITorneoFechaRepo
{
    public TorneoFechaRepo(AppDbContext context) : base(context)
    {
    }

    protected override Expression<Func<TorneoFecha, bool>> FiltroPorPadre(int padreId)
    {
        return x => x.ZonaId == padreId;
    }

    protected override IQueryable<TorneoFecha> Set()
    {
        return base.Set()
            .Include(x => x.InstanciaEliminacionDirecta)
            .AsQueryable();
    }
}
