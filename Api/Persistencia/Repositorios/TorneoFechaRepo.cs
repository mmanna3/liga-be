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
        return Context.Set<TorneoFecha>()
            .Include("InstanciaEliminacionDirecta")
            .Include(x => x.Jornadas)
            .Include(x => x.Jornadas)
                .ThenInclude(j => ((JornadaNormal)j).LocalEquipo)
            .Include(x => x.Jornadas)
                .ThenInclude(j => ((JornadaNormal)j).VisitanteEquipo)
            .Include(x => x.Jornadas)
                .ThenInclude(j => ((JornadaLibre)j).Equipo)
            .Include(x => x.Jornadas)
                .ThenInclude(j => ((JornadaInterzonal)j).Equipo)
            .AsQueryable();
    }

    public async Task<IEnumerable<int>> ListarIdsPorPadre(int padreId)
    {
        return await Context.Set<TorneoFecha>()
            .Where(x => x.ZonaId == padreId)
            .Select(x => x.Id)
            .ToListAsync();
    }

    public async Task<TorneoFecha?> ObtenerPorIdYPadreParaEliminar(int padreId, int id)
    {
        return await Context.Set<TorneoFecha>()
            .Where(x => x.ZonaId == padreId && x.Id == id)
            .FirstOrDefaultAsync();
    }
}
