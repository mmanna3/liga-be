using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class DelegadoRepo : RepositorioABM<Delegado>, IDelegadoRepo
{
    public DelegadoRepo(AppDbContext context) : base(context)
    {
    }
    
    protected override IQueryable<Delegado> Set()
    {
        return Context.Set<Delegado>()
            .Include(x => x.Club)
                .ThenInclude(x => x.Equipos)
            .Include(x => x.Usuario)
            .Include(x => x.EstadoDelegado)
            .AsQueryable();
    }
    
    public virtual async Task<Delegado?> ObtenerPorDNI(string dni)
    {
        return await Set().AsNoTracking().FirstOrDefaultAsync(x => x.DNI == dni);
    }

    public virtual async Task<Delegado> ObtenerPorUsuario(string usuario)
    {
        return await Set()
            .AsNoTracking()
            .Include(x => x.Club.Equipos)
                .ThenInclude(x => x.Torneo)
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