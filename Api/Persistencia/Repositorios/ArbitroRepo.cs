using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;

namespace Api.Persistencia.Repositorios;

public class ArbitroRepo : RepositorioABM<Arbitro>, IArbitroRepo
{
    public ArbitroRepo(AppDbContext context) : base(context)
    {
    }
}
