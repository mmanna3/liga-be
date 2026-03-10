using System.Linq.Expressions;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia._Config;

/// <summary>
/// Repositorio base para entidades que siempre dependen de un padre.
/// La clase derivada debe implementar FiltroPorPadre para definir cómo filtrar por el ID del padre.
/// </summary>
public abstract class RepositorioABMAnidado<TModel, TPadreId> : RepositorioABM<TModel>, IRepositorioABMAnidado<TModel, TPadreId>
    where TModel : Entidad
{
    protected RepositorioABMAnidado(AppDbContext context) : base(context)
    {
    }

    protected abstract Expression<Func<TModel, bool>> FiltroPorPadre(TPadreId padreId);

    public virtual async Task<IEnumerable<TModel>> ListarPorPadre(TPadreId padreId)
    {
        return await Set().Where(FiltroPorPadre(padreId)).ToListAsync();
    }

    public virtual async Task<TModel?> ObtenerPorIdYPadre(TPadreId padreId, int id)
    {
        return await Set()
            .AsNoTracking()
            .Where(FiltroPorPadre(padreId))
            .SingleOrDefaultAsync(x => x.Id == id);
    }
}
