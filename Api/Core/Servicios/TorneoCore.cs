using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class TorneoCore : ABMCore<ITorneoRepo, Torneo, TorneoDTO>, ITorneoCore
{
    private readonly IEquipoRepo _equipoRepo;

    public TorneoCore(IBDVirtual bd, ITorneoRepo repo, IMapper mapper, IEquipoRepo equipoRepo) : base(bd, repo, mapper)
    {
        _equipoRepo = equipoRepo;
    }

    protected override async Task AntesDeEliminar(int id, Torneo entidad)
    {
        await _equipoRepo.AnularTorneoEnEquipos(id);
        await Task.CompletedTask;
    }
} 