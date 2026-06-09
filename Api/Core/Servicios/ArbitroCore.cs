using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using Api.Persistencia._Config;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Servicios;

public class ArbitroCore : ABMCore<IArbitroRepo, Arbitro, ArbitroDTO>, IArbitroCore
{
    private readonly AppDbContext _context;

    public ArbitroCore(IBDVirtual bd, IArbitroRepo repo, IMapper mapper, AppDbContext context) : base(bd, repo, mapper)
    {
        _context = context;
    }

    protected override async Task<Arbitro> AntesDeCrear(ArbitroDTO dto, Arbitro entidad)
    {
        var ids = await ValidarYObtenerTorneoAgrupadorIds(dto.TorneoAgrupadorIds);
        entidad.ArbitroTorneoAgrupadores = ConstruirAgrupadores(ids);
        return entidad;
    }

    protected override async Task<Arbitro> AntesDeModificar(int id, ArbitroDTO dto, Arbitro entidadAnterior, Arbitro entidadNueva)
    {
        var ids = await ValidarYObtenerTorneoAgrupadorIds(dto.TorneoAgrupadorIds);
        await Repo.EliminarAgrupadoresDelArbitro(id);
        entidadNueva.ArbitroTorneoAgrupadores = ConstruirAgrupadores(ids);
        entidadNueva.Id = id;
        return entidadNueva;
    }

    private async Task<List<int>> ValidarYObtenerTorneoAgrupadorIds(List<int>? torneoAgrupadorIds)
    {
        var ids = (torneoAgrupadorIds ?? []).Distinct().ToList();
        if (ids.Count == 0)
            return ids;

        var existentes = await _context.TorneoAgrupadores
            .Where(t => ids.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync();
        var inexistentes = ids.Except(existentes).ToList();
        if (inexistentes.Count > 0)
            throw new ExcepcionControlada("Uno o más agrupadores de torneo no existen en el sistema.");

        return ids;
    }

    private static List<ArbitroTorneoAgrupador> ConstruirAgrupadores(IEnumerable<int> torneoAgrupadorIds)
    {
        return torneoAgrupadorIds.Select(torneoAgrupadorId => new ArbitroTorneoAgrupador
        {
            Id = 0,
            TorneoAgrupadorId = torneoAgrupadorId
        }).ToList();
    }
}
