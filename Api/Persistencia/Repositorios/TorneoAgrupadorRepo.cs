using Api.Core.DTOs.AppCarnetDigital;
using Api.Core.Entidades;
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
            .AsQueryable();
    }

    public async Task<IReadOnlyList<InformacionInicialAgrupadorDTO>> ListarInformacionInicialParaAppAsync(
        CancellationToken cancellationToken = default)
    {
        var agrupadores = await Context.Set<TorneoAgrupador>()
            .AsNoTracking()
            .Where(a => a.EsVisibleEnApp)
            .OrderBy(a => a.Nombre)
            .Include(a => a.Torneos.Where(t => t.EsVisibleEnApp))
            .ThenInclude(t => t.Fases.Where(f => f.EsVisibleEnApp))
            .ToListAsync(cancellationToken);

        var faseIds = agrupadores
            .SelectMany(a => a.Torneos)
            .SelectMany(t => t.Fases)
            .Select(f => f.Id)
            .Distinct()
            .ToList();

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
                Torneos = agr.Torneos
                    .OrderBy(t => t.Nombre)
                    .Select(t => new InformacionInicialTorneoDTO
                    {
                        Id = t.Id,
                        Nombre = t.Nombre,
                        Fases = t.Fases
                            .OrderBy(f => f.Numero)
                            .ThenBy(f => f.Nombre)
                            .Select(f => new InformacionInicialFaseDTO
                            {
                                Id = f.Id,
                                Nombre = f.Nombre,
                                Zonas = zonasPorFaseId.GetValueOrDefault(f.Id) ?? []
                            })
                            .ToList()
                    })
                    .ToList()
            };
            resultado.Add(dtoAgr);
        }

        return resultado;
    }
}
