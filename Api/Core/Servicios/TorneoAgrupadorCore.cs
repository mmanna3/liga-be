using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class TorneoAgrupadorCore : ABMCore<ITorneoAgrupadorRepo, TorneoAgrupador, TorneoAgrupadorDTO>, ITorneoAgrupadorCore
{
    public TorneoAgrupadorCore(IBDVirtual bd, ITorneoAgrupadorRepo repo, IMapper mapper) : base(bd, repo, mapper)
    {
    }

    protected override async Task AntesDeEliminar(int id, TorneoAgrupador entidad)
    {
        if (entidad.Torneos?.Count > 0)
            throw new ExcepcionControlada("No se puede eliminar un agrupador que tiene torneos asociados.");
        await Task.CompletedTask;
    }
}
