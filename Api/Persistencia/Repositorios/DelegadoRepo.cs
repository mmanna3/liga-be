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
}