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

        if (dto.Orden < 1)
            throw new ExcepcionControlada("El orden de la categoría debe ser mayor o igual a 1.");

        var existentes = (await Repo.ListarPorPadre(padreId)).ToList();
        if (existentes.Any(c => c.Orden == dto.Orden))
            throw new ExcepcionControlada("Ya existe una categoría con ese orden en el torneo.");

        entidad.TorneoId = padreId;
        return entidad;
    }

    protected override async Task AntesDeModificar(int padreId, int id, TorneoCategoriaDTO dto, TorneoCategoria entidadAnterior, TorneoCategoria entidadNueva)
    {
        if (dto.AnioDesde > dto.AnioHasta)
            throw new ExcepcionControlada("El año desde no puede ser mayor que el año hasta.");

        if (dto.Orden < 1)
            throw new ExcepcionControlada("El orden de la categoría debe ser mayor o igual a 1.");

        var demas = (await Repo.ListarPorPadre(padreId)).Where(c => c.Id != id).ToList();
        if (demas.Any(c => c.Orden == dto.Orden))
            throw new ExcepcionControlada("Ya existe otra categoría con ese orden en el torneo.");

        entidadNueva.TorneoId = padreId;
    }
}
