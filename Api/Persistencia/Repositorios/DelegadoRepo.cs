using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class DelegadoRepo : RepositorioABM<Delegado>, IDelegadoRepo
{
    public DelegadoRepo(AppDbContext context) : base(context)
    {
    }
    
    public async Task<List<Delegado>> ListarActivosDelClub(int clubId)
    {
        return await Context.DelegadoClub
            .Where(dc => dc.ClubId == clubId && dc.EstadoDelegadoId == (int)EstadoDelegadoEnum.Activo)
            .Select(dc => dc.Delegado)
            .Distinct()
            .AsNoTracking()
            .ToListAsync();
    }

    protected override IQueryable<Delegado> Set()
    {
        return Context.Set<Delegado>()
            .Include(x => x.DelegadoClubs)
                .ThenInclude(dc => dc.Club)
                    .ThenInclude(c => c.Equipos)
                        .ThenInclude(e => e.Torneo)
            .Include(x => x.DelegadoClubs)
                .ThenInclude(dc => dc.EstadoDelegado)
            .Include(x => x.Usuario)
            .AsQueryable();
    }

    public async Task<List<(Delegado Delegado, int? JugadorId)>> ListarConFiltroConJugadorIds(IList<EstadoDelegadoEnum> estados)
    {
        if (estados == null || estados.Count == 0)
            return await ListarConJugadorIds();
        var estadoIds = estados.Select(e => (int)e).ToHashSet();
        var query = from d in Set()
                    join j in Context.Jugadores on d.DNI equals j.DNI into jGroup
                    from j in jGroup.DefaultIfEmpty()
                    where d.DelegadoClubs.Any(dc => estadoIds.Contains(dc.EstadoDelegadoId))
                    select new { Delegado = d, JugadorId = (int?)j.Id };
        var result = await query.AsNoTracking().ToListAsync();
        return result.Select(x => (x.Delegado, x.JugadorId)).ToList();
    }

    public virtual async Task<Delegado?> ObtenerPorDNI(string dni)
    {
        return await Set().AsNoTracking().FirstOrDefaultAsync(x => x.DNI == dni);
    }

    public virtual async Task<Delegado> ObtenerPorUsuario(string usuario)
    {
        return await Set()
            .AsNoTracking()
            .Where(x => x.Usuario != null && x.Usuario.NombreUsuario == usuario)
            .SingleOrDefaultAsync()
               ?? throw new InvalidOperationException();
    }
    
    public void Eliminar(Delegado delegado)
    {
        Context.Delegados.Remove(delegado);
    }

    public async Task<List<(Delegado Delegado, int? JugadorId)>> ListarConJugadorIds()
    {
        var query = from d in Set()
                    join j in Context.Jugadores on d.DNI equals j.DNI into jGroup
                    from j in jGroup.DefaultIfEmpty()
                    select new { Delegado = d, JugadorId = (int?)j.Id };
        var result = await query.AsNoTracking().ToListAsync();
        return result.Select(x => (x.Delegado, x.JugadorId)).ToList();
    }

    public async Task<int?> ObtenerJugadorIdPorDNI(string dni)
    {
        return await Context.Jugadores
            .Where(j => j.DNI == dni)
            .Select(j => (int?)j.Id)
            .FirstOrDefaultAsync();
    }
}