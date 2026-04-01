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

    public override async Task<int> Crear(int padreId, TorneoFaseDTO dto)
    {
        var entidad = CrearEntidadDesdeDto(dto);
        entidad = await AntesDeCrear(padreId, dto, entidad);
        Repo.Crear(entidad);
        await BDVirtual.GuardarCambios();
        return entidad.Id;
    }

    public override async Task<int> Modificar(int padreId, int id, TorneoFaseDTO nuevo)
    {
        var entidadAnterior = await Repo.ObtenerPorIdYPadre(padreId, id);
        if (entidadAnterior == null)
            throw new ExcepcionControlada("No existe la entidad a modificar o no pertenece al recurso padre indicado.");

        var tipoActual = entidadAnterior switch
        {
            FaseTodosContraTodos => TipoDeFaseEnum.TodosContraTodos,
            FaseEliminacionDirecta => TipoDeFaseEnum.EliminacionDirecta,
            _ => throw new ExcepcionControlada("Tipo de fase no reconocido.")
        };

        if (tipoActual != nuevo.TipoDeFase)
        {
            var tieneZonas = entidadAnterior switch
            {
                FaseTodosContraTodos f => f.Zonas?.Any() == true,
                FaseEliminacionDirecta f => f.Zonas?.Any() == true,
                _ => false
            };
            if (tieneZonas)
                throw new ExcepcionControlada("No se puede cambiar el tipo de una fase que ya tiene zonas asignadas.");

            await Repo.CambiarTipo(padreId, id, nuevo.TipoDeFase);
            entidadAnterior = await Repo.ObtenerPorIdYPadre(padreId, id)
                ?? throw new ExcepcionControlada("No existe la entidad a modificar o no pertenece al recurso padre indicado.");
        }

        var entidadNueva = CrearEntidadDesdeDto(nuevo, id);
        await AntesDeModificar(padreId, id, nuevo, entidadAnterior, entidadNueva);

        Repo.Modificar(entidadAnterior, entidadNueva);
        await BDVirtual.GuardarCambios();
        return id;
    }

    public override async Task<int> Eliminar(int padreId, int id)
    {
        var entidad = await Repo.ObtenerPorIdYPadre(padreId, id);
        if (entidad == null)
            return -1;

        Repo.Eliminar(entidad);
        await BDVirtual.GuardarCambios();

        await Repo.DecrementarNumeroDeFasesPosteriores(padreId, entidad.Numero);
        return id;
    }

    private static TorneoFase CrearEntidadDesdeDto(TorneoFaseDTO dto, int id = 0)
    {
        return dto.TipoDeFase switch
        {
            TipoDeFaseEnum.TodosContraTodos => new FaseTodosContraTodos
            {
                Id = id,
                Nombre = dto.Nombre,
                Numero = dto.Numero,
                TorneoId = dto.TorneoId,
                EstadoFaseId = dto.EstadoFaseId,
                EsVisibleEnApp = dto.EsVisibleEnApp
            },
            TipoDeFaseEnum.EliminacionDirecta => new FaseEliminacionDirecta
            {
                Id = id,
                Nombre = dto.Nombre,
                Numero = dto.Numero,
                TorneoId = dto.TorneoId,
                EstadoFaseId = dto.EstadoFaseId,
                EsVisibleEnApp = dto.EsVisibleEnApp,
                InstanciaEliminacionDirectaId = dto.InstanciaEliminacionDirectaId
            },
            _ => throw new ExcepcionControlada("Tipo de fase no válido.")
        };
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
        if (dto.TipoDeFase == TipoDeFaseEnum.TodosContraTodos && dto.InstanciaEliminacionDirectaId.HasValue)
            throw new ExcepcionControlada("La instancia de eliminación directa solo aplica cuando el tipo de fase es eliminación directa.");
    }
}
