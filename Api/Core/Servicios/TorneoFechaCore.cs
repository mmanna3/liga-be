using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class TorneoFechaCore : ABMCoreAnidado<ITorneoFechaRepo, TorneoFecha, TorneoFechaDTO, int>, ITorneoFechaCore
{
    private readonly ITorneoZonaRepo _torneoZonaRepo;

    public TorneoFechaCore(IBDVirtual bd, ITorneoFechaRepo repo, ITorneoZonaRepo torneoZonaRepo, IMapper mapper)
        : base(bd, repo, mapper)
    {
        _torneoZonaRepo = torneoZonaRepo;
    }

    protected override async Task<TorneoFecha> AntesDeCrear(int padreId, TorneoFechaDTO dto, TorneoFecha entidad)
    {
        var zona = await _torneoZonaRepo.ObtenerPorId(padreId);
        if (zona == null)
            throw new ExcepcionControlada("La zona indicada no existe.");

        entidad.ZonaId = padreId;
        return entidad;
    }

    protected override Task AntesDeModificar(int padreId, int id, TorneoFechaDTO dto, TorneoFecha entidadAnterior, TorneoFecha entidadNueva)
    {
        entidadNueva.ZonaId = padreId;
        return Task.CompletedTask;
    }
}
