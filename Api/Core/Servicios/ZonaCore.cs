using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class ZonaCore : ABMCoreAnidado<IZonaRepo, Zona, ZonaDTO, int>, IZonaCore
{
    private readonly IFaseRepo _torneoFaseRepo;
    private readonly IEquipoRepo _equipoRepo;

    public ZonaCore(IBDVirtual bd, IZonaRepo repo, IFaseRepo torneoFaseRepo,
        IEquipoRepo equipoRepo, IMapper mapper)
        : base(bd, repo, mapper)
    {
        _torneoFaseRepo = torneoFaseRepo;
        _equipoRepo = equipoRepo;
    }

    protected override async Task<Zona> AntesDeCrear(int padreId, ZonaDTO dto, Zona entidad)
    {
        var fase = await _torneoFaseRepo.ObtenerPorId(padreId);
        if (fase == null)
            throw new ExcepcionControlada("La fase indicada no existe.");

        if (fase is FaseTodosContraTodos && entidad is not ZonaTodosContraTodos)
            throw new ExcepcionControlada("En una fase todos contra todos solo se pueden crear zonas todos contra todos.");
        if (fase is FaseEliminacionDirecta && entidad is not ZonaEliminacionDirecta)
            throw new ExcepcionControlada("En una fase de eliminación directa solo se pueden crear zonas de eliminación directa.");
        if (fase is not FaseTodosContraTodos and not FaseEliminacionDirecta)
            throw new ExcepcionControlada("Tipo de fase no soportado para crear zonas.");

        entidad.FaseId = padreId;
        return entidad;
    }

    public override async Task<int> Crear(int padreId, ZonaDTO dto)
    {
        var fase = await _torneoFaseRepo.ObtenerPorId(padreId);
        if (fase == null)
            throw new ExcepcionControlada("La fase indicada no existe.");

        Zona entidad = fase switch
        {
            FaseTodosContraTodos => new ZonaTodosContraTodos
            {
                Id = 0,
                Nombre = dto.Nombre ?? string.Empty,
                FaseId = padreId
            },
            FaseEliminacionDirecta => new ZonaEliminacionDirecta
            {
                Id = 0,
                Nombre = dto.Nombre ?? string.Empty,
                FaseId = padreId,
                CategoriaId = dto.CategoriaId
                    ?? throw new ExcepcionControlada("La categoría es obligatoria para zonas de eliminación directa.")
            },
            _ => throw new ExcepcionControlada("Tipo de fase no soportado para crear zonas.")
        };

        entidad = await AntesDeCrear(padreId, dto, entidad);
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

    protected override Task AntesDeModificar(int padreId, int id, ZonaDTO dto, Zona entidadAnterior, Zona entidadNueva)
    {
        entidadNueva.FaseId = padreId;
        return Task.CompletedTask;
    }

    public override async Task<int> Modificar(int padreId, int id, ZonaDTO dto)
    {
        var entidadAnterior = await Repo.ObtenerPorIdYPadre(padreId, id);
        if (entidadAnterior == null)
            throw new ExcepcionControlada("No existe la entidad a modificar o no pertenece al recurso padre indicado.");

        Zona entidadNueva = entidadAnterior switch
        {
            ZonaTodosContraTodos => new ZonaTodosContraTodos
            {
                Id = id,
                Nombre = dto.Nombre ?? string.Empty,
                FaseId = padreId
            },
            ZonaEliminacionDirecta ed => new ZonaEliminacionDirecta
            {
                Id = id,
                Nombre = dto.Nombre ?? string.Empty,
                FaseId = padreId,
                CategoriaId = dto.CategoriaId ?? ed.CategoriaId
            },
            _ => throw new ExcepcionControlada("Tipo de zona no soportado para modificar.")
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

    public async Task<IEnumerable<ZonaDTO>> CrearMasivamente(int padreId, IEnumerable<ZonaDTO> dtos)
    {
        var creados = new List<ZonaDTO>();
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

    public async Task ModificarMasivamente(int padreId, IEnumerable<ZonaDTO> dtos)
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