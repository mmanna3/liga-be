using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class FixtureAlgoritmoRepo : RepositorioABM<FixtureAlgoritmo>, IFixtureAlgoritmoRepo
{
    public FixtureAlgoritmoRepo(AppDbContext context) : base(context)
    {
    }

    protected override IQueryable<FixtureAlgoritmo> Set()
    {
        return Context.Set<FixtureAlgoritmo>()
            .Include(fa => fa.Fechas)
            .AsQueryable();
    }

    public async Task EliminarFechasDelFixture(int fixtureAlgoritmoId)
    {
        var fechas = await Context.FixtureAlgoritmoFecha
            .Where(f => f.FixtureAlgoritmoId == fixtureAlgoritmoId)
            .ToListAsync();
        Context.FixtureAlgoritmoFecha.RemoveRange(fechas);
    }
}
