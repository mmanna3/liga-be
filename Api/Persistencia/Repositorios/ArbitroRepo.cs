using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class ArbitroRepo : RepositorioABM<Arbitro>, IArbitroRepo
{
    public ArbitroRepo(AppDbContext context) : base(context)
    {
    }

    protected override IQueryable<Arbitro> Set()
    {
        return Context.Set<Arbitro>()
            .Include(x => x.ArbitroTorneoAgrupadores)
                .ThenInclude(x => x.TorneoAgrupador)
            .Include(x => x.ArbitroFranjasHorarias)
            .Include("ArbitroEquiposProhibidos.Equipo.Club")
            .Include("ArbitroEquiposProhibidos.Equipo.Zonas.Zona.Fase.Torneo")
            .AsQueryable();
    }

    public async Task<IEnumerable<Arbitro>> ListarPorEstado(EstadoArbitroEnum estado)
    {
        return await Set()
            .Where(a => a.EstadoArbitroId == (int)estado)
            .ToListAsync();
    }

    public async Task<bool> ExisteConDni(string dni)
    {
        return await Context.Arbitros.AnyAsync(a => a.DNI == dni);
    }

    public async Task EliminarAgrupadoresDelArbitro(int arbitroId)
    {
        var agrupadores = await Context.ArbitroTorneoAgrupador
            .Where(a => a.ArbitroId == arbitroId)
            .ToListAsync();
        Context.ArbitroTorneoAgrupador.RemoveRange(agrupadores);
    }

    public async Task EliminarEquiposProhibidosDelArbitro(int arbitroId)
    {
        var equipos = await Context.ArbitroEquipoProhibido
            .Where(a => a.ArbitroId == arbitroId)
            .ToListAsync();
        Context.ArbitroEquipoProhibido.RemoveRange(equipos);
    }

    public async Task EliminarFranjasHorariasDelArbitro(int arbitroId)
    {
        var franjas = await Context.ArbitroFranjaHoraria
            .Where(a => a.ArbitroId == arbitroId)
            .ToListAsync();
        Context.ArbitroFranjaHoraria.RemoveRange(franjas);
    }
}
