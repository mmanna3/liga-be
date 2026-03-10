using System.Linq.Expressions;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;

namespace Api.Persistencia.Repositorios;

public class TorneoZonaRepo : RepositorioABMAnidado<TorneoZona, int>, ITorneoZonaRepo
{
    public TorneoZonaRepo(AppDbContext context) : base(context)
    {
    }

    protected override Expression<Func<TorneoZona, bool>> FiltroPorPadre(int padreId)
    {
        return x => x.TorneoFaseId == padreId;
    }
}
