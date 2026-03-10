using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

/// <summary>
/// Core base para entidades que siempre dependen de un padre.
/// El repositorio debe implementar IRepositorioABMAnidado para ListarPorPadre y ObtenerPorIdYPadre.
/// </summary>
public abstract class ABMCoreAnidado<TRepo, TEntidad, TDTO, TPadreId> : ICoreABMAnidado<TPadreId, TDTO>
    where TRepo : IRepositorioABMAnidado<TEntidad, TPadreId>
    where TEntidad : Entidad
    where TDTO : DTO
{
    protected readonly IBDVirtual BDVirtual;
    protected readonly TRepo Repo;
    protected readonly IMapper Mapper;

    protected ABMCoreAnidado(IBDVirtual bd, TRepo repo, IMapper mapper)
    {
        BDVirtual = bd;
        Repo = repo;
        Mapper = mapper;
    }

    public virtual async Task<IEnumerable<TDTO>> ListarPorPadre(TPadreId padreId)
    {
        var entidades = await Repo.ListarPorPadre(padreId);
        return Mapper.Map<List<TDTO>>(entidades);
    }

    public virtual async Task<int> Crear(TPadreId padreId, TDTO dto)
    {
        var entidad = Mapper.Map<TEntidad>(dto);
        entidad = await AntesDeCrear(padreId, dto, entidad);
        Repo.Crear(entidad);
        await BDVirtual.GuardarCambios();
        return entidad.Id;
    }

    protected abstract Task<TEntidad> AntesDeCrear(TPadreId padreId, TDTO dto, TEntidad entidad);

    public virtual async Task<TDTO?> ObtenerPorId(TPadreId padreId, int id)
    {
        var entidad = await Repo.ObtenerPorIdYPadre(padreId, id);
        if (entidad == null)
            return default;

        var dto = Mapper.Map<TDTO>(entidad);
        dto = AntesDeObtenerPorId(entidad, dto);
        return dto;
    }

    protected virtual TDTO AntesDeObtenerPorId(TEntidad entidad, TDTO dto)
    {
        return dto;
    }

    public virtual async Task<int> Modificar(TPadreId padreId, int id, TDTO nuevo)
    {
        var entidadAnterior = await Repo.ObtenerPorIdYPadre(padreId, id);
        if (entidadAnterior == null)
            throw new ExcepcionControlada("No existe la entidad a modificar o no pertenece al recurso padre indicado.");

        var entidadNueva = Mapper.Map<TEntidad>(nuevo);
        if (entidadNueva == null)
            throw new ExcepcionControlada("Hubo un problema mapeando la entidad");

        await AntesDeModificar(padreId, id, nuevo, entidadAnterior, entidadNueva);

        Repo.Modificar(entidadAnterior, entidadNueva);
        await BDVirtual.GuardarCambios();
        return id;
    }

    protected virtual Task AntesDeModificar(TPadreId padreId, int id, TDTO dto, TEntidad entidadAnterior, TEntidad entidadNueva)
    {
        return Task.CompletedTask;
    }

    public virtual async Task<int> Eliminar(TPadreId padreId, int id)
    {
        var entidad = await Repo.ObtenerPorIdYPadre(padreId, id);
        if (entidad == null)
            return -1;

        await AntesDeEliminar(padreId, id, entidad);
        Repo.Eliminar(entidad);
        await BDVirtual.GuardarCambios();
        return id;
    }

    protected virtual Task AntesDeEliminar(TPadreId padreId, int id, TEntidad entidad)
    {
        return Task.CompletedTask;
    }
}
