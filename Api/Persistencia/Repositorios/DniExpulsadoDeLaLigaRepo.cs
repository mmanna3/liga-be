using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class DniExpulsadoDeLaLigaRepo : RepositorioABM<DniExpulsadoDeLaLiga>, IDniExpulsadoDeLaLigaRepo
{
    public DniExpulsadoDeLaLigaRepo(AppDbContext context) : base(context)
    {
    }

    public Task<bool> ExistePorDniAsync(int dni, CancellationToken cancellationToken = default) =>
        Context.Set<DniExpulsadoDeLaLiga>().AsNoTracking().AnyAsync(x => x.DNI == dni, cancellationToken);
}
