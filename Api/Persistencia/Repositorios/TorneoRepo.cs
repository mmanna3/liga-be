using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class TorneoRepo : RepositorioABM<Torneo>, ITorneoRepo
{
    public TorneoRepo(AppDbContext context) : base(context)
    {
    }
    
    protected override IQueryable<Torneo> Set()
    {
        return Context.Set<Torneo>()
            .Include(x => x.TorneoAgrupador)
            .Include(x => x.FaseApertura)
            .Include(x => x.FaseClausura)
            .Include(x => x.Categorias)
            .Include("Fases.Zonas")
            .Include("Fases.Zonas.Categoria")
            .Include("Fases.Zonas.Fechas")
            .Include("Fases.Zonas.EquiposZona")
            .Include(x => x.Fases)
                .ThenInclude(f => f.EstadoFase)
            .AsSplitQuery()
            .AsQueryable();
    }

    public async Task<Zona?> ObtenerZonaUnicaDeTorneo(int torneoId)
    {
        return await Context.Zonas
            .OfType<ZonaTodosContraTodos>()
            .Include(tz => tz.Fase)
            .Where(tz => tz.Fase.TorneoId == torneoId && tz.Fase.Numero == 1 && tz.Nombre == "Zona única")
            .FirstOrDefaultAsync();
    }

    public async Task CrearFaseUnicaYZonaUnica(int torneoId)
    {
        var fase = new FaseTodosContraTodos
        {
            Id = 0,
            TorneoId = torneoId,
            Nombre = string.Empty,
            Numero = 1,
            EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
            EsVisibleEnApp = true
        };
        Context.Fases.Add(fase);
        await Context.SaveChangesAsync();

        var zona = new ZonaTodosContraTodos
        {
            Id = 0,
            FaseId = fase.Id,
            Nombre = "Zona única"
        };
        Context.Zonas.Add(zona);
        await Context.SaveChangesAsync();
    }

    public async Task<bool> ExisteTorneoConNombreAnioYAgrupador(string nombre, int anio, int torneoAgrupadorId, int? excluirId = null)
    {
        var query = Context.Torneos.Where(t => t.Nombre == nombre && t.Anio == anio && t.TorneoAgrupadorId == torneoAgrupadorId);
        if (excluirId.HasValue)
            query = query.Where(t => t.Id != excluirId.Value);
        return await query.AnyAsync();
    }

    public async Task<IEnumerable<Torneo>> ListarFiltrado(int? anio, int? torneoAgrupadorId)
    {
        var query = Set();

        if (anio.HasValue)
            query = query.Where(t => t.Anio == anio.Value);
        if (torneoAgrupadorId.HasValue)
            query = query.Where(t => t.TorneoAgrupadorId == torneoAgrupadorId.Value);

        return await query.ToListAsync();
    }

    public async Task<int> ActualizarEsVisibleEnApp(int id, bool esVisibleEnApp)
    {
        return await Context.Set<Torneo>()
            .Where(t => t.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.EsVisibleEnApp, esVisibleEnApp));
    }

    public async Task<int> ActualizarFasesParaTablaAnual(int torneoId, int? faseAperturaId, int? faseClausuraId)
    {
        return await Context.Set<Torneo>()
            .Where(t => t.Id == torneoId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.FaseAperturaId, faseAperturaId)
                .SetProperty(t => t.FaseClausuraId, faseClausuraId));
    }

    public async Task LimpiarReferenciasFaseTablaAnualSiCoinciden(int torneoId, int faseId)
    {
        await Context.Set<Torneo>()
            .Where(t => t.Id == torneoId && t.FaseAperturaId == faseId)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.FaseAperturaId, (int?)null));
        await Context.Set<Torneo>()
            .Where(t => t.Id == torneoId && t.FaseClausuraId == faseId)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.FaseClausuraId, (int?)null));
    }

    public async Task<Torneo?> ObtenerPorIdConCategoriasAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Torneo>()
            .AsNoTracking()
            .Include(t => t.Categorias)
            .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);
    }
} 