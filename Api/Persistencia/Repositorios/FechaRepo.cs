using System.Linq.Expressions;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class FechaRepo : RepositorioABMAnidado<Fecha, int>, IFechaRepo
{
    public FechaRepo(AppDbContext context) : base(context)
    {
    }

    protected override Expression<Func<Fecha, bool>> FiltroPorPadre(int padreId)
    {
        return x => x.ZonaId == padreId;
    }

    protected override IQueryable<Fecha> Set()
    {
        return Context.Set<Fecha>()
            .Include("Instancia")
            .Include(x => x.Jornadas)
                .ThenInclude(j => ((JornadaNormal)j).LocalEquipo)
            .Include(x => x.Jornadas)
                .ThenInclude(j => ((JornadaNormal)j).VisitanteEquipo)
            .Include(x => x.Jornadas)
                .ThenInclude(j => ((JornadaLibre)j).EquipoLocal)
            .Include(x => x.Jornadas)
                .ThenInclude(j => ((JornadaInterzonal)j).Equipo)
            .Include(x => x.Jornadas)
                .ThenInclude(j => j.Partidos)
                .ThenInclude(p => p.Categoria)
            .AsSplitQuery()
            .AsQueryable();
    }

    public async Task<IEnumerable<int>> ListarIdsPorPadre(int padreId)
    {
        return await Context.Set<Fecha>()
            .Where(x => x.ZonaId == padreId)
            .Select(x => x.Id)
            .ToListAsync();
    }

    public async Task<Fecha?> ObtenerPorIdYPadreParaEliminar(int padreId, int id)
    {
        return await Context.Set<Fecha>()
            .Where(x => x.ZonaId == padreId && x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<int> ActualizarEsVisibleEnApp(int zonaId, int fechaId, bool esVisibleEnApp)
    {
        return await Context.Set<Fecha>()
            .Where(f => f.ZonaId == zonaId && f.Id == fechaId)
            .ExecuteUpdateAsync(s => s.SetProperty(f => f.EsVisibleEnApp, esVisibleEnApp));
    }

    public async Task<IReadOnlyList<FechaTodosContraTodos>> ListarTodosContraTodosPorZonaParaAppAsync(int zonaId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<FechaTodosContraTodos>()
            .AsNoTracking()
            .Where(f => f.ZonaId == zonaId && f.EsVisibleEnApp)
            .OrderBy(f => f.Numero)
            .Include(x => x.Jornadas)
            .ThenInclude(j => ((JornadaNormal)j).LocalEquipo)
            .Include(x => x.Jornadas)
            .ThenInclude(j => ((JornadaNormal)j).VisitanteEquipo)
            .Include(x => x.Jornadas)
            .ThenInclude(j => ((JornadaLibre)j).EquipoLocal)
            .Include(x => x.Jornadas)
            .ThenInclude(j => ((JornadaInterzonal)j).Equipo)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }
}
