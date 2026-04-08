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

    public async Task<IReadOnlyList<FechaTodosContraTodos>> ListarTodosContraTodosPorZonaParaAppConPartidosAsync(
        int zonaId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<FechaTodosContraTodos>()
            .AsNoTracking()
            .Where(f => f.ZonaId == zonaId && f.EsVisibleEnApp)
            .OrderBy(f => f.Numero)
            .Include(x => x.Zona)
            .ThenInclude(z => z.Fase)
            .ThenInclude(f => f.Torneo)
            .ThenInclude(t => t.Categorias)
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
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TorneoCategoria>> ListarCategoriasTorneoPorZonaTodosContraTodosAsync(int zonaId,
        CancellationToken cancellationToken = default)
    {
        var zona = await Context.Set<ZonaTodosContraTodos>()
            .AsNoTracking()
            .Where(z => z.Id == zonaId)
            .Include(z => z.Fase)
            .ThenInclude(f => f.Torneo)
            .ThenInclude(t => t.Categorias)
            .FirstOrDefaultAsync(cancellationToken);

        if (zona == null)
            return Array.Empty<TorneoCategoria>();

        return zona.Fase.Torneo.Categorias.OrderBy(c => c.Id).ToList();
    }

    public async Task<(int ZonaAperturaId, int ZonaClausuraId)?> ObtenerIdsZonasAnualPorZonaReferenciaAsync(int zonaId,
        CancellationToken cancellationToken = default)
    {
        var zonaRef = await Context.Set<ZonaTodosContraTodos>()
            .AsNoTracking()
            .Where(z => z.Id == zonaId)
            .Include(z => z.Fase)
            .ThenInclude(f => f.Torneo)
            .FirstOrDefaultAsync(cancellationToken);

        if (zonaRef == null)
            return null;

        var torneo = zonaRef.Fase.Torneo;
        if (torneo.FaseAperturaId is not { } idFa || torneo.FaseClausuraId is not { } idFc)
            return null;

        var nombreClave = zonaRef.Nombre.Trim();
        var candidatos = await Context.Set<ZonaTodosContraTodos>()
            .AsNoTracking()
            .Where(z => z.FaseId == idFa || z.FaseId == idFc)
            .Select(z => new { z.Id, z.FaseId, z.Nombre })
            .ToListAsync(cancellationToken);

        var za = candidatos.FirstOrDefault(z => z.FaseId == idFa && z.Nombre.Trim() == nombreClave);
        var zc = candidatos.FirstOrDefault(z => z.FaseId == idFc && z.Nombre.Trim() == nombreClave);
        if (za == null || zc == null)
            return null;

        return (za.Id, zc.Id);
    }

    /// <summary>App carnet: incluir fechas aunque las jornadas aún no tengan fila <see cref="Partido"/>.</summary>
    public async Task<IReadOnlyList<FechaEliminacionDirecta>> ListarEliminacionDirectaPorZonaParaAppAsync(int zonaId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<FechaEliminacionDirecta>()
            .AsNoTracking()
            .Where(f => f.ZonaId == zonaId && f.EsVisibleEnApp)
            .OrderByDescending(f => f.InstanciaId)
            .Include(f => f.Instancia)
            .Include(f => f.Zona)
            .Include(f => f.Jornadas)
            .ThenInclude(j => ((JornadaNormal)j).LocalEquipo)
            .Include(f => f.Jornadas)
            .ThenInclude(j => ((JornadaNormal)j).VisitanteEquipo)
            .Include(f => f.Jornadas)
            .ThenInclude(j => ((JornadaLibre)j).EquipoLocal)
            .Include(f => f.Jornadas)
            .ThenInclude(j => ((JornadaInterzonal)j).Equipo)
            .Include(f => f.Jornadas)
            .ThenInclude(j => j.Partidos)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }
}
