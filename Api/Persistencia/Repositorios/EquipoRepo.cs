using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class EquipoRepo : RepositorioABM<Equipo>, IEquipoRepo
{
    public EquipoRepo(AppDbContext context) : base(context)
    {
    }
    
    protected override IQueryable<Equipo> Set()
    {
        return Context.Set<Equipo>()
            .Include(x => x.Club)
            .Include("Zonas.Zona.Fase.Torneo.TorneoAgrupador")
            .Include(x => x.Jugadores)
                .ThenInclude(x => x.Jugador)
            .Include(x => x.Jugadores)
                .ThenInclude(x => x.EstadoJugador)
            .AsSplitQuery()
            .AsQueryable();
    }

    public async Task<bool> ExisteEquipoConMismoNombreEnZona(string nombre, IEnumerable<int> zonaIds, int? equipoIdExcluir = null)
    {
        var ids = zonaIds?.ToList() ?? [];
        var nombreLower = nombre.ToLower();

        if (ids.Count == 0)
        {
            var query = Context.Set<Equipo>()
                .Where(e => e.Nombre.ToLower() == nombreLower && !e.Zonas.Any());
            if (equipoIdExcluir.HasValue)
                query = query.Where(e => e.Id != equipoIdExcluir.Value);
            return await query.AnyAsync();
        }

        var query2 = Context.Set<EquipoZona>()
            .Where(ez => ids.Contains(ez.ZonaId))
            .Where(ez => ez.Equipo.Nombre.ToLower() == nombreLower);
        if (equipoIdExcluir.HasValue)
            query2 = query2.Where(ez => ez.EquipoId != equipoIdExcluir.Value);
        return await query2.AnyAsync();
    }

    public async Task<int> ContarEquiposDelJugador(int jugadorId)
    {
        return await Context.JugadorEquipo.CountAsync(je => je.JugadorId == jugadorId);
    }

    public async Task QuitarEquiposDeZona(int zonaId)
    {
        var registros = await Context.Set<EquipoZona>()
            .Where(e => e.ZonaId == zonaId)
            .ToListAsync();
        Context.Set<EquipoZona>().RemoveRange(registros);
    }

    public async Task AsignarEquiposAZona(int zonaId, IEnumerable<int> equipoIds)
    {
        var ids = equipoIds.Distinct().ToList();
        if (ids.Count == 0)
            return;

        foreach (var equipoId in ids)
        {
            var existe = await Context.Set<EquipoZona>()
                .AnyAsync(e => e.EquipoId == equipoId && e.ZonaId == zonaId);
            if (!existe)
            {
                Context.Set<EquipoZona>().Add(new EquipoZona
                {
                    Id = 0,
                    EquipoId = equipoId,
                    ZonaId = zonaId
                });
            }
        }
    }

    public async Task SincronizarZonasDelEquipo(int equipoId, IEnumerable<int> zonaIds)
    {
        var registrosActuales = await Context.Set<EquipoZona>()
            .Where(ez => ez.EquipoId == equipoId)
            .ToListAsync();
        Context.Set<EquipoZona>().RemoveRange(registrosActuales);

        var ids = zonaIds.Distinct().ToList();
        foreach (var zonaId in ids)
        {
            Context.Set<EquipoZona>().Add(new EquipoZona
            {
                Id = 0,
                EquipoId = equipoId,
                ZonaId = zonaId
            });
        }
    }

    public async Task<IEnumerable<Equipo>> ListarConZonasParaEquiposParaZonas()
    {
        return await Context.Set<Equipo>()
            .Include(e => e.Club)
            .Include("Zonas.Zona.Fase.Torneo.TorneoAgrupador")
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Equipo>> ListarPorZonaIdAsync(int zonaId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Equipo>()
            .AsNoTracking()
            .Where(e => e.Zonas.Any(z => z.ZonaId == zonaId))
            .Include(e => e.Club)
            .OrderBy(e => e.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<int>> ListarTorneoIdsDelEquipoEnAnioAsync(int equipoId, int anio,
        CancellationToken cancellationToken = default)
    {
        return await (
            from ez in Context.Set<EquipoZona>()
            join z in Context.Zonas on ez.ZonaId equals z.Id
            join f in Context.Fases on z.FaseId equals f.Id
            join t in Context.Torneos on f.TorneoId equals t.Id
            where ez.EquipoId == equipoId && t.Anio == anio
            select t.Id).Distinct().ToListAsync(cancellationToken);
    }
}
