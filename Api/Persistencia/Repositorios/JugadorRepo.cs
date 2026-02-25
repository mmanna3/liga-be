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
                .ThenInclude(x => x.Equipo)
                    .ThenInclude(x => x.Torneo)
            .Include(x => x.JugadorEquipos)
                .ThenInclude(x => x.EstadoJugador)
            .Include(x => x.JugadorEquipos)
                .ThenInclude(x => x.HistorialDePagos)
            .AsQueryable();
    }

    public async Task<IEnumerable<Jugador>> ListarConFiltro(IList<EstadoJugadorEnum> estados)
    {
        if (estados.Count == 0)
            return await Listar();
        
        return await Set()
            .Where(j => j.JugadorEquipos.Any(je => estados.Contains((EstadoJugadorEnum)je.EstadoJugadorId)))
            .ToListAsync();
    }


    public virtual async Task<Jugador?> ObtenerPorDNI(string dni)
    {
        return await Set().SingleOrDefaultAsync(x => x.DNI == dni);
    }

    public async Task<Jugador?> ObtenerPorIdParaEliminar(int id)
    {
        return await Context.Set<Jugador>()
            .Include(x => x.JugadorEquipos)
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public void SiElDNISeHabiaFichadoYEstaRechazadoEliminarJugador(string dni)
    {
        var jugador = Context.Jugadores.SingleOrDefault(x => x.DNI == dni);
        if (jugador != null)
            Context.Jugadores.Remove(jugador);
    }
    
    public void CambiarEstado(int jugadorEquipoId, EstadoJugadorEnum nuevoEstado, string? motivo = null)
    {
        var jugadorEquipo = Context.JugadorEquipo.SingleOrDefault(x => x.Id == jugadorEquipoId);
        if (jugadorEquipo != null)
        {
            if (nuevoEstado == EstadoJugadorEnum.Activo && jugadorEquipo.EstadoJugadorId == (int)EstadoJugadorEnum.AprobadoPendienteDePago)
            {
                var historialDePago = new HistorialDePagos
                {
                    JugadorEquipoId = jugadorEquipoId,
                    Fecha = DateTime.Now
                };
                Context.HistorialDePagos.Add(historialDePago);
            }
            
            jugadorEquipo.EstadoJugadorId = (int)nuevoEstado;
            jugadorEquipo.Motivo = motivo;
            
            Context.Update(jugadorEquipo);
        }
    }

    public void Eliminar(Jugador jugador)
    {
        Context.Jugadores.Remove(jugador);
    }

    public void EliminarJugadorEquipo(int jugadorEquipoId)
    {
        var jugadorEquipo = Context.JugadorEquipo.Find(jugadorEquipoId);
        if (jugadorEquipo != null)
        {
            Context.JugadorEquipo.Remove(jugadorEquipo);
        }
    }

    public async Task<bool> JugadorYaJuegaEnTorneoDelEquipoDestino(int jugadorId, int equipoOrigenId, int equipoDestinoId)
    {
        var torneoDestinoId = await Context.Equipos
            .Where(e => e.Id == equipoDestinoId)
            .Select(e => e.TorneoId)
            .FirstOrDefaultAsync();

        if (torneoDestinoId == null)
            return false;

        return await Context.JugadorEquipo
            .AnyAsync(je =>
                je.JugadorId == jugadorId &&
                je.EquipoId != equipoOrigenId &&
                je.Equipo.TorneoId == torneoDestinoId);
    }
}