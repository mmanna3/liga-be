using Api.Core.Entidades;
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
            .Include("ArbitroEquiposProhibidos.Equipo.Club")
            .Include("ArbitroEquiposProhibidos.Equipo.Zonas.Zona.Fase.Torneo")
            .AsQueryable();
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
}
