using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class SponsorWebPublicaRepo : RepositorioABM<SponsorWebPublica>, ISponsorWebPublicaRepo
{
    public SponsorWebPublicaRepo(AppDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<SponsorWebPublica>> Listar()
    {
        return await Context.Set<SponsorWebPublica>()
            .AsNoTracking()
            .OrderBy(s => s.Orden)
            .ThenBy(s => s.Id)
            .ToListAsync();
    }

    public async Task<int> ObtenerProximoOrdenAsync(CancellationToken cancellationToken = default)
    {
        var max = await Context.Set<SponsorWebPublica>()
            .AsNoTracking()
            .Select(s => (int?)s.Orden)
            .MaxAsync(cancellationToken);
        return (max ?? 0) + 1;
    }
}
