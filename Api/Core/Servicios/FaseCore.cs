using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class FaseCore : ABMCoreAnidado<IFaseRepo, Fase, FaseDTO, int>, IFaseCore
{
    private readonly ITorneoRepo _torneoRepo;

    public FaseCore(IBDVirtual bd, IFaseRepo repo, ITorneoRepo torneoRepo, IMapper mapper)
        : base(bd, repo, mapper)
    {
        _torneoRepo = torneoRepo;
    }

    public override async Task<int> Crear(int padreId, FaseDTO dto)
    {
        var entidad = CrearEntidadDesdeDto(dto);
        entidad = await AntesDeCrear(padreId, dto, entidad);
        Repo.Crear(entidad);
        await BDVirtual.GuardarCambios();
        return entidad.Id;
    }

    public override async Task<int> Modificar(int padreId, int id, FaseDTO nuevo)
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

    private static Fase CrearEntidadDesdeDto(FaseDTO dto, int id = 0)
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
                EsVisibleEnApp = dto.EsVisibleEnApp
            },
            _ => throw new ExcepcionControlada("Tipo de fase no válido.")
        };
    }

    protected override async Task<Fase> AntesDeCrear(int padreId, FaseDTO dto, Fase entidad)
    {
        var torneo = await _torneoRepo.ObtenerPorId(padreId);
        if (torneo == null)
            throw new ExcepcionControlada("El torneo indicado no existe.");

        entidad.TorneoId = padreId;
        return entidad;
    }

    protected override Task AntesDeModificar(int padreId, int id, FaseDTO dto, Fase entidadAnterior, Fase entidadNueva)
    {
        entidadNueva.TorneoId = padreId;
        return Task.CompletedTask;
    }

    public async Task CambiarVisibilidadEnApp(int torneoId, int faseId, bool esVisibleEnApp)
    {
        var filas = await Repo.ActualizarEsVisibleEnApp(torneoId, faseId, esVisibleEnApp);
        if (filas == 0)
            throw new ExcepcionControlada("No existe la fase a modificar o no pertenece al torneo indicado.");
    }
}
