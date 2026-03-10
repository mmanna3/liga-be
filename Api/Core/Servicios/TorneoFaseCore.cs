using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class TorneoFaseCore : ABMCoreAnidado<ITorneoFaseRepo, TorneoFase, TorneoFaseDTO, int>, ITorneoFaseCore
{
    private readonly ITorneoRepo _torneoRepo;

    public TorneoFaseCore(IBDVirtual bd, ITorneoFaseRepo repo, ITorneoRepo torneoRepo, IMapper mapper)
        : base(bd, repo, mapper)
    {
        _torneoRepo = torneoRepo;
    }

    protected override async Task<TorneoFase> AntesDeCrear(int padreId, TorneoFaseDTO dto, TorneoFase entidad)
    {
        var torneo = await _torneoRepo.ObtenerPorId(padreId);
        if (torneo == null)
            throw new ExcepcionControlada("El torneo indicado no existe.");

        ValidarInstanciaEliminacionDirecta(dto);

        entidad.TorneoId = padreId;
        return entidad;
    }

    protected override Task AntesDeModificar(int padreId, int id, TorneoFaseDTO dto, TorneoFase entidadAnterior, TorneoFase entidadNueva)
    {
        ValidarInstanciaEliminacionDirecta(dto);

        entidadNueva.TorneoId = padreId;
        return Task.CompletedTask;
    }

    private static void ValidarInstanciaEliminacionDirecta(TorneoFaseDTO dto)
    {
        if (dto.FaseFormatoId == (int)FormatoDeLaFaseEnum.TodosContraTodos && dto.InstanciaEliminacionDirectaId.HasValue)
            throw new ExcepcionControlada("La instancia de eliminación directa solo aplica cuando el formato es eliminación directa.");
    }
}
