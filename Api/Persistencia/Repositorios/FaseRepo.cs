using System.Linq.Expressions;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class FaseRepo : RepositorioABMAnidado<Fase, int>, IFaseRepo
{
    public FaseRepo(AppDbContext context) : base(context)
    {
    }

    protected override Expression<Func<Fase, bool>> FiltroPorPadre(int padreId)
    {
        return x => x.TorneoId == padreId;
    }

    public override async Task<IEnumerable<Fase>> ListarPorPadre(int padreId)
    {
        return await Set()
            .Include(x => x.EstadoFase)
            .Include("Zonas")
            .Include("Zonas.Categoria")
            .Include("Zonas.Fechas")
            .Include("Zonas.EquiposZona")
            .AsSplitQuery()
            .Where(FiltroPorPadre(padreId))
            .ToListAsync();
    }

    public override async Task<Fase?> ObtenerPorIdYPadre(int padreId, int id)
    {
        return await Set()
            .AsNoTracking()
            .Include(x => x.EstadoFase)
            .Include("Zonas")
            .Include("Zonas.Categoria")
            .Include("Zonas.Fechas")
            .Include("Zonas.EquiposZona")
            .AsSplitQuery()
            .Where(FiltroPorPadre(padreId))
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task DecrementarNumeroDeFasesPosteriores(int torneoId, int numeroEliminado)
    {
        await Context.Database.ExecuteSqlRawAsync(
            "UPDATE [Fases] SET [Numero] = [Numero] - 1 WHERE [TorneoId] = {0} AND [Numero] > {1}",
            torneoId, numeroEliminado);
    }

    public async Task CambiarTipo(int padreId, int id, TipoDeFaseEnum nuevoTipo)
    {
        var discriminador = nuevoTipo == TipoDeFaseEnum.TodosContraTodos
            ? "TodosContraTodos"
            : "EliminacionDirecta";
        await Context.Database.ExecuteSqlRawAsync(
            "UPDATE [Fases] SET [TipoFase] = {0} WHERE [Id] = {1} AND [TorneoId] = {2}",
            discriminador, id, padreId);
    }
}
