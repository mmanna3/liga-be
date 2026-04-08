using System.Linq.Expressions;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class LeyendaTablaPosicionesRepo : RepositorioABMAnidado<LeyendaTablaPosiciones, int>, ILeyendaTablaPosicionesRepo
{
    public LeyendaTablaPosicionesRepo(AppDbContext context) : base(context)
    {
    }

    protected override IQueryable<LeyendaTablaPosiciones> Set()
    {
        return Context.Set<LeyendaTablaPosiciones>()
            .Include(x => x.Categoria)
            .Include(x => x.Equipo)
            .AsQueryable();
    }

    protected override Expression<Func<LeyendaTablaPosiciones, bool>> FiltroPorPadre(int padreId)
    {
        return x => x.ZonaId == padreId;
    }

    public async Task<bool> ExisteOtraConMismaZonaCategoriaYEquipo(int zonaId, int? categoriaId, int? equipoId,
        int? excluirLeyendaId)
    {
        var q = Context.Set<LeyendaTablaPosiciones>().AsNoTracking().Where(x => x.ZonaId == zonaId);
        if (categoriaId.HasValue)
            q = q.Where(x => x.CategoriaId == categoriaId);
        else
            q = q.Where(x => x.CategoriaId == null);

        if (equipoId.HasValue)
            q = q.Where(x => x.EquipoId == equipoId);
        else
            q = q.Where(x => x.EquipoId == null);

        if (excluirLeyendaId.HasValue)
            q = q.Where(x => x.Id != excluirLeyendaId.Value);

        return await q.AnyAsync();
    }
}
