using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class TorneoZonaCore : ABMCoreAnidado<ITorneoZonaRepo, TorneoZona, TorneoZonaDTO, int>, ITorneoZonaCore
{
    private readonly ITorneoFaseRepo _torneoFaseRepo;

    public TorneoZonaCore(IBDVirtual bd, ITorneoZonaRepo repo, ITorneoFaseRepo torneoFaseRepo, IMapper mapper)
        : base(bd, repo, mapper)
    {
        _torneoFaseRepo = torneoFaseRepo;
    }

    protected override async Task<TorneoZona> AntesDeCrear(int padreId, TorneoZonaDTO dto, TorneoZona entidad)
    {
        var fase = await _torneoFaseRepo.ObtenerPorId(padreId);
        if (fase == null)
            throw new ExcepcionControlada("La fase indicada no existe.");

        entidad.TorneoFaseId = padreId;
        return entidad;
    }

    protected override Task AntesDeModificar(int padreId, int id, TorneoZonaDTO dto, TorneoZona entidadAnterior, TorneoZona entidadNueva)
    {
        entidadNueva.TorneoFaseId = padreId;
        return Task.CompletedTask;
    }
}