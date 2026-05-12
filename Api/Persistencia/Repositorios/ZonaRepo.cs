using System.Linq.Expressions;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class ZonaRepo : RepositorioABMAnidado<Zona, int>, IZonaRepo
{
    public ZonaRepo(AppDbContext context) : base(context)
    {
    }

    protected override IQueryable<Zona> Set()
    {
        return Context.Set<Zona>()
            .Include("Fase.Torneo.TorneoAgrupador")
            .Include("Categoria")
            .Include(x => x.EquiposZona)
                .ThenInclude(e => e.Equipo)
                    .ThenInclude(e => e.Club)
            .Include("Fechas")
            .AsSplitQuery()
            .AsQueryable();
    }

    protected override Expression<Func<Zona, bool>> FiltroPorPadre(int padreId)
    {
        return x => x.FaseId == padreId;
    }

    public override async Task<IEnumerable<Zona>> ListarPorPadre(int padreId)
    {
        return await Set()
            .Where(FiltroPorPadre(padreId))
            .OrderBy(z => z.Orden)
            .ToListAsync();
    }

    public async Task<IEnumerable<int>> ListarIdsPorPadre(int padreId)
    {
        return await Context.Set<Zona>()
            .Where(x => x.FaseId == padreId)
            .Select(x => x.Id)
            .ToListAsync();
    }

    public async Task<Zona?> ObtenerPorIdYPadreParaEliminar(int padreId, int id)
    {
        return await Context.Set<Zona>()
            .Where(x => x.FaseId == padreId && x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task AsignarOrdenTemporal(int padreId, IEnumerable<int> ids)
    {
        var idsArray = ids.ToArray();
        if (idsArray.Length == 0)
            return;

        await Context.Set<Zona>()
            .Where(z => z.FaseId == padreId && idsArray.Contains(z.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(z => z.Orden, z => -z.Id));
    }
}
