using Api.Core.DTOs.AppCarnetDigital;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class TorneoAgrupadorRepo : RepositorioABM<TorneoAgrupador>, ITorneoAgrupadorRepo
{
    public TorneoAgrupadorRepo(AppDbContext context) : base(context)
    {
    }

    protected override IQueryable<TorneoAgrupador> Set()
    {
        return Context.Set<TorneoAgrupador>()
            .Include(x => x.Torneos)
            .Include(x => x.Color)
            .AsQueryable();
    }

    public async Task<IReadOnlyList<InformacionInicialAgrupadorDTO>> ListarInformacionInicialParaAppAsync(
        CancellationToken cancellationToken = default)
    {
        var anioActual = DateTime.Today.Year;

        var agrupadores = await Context.Set<TorneoAgrupador>()
            .AsNoTracking()
            .Where(a => a.EsVisibleEnApp)
            .OrderBy(a => a.Nombre)
            .Include(a => a.Color)
            .Include(a => a.Torneos.Where(t => t.EsVisibleEnApp))
            .ThenInclude(t => t.Fases.Where(f => f.EsVisibleEnApp))
            .ToListAsync(cancellationToken);

        var faseIds = new HashSet<int>(
            agrupadores.SelectMany(a => a.Torneos).SelectMany(t => t.Fases).Select(f => f.Id));
        foreach (var t in agrupadores.SelectMany(a => a.Torneos))
        {
            if (t.FaseAperturaId.HasValue)
                faseIds.Add(t.FaseAperturaId.Value);
            if (t.FaseClausuraId.HasValue)
                faseIds.Add(t.FaseClausuraId.Value);
        }

        Dictionary<int, List<InformacionInicialZonaDTO>> zonasPorFaseId = new();
        if (faseIds.Count > 0)
        {
            var filasZona = await Context.Set<Zona>()
                .AsNoTracking()
                .Where(z => faseIds.Contains(z.FaseId))
                .OrderBy(z => z.Nombre)
                .Select(z => new { z.FaseId, z.Id, z.Nombre })
                .ToListAsync(cancellationToken);

            zonasPorFaseId = filasZona
                .GroupBy(x => x.FaseId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => new InformacionInicialZonaDTO { Id = x.Id, Nombre = x.Nombre }).ToList());
        }

        var resultado = new List<InformacionInicialAgrupadorDTO>();
        foreach (var agr in agrupadores)
        {
            var dtoAgr = new InformacionInicialAgrupadorDTO
            {
                Id = agr.Id,
                Nombre = agr.Nombre,
                Color = agr.Color != null ? agr.Color.Nombre : nameof(ColorEnum.Negro),
                Torneos = agr.Torneos
                    // Año calendario actual primero (A-Z); luego años futuros (por año); luego pasados (más reciente primero).
                    .OrderBy(t => t.Anio == anioActual ? 0 : t.Anio > anioActual ? 1 : 2)
                    .ThenBy(t => t.Anio == anioActual ? 0 : t.Anio > anioActual ? t.Anio : -t.Anio)
                    .ThenBy(t => t.Nombre, StringComparer.CurrentCultureIgnoreCase)
                    .Select(t =>
                    {
                        var fases = t.Fases
                            .OrderBy(f => f.Numero)
                            .ThenBy(f => f.Nombre)
                            .Select(f => new InformacionInicialFaseDTO
                            {
                                Id = f.Id,
                                Nombre = f.Nombre,
                                TipoDeFase = f switch
                                {
                                    FaseTodosContraTodos => nameof(TipoDeFaseEnum.TodosContraTodos),
                                    FaseEliminacionDirecta => nameof(TipoDeFaseEnum.EliminacionDirecta),
                                    _ => string.Empty
                                },
                                Zonas = zonasPorFaseId.GetValueOrDefault(f.Id) ?? []
                            })
                            .ToList();

                        if (t.FaseAperturaId is { } idApertura && t.FaseClausuraId is { } idClausura)
                        {
                            fases.Add(new InformacionInicialFaseDTO
                            {
                                Id = 0,
                                Nombre = "Anual",
                                TipoDeFase = "Anual",
                                Zonas = ZonasAnualCombinadas(
                                    zonasPorFaseId.GetValueOrDefault(idApertura),
                                    zonasPorFaseId.GetValueOrDefault(idClausura))
                            });
                        }

                        return new InformacionInicialTorneoDTO
                        {
                            Id = t.Id,
                            Nombre = t.Anio == anioActual ? t.Nombre : $"{t.Nombre} {t.Anio}",
                            Fases = fases
                        };
                    })
                    .ToList()
            };
            if (dtoAgr.Torneos.Count > 0)
                resultado.Add(dtoAgr);
        }

        return resultado;
    }

    private static List<InformacionInicialZonaDTO> ZonasAnualCombinadas(
        List<InformacionInicialZonaDTO>? deApertura,
        List<InformacionInicialZonaDTO>? deClausura)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var list = new List<InformacionInicialZonaDTO>();
        foreach (var z in (deApertura ?? []).Concat(deClausura ?? []))
        {
            if (seen.Add(z.Nombre.Trim()))
                list.Add(z);
        }
        return list;
    }
}
