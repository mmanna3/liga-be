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
            .Include(x => x.Jugadores)
                .ThenInclude(x => x.Jugador)
            .Include(x => x.Jugadores)
                .ThenInclude(x => x.EstadoJugador)
            .AsQueryable();
    }
}