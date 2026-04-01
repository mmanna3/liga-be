using System.Linq.Expressions;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class TorneoFaseRepo : RepositorioABMAnidado<TorneoFase, int>, ITorneoFaseRepo
{
    public TorneoFaseRepo(AppDbContext context) : base(context)
    {
    }

    protected override Expression<Func<TorneoFase, bool>> FiltroPorPadre(int padreId)
    {
        return x => x.TorneoId == padreId;
    }

    public override async Task<IEnumerable<TorneoFase>> ListarPorPadre(int padreId)
    {
        return await Set()
            .Include("InstanciaEliminacionDirecta")
            .Include(x => x.EstadoFase)
            .Include("Zonas")
            .Include("Zonas.Fechas")
            .Include("Zonas.EquiposZona")
            .Where(FiltroPorPadre(padreId))
            .ToListAsync();
    }

    public override async Task<TorneoFase?> ObtenerPorIdYPadre(int padreId, int id)
    {
        return await Set()
            .AsNoTracking()
            .Include("InstanciaEliminacionDirecta")
            .Include(x => x.EstadoFase)
            .Include("Zonas")
            .Include("Zonas.Fechas")
            .Include("Zonas.EquiposZona")
            .Where(FiltroPorPadre(padreId))
            .SingleOrDefaultAsync(x => x.Id == id);
    }
}
