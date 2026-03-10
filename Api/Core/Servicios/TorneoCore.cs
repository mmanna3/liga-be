using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class TorneoCore : ABMCore<ITorneoRepo, Torneo, TorneoDTO>, ITorneoCore
{
    public TorneoCore(IBDVirtual bd, ITorneoRepo repo, IMapper mapper) : base(bd, repo, mapper)
    {
    }

    public override async Task<int> Crear(TorneoDTO dto)
    {
        var id = await base.Crear(dto);
        await Repo.CrearFaseUnicaYZonaUnica(id);
        return id;
    }
} 