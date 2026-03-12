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
            .Include(x => x.ZonaExcluyente)
                .ThenInclude(z => z!.TorneoFase)
                    .ThenInclude(f => f.Torneo)
            .Include(x => x.Jugadores)
                .ThenInclude(x => x.Jugador)
            .Include(x => x.Jugadores)
                .ThenInclude(x => x.EstadoJugador)
            .AsQueryable();
    }

    public async Task<bool> ExisteEquipoConMismoNombreEnZona(string nombre, int? zonaActualId, int? equipoIdExcluir = null)
    {
        var query = Context.Set<Equipo>()
            .Where(e => e.Nombre.ToLower() == nombre.ToLower() && e.ZonaExcluyenteId == zonaActualId);
            
        if (equipoIdExcluir.HasValue)
        {
            query = query.Where(e => e.Id != equipoIdExcluir.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<int> ContarEquiposDelJugador(int jugadorId)
    {
        return await Context.JugadorEquipo.CountAsync(je => je.JugadorId == jugadorId);
    }

    public async Task QuitarEquiposDeZona(int zonaId)
    {
        var equipos = await Context.Set<Equipo>()
            .Where(e => e.ZonaExcluyenteId == zonaId)
            .ToListAsync();
        foreach (var equipo in equipos)
        {
            equipo.ZonaExcluyenteId = null;
        }
    }

    public async Task AsignarEquiposAZona(int zonaId, IEnumerable<int> equipoIds)
    {
        var ids = equipoIds.Distinct().ToList();
        if (ids.Count == 0)
            return;

        var equipos = await Context.Set<Equipo>()
            .Where(e => ids.Contains(e.Id))
            .ToListAsync();
        foreach (var equipo in equipos)
        {
            equipo.ZonaExcluyenteId = zonaId;
        }
    }
}