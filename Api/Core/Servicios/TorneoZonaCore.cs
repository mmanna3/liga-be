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
    private readonly IEquipoRepo _equipoRepo;

    public TorneoZonaCore(IBDVirtual bd, ITorneoZonaRepo repo, ITorneoFaseRepo torneoFaseRepo,
        IEquipoRepo equipoRepo, IMapper mapper)
        : base(bd, repo, mapper)
    {
        _torneoFaseRepo = torneoFaseRepo;
        _equipoRepo = equipoRepo;
    }

    protected override async Task<TorneoZona> AntesDeCrear(int padreId, TorneoZonaDTO dto, TorneoZona entidad)
    {
        var fase = await _torneoFaseRepo.ObtenerPorId(padreId);
        if (fase == null)
            throw new ExcepcionControlada("La fase indicada no existe.");
        if (fase is not FaseTodosContraTodos)
            throw new ExcepcionControlada("Solo las fases todos contra todos admiten zonas.");

        entidad.TorneoFaseId = padreId;
        return entidad;
    }

    public override async Task<int> Crear(int padreId, TorneoZonaDTO dto)
    {
        var entidad = new ZonaTodosContraTodos
        {
            Id = 0,
            Nombre = dto.Nombre ?? string.Empty,
            TorneoFaseId = padreId
        };
        entidad = (ZonaTodosContraTodos)await AntesDeCrear(padreId, dto, entidad);
        Repo.Crear(entidad);
        await BDVirtual.GuardarCambios();
        var id = entidad.Id;

        if (dto.Equipos != null)
        {
            var equipoIds = ParsearEquipoIds(dto.Equipos);
            await AplicarEquiposEnZona(id, padreId, equipoIds);
            await BDVirtual.GuardarCambios();
        }

        return id;
    }

    protected override Task AntesDeModificar(int padreId, int id, TorneoZonaDTO dto, TorneoZona entidadAnterior, TorneoZona entidadNueva)
    {
        entidadNueva.TorneoFaseId = padreId;
        return Task.CompletedTask;
    }

    public override async Task<int> Modificar(int padreId, int id, TorneoZonaDTO dto)
    {
        var entidadAnterior = await Repo.ObtenerPorIdYPadre(padreId, id);
        if (entidadAnterior == null)
            throw new ExcepcionControlada("No existe la entidad a modificar o no pertenece al recurso padre indicado.");
        if (entidadAnterior is not ZonaTodosContraTodos)
            throw new ExcepcionControlada("Solo se pueden modificar zonas todos contra todos.");

        var entidadNueva = new ZonaTodosContraTodos
        {
            Id = id,
            Nombre = dto.Nombre ?? string.Empty,
            TorneoFaseId = padreId
        };
        await AntesDeModificar(padreId, id, dto, entidadAnterior, entidadNueva);
        Repo.Modificar(entidadAnterior, entidadNueva);
        await BDVirtual.GuardarCambios();

        if (dto.Equipos != null)
        {
            var equipoIds = ParsearEquipoIds(dto.Equipos);
            await AplicarEquiposEnZona(id, padreId, equipoIds);
            await BDVirtual.GuardarCambios();
        }

        return id;
    }

    public async Task<IEnumerable<TorneoZonaDTO>> CrearMasivamente(int padreId, IEnumerable<TorneoZonaDTO> dtos)
    {
        var creados = new List<TorneoZonaDTO>();
        foreach (var dto in dtos)
        {
            var id = await Crear(padreId, dto);
            var creado = await ObtenerPorId(padreId, id);
            if (creado != null)
                creados.Add(creado);
        }
        return creados;
    }

    public override async Task<int> Eliminar(int padreId, int id)
    {
        var entidad = await Repo.ObtenerPorIdYPadreParaEliminar(padreId, id);
        if (entidad == null)
            return -1;

        await AntesDeEliminar(padreId, id, entidad);
        await AplicarEquiposEnZona(id, padreId, []);
        await BDVirtual.GuardarCambios();
        Repo.Eliminar(entidad);
        await BDVirtual.GuardarCambios();
        return id;
    }

    public async Task ModificarMasivamente(int padreId, IEnumerable<TorneoZonaDTO> dtos)
    {
        var list = dtos?.ToList() ?? [];
        var idsEnRequest = list.Where(d => d.Id > 0).Select(d => d.Id).ToHashSet();

        var idsExistentes = (await Repo.ListarIdsPorPadre(padreId)).ToList();
        var idsAEliminar = list.Count == 0
            ? idsExistentes
            : idsExistentes.Where(id => !idsEnRequest.Contains(id)).ToList();

        foreach (var id in idsAEliminar)
        {
            await Eliminar(padreId, id);
        }

        foreach (var dto in list)
        {
            if (dto.Id <= 0)
                await Crear(padreId, dto);
            else
                await Modificar(padreId, dto.Id, dto);
        }
    }

    private async Task AplicarEquiposEnZona(int zonaId, int faseId, IReadOnlyList<int> equipoIds)
    {
        await _equipoRepo.QuitarEquiposDeZona(zonaId);
        await BDVirtual.GuardarCambios();
        if (equipoIds.Count > 0)
            await _equipoRepo.AsignarEquiposAZona(zonaId, equipoIds);
    }

    private static List<int> ParsearEquipoIds(List<EquipoDeLaZonaDTO> equipos)
    {
        var ids = new List<int>();
        foreach (var e in equipos)
        {
            if (int.TryParse(e.Id, out var equipoId))
                ids.Add(equipoId);
        }
        return ids;
    }
}