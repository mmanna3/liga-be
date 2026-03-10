using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class TorneoCategoriaCore : ABMCoreAnidado<ITorneoCategoriaRepo, TorneoCategoria, TorneoCategoriaDTO, int>, ITorneoCategoriaCore
{
    private readonly ITorneoRepo _torneoRepo;

    public TorneoCategoriaCore(IBDVirtual bd, ITorneoCategoriaRepo repo, ITorneoRepo torneoRepo, IMapper mapper)
        : base(bd, repo, mapper)
    {
        _torneoRepo = torneoRepo;
    }

    protected override async Task<TorneoCategoria> AntesDeCrear(int padreId, TorneoCategoriaDTO dto, TorneoCategoria entidad)
    {
        var torneo = await _torneoRepo.ObtenerPorId(padreId);
        if (torneo == null)
            throw new ExcepcionControlada("El torneo indicado no existe.");

        if (dto.AnioDesde > dto.AnioHasta)
            throw new ExcepcionControlada("El año desde no puede ser mayor que el año hasta.");

        entidad.TorneoId = padreId;
        return entidad;
    }

    protected override Task AntesDeModificar(int padreId, int id, TorneoCategoriaDTO dto, TorneoCategoria entidadAnterior, TorneoCategoria entidadNueva)
    {
        if (dto.AnioDesde > dto.AnioHasta)
            throw new ExcepcionControlada("El año desde no puede ser mayor que el año hasta.");

        entidadNueva.TorneoId = padreId;
        return Task.CompletedTask;
    }
}
