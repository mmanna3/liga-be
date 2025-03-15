using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class TorneoRepo : RepositorioABM<Torneo>, ITorneoRepo
{
    public TorneoRepo(AppDbContext context) : base(context)
    {
    }
    
    protected override IQueryable<Torneo> Set()
    {
        return Context.Set<Torneo>()
            .Include(x => x.Equipos)
            .AsQueryable();
    }
} 