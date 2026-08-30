using Api.Core.DTOs;
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

    /// <summary>
    /// Listado liviano: proyección plana con <c>AsNoTracking</c>. Trae fases y
    /// grupos resumidos (sin zonas / fechas / categorías / estado), y no toca
    /// <c>FaseApertura</c> / <c>FaseClausura</c> ni las categorías del torneo.
    /// </summary>
    public async Task<List<TorneoListadoDTO>> ListarParaListado(int? anio, int? torneoAgrupadorId)
    {
        var query = Context.Set<Torneo>().AsNoTracking();

        if (anio.HasValue)
            query = query.Where(t => t.Anio == anio.Value);
        if (torneoAgrupadorId.HasValue)
            query = query.Where(t => t.TorneoAgrupadorId == torneoAgrupadorId.Value);

        return await query
            .OrderBy(t => t.Nombre)
            .Select(t => new TorneoListadoDTO
            {
                Id = t.Id,
                Nombre = t.Nombre,
                Anio = t.Anio,
                EsVisibleEnApp = t.EsVisibleEnApp,
                HorarioDeJuego = t.HorarioDeJuego,
                TorneoAgrupadorId = t.TorneoAgrupadorId,
                TorneoAgrupadorNombre = t.TorneoAgrupador != null ? t.TorneoAgrupador.Nombre : string.Empty,
                Fases = t.Fases
                    .OrderBy(f => f.Numero)
                    .Select(f => new FaseResumenDTO
                    {
                        Id = f.Id,
                        Numero = f.Numero,
                        Nombre = f.Nombre,
                        GrupoDeFasesId = f.GrupoDeFasesId,
                        TipoDeFase = f is FaseEliminacionDirecta
                            ? TipoDeFaseEnum.EliminacionDirecta
                            : TipoDeFaseEnum.TodosContraTodos,
                        TipoDeFaseNombre = f is FaseEliminacionDirecta
                            ? "Eliminación directa"
                            : "Todos contra todos",
                        SePuedeEditar = f is FaseEliminacionDirecta
                            || !Context.Set<Zona>().Any(z => z.FaseId == f.Id)
                    })
                    .ToList(),
                GruposDeFases = t.GruposDeFases
                    .OrderBy(g => g.Numero)
                    .Select(g => new GrupoDeFasesResumenDTO
                    {
                        Id = g.Id,
                        Numero = g.Numero,
                        Nombre = g.Nombre,
                        GrupoDeFasesPadreId = g.GrupoDeFasesPadreId
                    })
                    .ToList()
            })
            .ToListAsync();
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
            .Include(x => x.Fases)
                .ThenInclude(f => f.Categorias)
            .Include(x => x.GruposDeFases)
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
            Nombre = "Zona única",
            Orden = 1
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