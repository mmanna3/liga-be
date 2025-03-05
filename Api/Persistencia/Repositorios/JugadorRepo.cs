using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class JugadorRepo : RepositorioABM<Jugador>, IJugadorRepo
{
    public JugadorRepo(AppDbContext context) : base(context)
    {
    }
    
    protected override IQueryable<Jugador> Set()
    {
        return Context.Set<Jugador>()
            .Include(x => x.JugadorEquipos)
                .ThenInclude(x => x.Equipo)
                    .ThenInclude(x => x.Club)
            .Include(x => x.JugadorEquipos)
                .ThenInclude(x => x.EstadoJugador)
            .AsQueryable();
    }
    
    public virtual async Task<Jugador?> ObtenerPorDNI(string dni)
    {
        return await Context.Set<Jugador>().SingleOrDefaultAsync(x => x.DNI == dni);
    }

    public void SiElDNISeHabiaFichadoYEstaRechazadoEliminarJugador(string dni)
    {
        var jugador = Context.Jugadores.SingleOrDefault(x => x.DNI == dni);
        if (jugador != null)
            Context.Jugadores.Remove(jugador);
    }
    
    public void CambiarEstado(int jugadorEquipoId, EstadoJugadorEnum nuevoEstado)
    {
        var jugadorEquipo = Context.JugadorEquipo.SingleOrDefault(x => x.Id == jugadorEquipoId);
        if (jugadorEquipo != null)
        {
            jugadorEquipo.EstadoJugadorId = (int)nuevoEstado;
            Context.Update(jugadorEquipo);
        }
    }
}