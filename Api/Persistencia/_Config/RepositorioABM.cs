using Api.Core.Entidades;
using Api.Core.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Api.Persistencia._Config;

public abstract class RepositorioABM<TModel> : RepositorioBase, IRepositorioABM<TModel>
    where TModel : Entidad
{
    protected RepositorioABM(AppDbContext context) : base(context)
    {
    }

    protected virtual IQueryable<TModel> Set()
    {
        return Context.Set<TModel>();
    }
    
    public virtual async Task<IEnumerable<TModel>> Listar()
    {
        return await Set().ToListAsync();
    }

    public EntityEntry<TModel> Crear(TModel modelo)
    {
        AntesDeCrear(modelo);
        return Context.Add(modelo);
    }

    public virtual async Task<TModel?> ObtenerPorId(int id)
    {
        return await Set().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
    }

    public virtual async Task<IEnumerable<TModel>> ObtenerPorIds(IEnumerable<int> ids)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return [];

        var entidades = await Set().AsNoTracking().Where(x => idList.Contains(x.Id)).ToListAsync();
        var idToIndex = idList.Select((id, idx) => (id, idx)).ToDictionary(x => x.id, x => x.idx);
        return entidades.OrderBy(e => idToIndex[e.Id]);
    }

    public void Modificar(TModel anterior, TModel nuevo)
    {
        AntesDeModificar(anterior, nuevo);
        Context.Update(nuevo);
        DespuesDeModificar(anterior, nuevo);
    }

    public virtual void Eliminar(TModel modelo)
    {
        Context.Remove(modelo);
    }
    
    protected virtual void AntesDeModificar(TModel entidadAnterior, TModel entidadNueva)
    {
    }
    
    protected virtual void DespuesDeModificar(TModel entidadAnterior, TModel entidadNueva)
    {
    }
    
    protected virtual void AntesDeCrear(TModel entidad)
    {
    }
}