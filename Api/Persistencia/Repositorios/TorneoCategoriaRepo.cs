using System.Linq.Expressions;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;

namespace Api.Persistencia.Repositorios;

public class TorneoCategoriaRepo : RepositorioABMAnidado<TorneoCategoria, int>, ITorneoCategoriaRepo
{
    public TorneoCategoriaRepo(AppDbContext context) : base(context)
    {
    }

    protected override Expression<Func<TorneoCategoria, bool>> FiltroPorPadre(int padreId)
    {
        return x => x.TorneoId == padreId;
    }
}
