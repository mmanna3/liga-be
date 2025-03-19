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
            .Include(x => x.Usuario)
            .AsQueryable();
    }
    
    public virtual async Task<Delegado> ObtenerPorUsuario(string usuario)
    {
        return await Set()
            .AsNoTracking()
            .Include(x => x.Club.Equipos)
                .ThenInclude(x => x.Torneo)
            .SingleOrDefaultAsync(x => x.Usuario.NombreUsuario == usuario) 
               ?? throw new InvalidOperationException();
    }
}