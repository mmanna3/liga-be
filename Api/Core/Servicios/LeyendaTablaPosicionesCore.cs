using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class LeyendaTablaPosicionesCore
    : ABMCoreAnidado<ILeyendaTablaPosicionesRepo, LeyendaTablaPosiciones, LeyendaTablaPosicionesDTO, int>,
        ILeyendaTablaPosicionesCore
{
    private readonly IZonaRepo _zonaRepo;
    private readonly IFaseCategoriaRepo _faseCategoriaRepo;
    private readonly IEquipoRepo _equipoRepo;
    private readonly IFechaRepo _fechaRepo;

    private const string MensajeDuplicada =
        "Ya existe una leyenda con la misma zona, categoría y equipo (o ya hay una leyenda general sin equipo para esa categoría).";

    public LeyendaTablaPosicionesCore(
        IBDVirtual bd,
        ILeyendaTablaPosicionesRepo repo,
        IZonaRepo zonaRepo,
        IFaseCategoriaRepo faseCategoriaRepo,
        IEquipoRepo equipoRepo,
        IFechaRepo fechaRepo,
        IMapper mapper)
        : base(bd, repo, mapper)
    {
        _zonaRepo = zonaRepo;
        _faseCategoriaRepo = faseCategoriaRepo;
        _equipoRepo = equipoRepo;
        _fechaRepo = fechaRepo;
    }

    protected override async Task<LeyendaTablaPosiciones> AntesDeCrear(int padreId, LeyendaTablaPosicionesDTO dto,
        LeyendaTablaPosiciones entidad)
    {
        await ValidarZonaYCategoria(padreId, dto.CategoriaId);
        if (dto.EsDeTablaAnual)
            await ValidarLeyendaDeTablaAnual(padreId, dto);
        else
            await ValidarEquipoYQuita(padreId, dto.EquipoId, dto.QuitaDePuntos);

        if (await Repo.ExisteOtraConMismaZonaCategoriaYEquipo(padreId, dto.CategoriaId, dto.EquipoId,
                dto.EsDeTablaAnual, null))
            throw new ExcepcionControlada(MensajeDuplicada);

        entidad.ZonaId = padreId;
        return entidad;
    }

    protected override async Task AntesDeModificar(int padreId, int id, LeyendaTablaPosicionesDTO dto,
        LeyendaTablaPosiciones entidadAnterior, LeyendaTablaPosiciones entidadNueva)
    {
        await ValidarZonaYCategoria(padreId, dto.CategoriaId);
        if (dto.EsDeTablaAnual)
            await ValidarLeyendaDeTablaAnual(padreId, dto);
        else
            await ValidarEquipoYQuita(padreId, dto.EquipoId, dto.QuitaDePuntos);

        if (await Repo.ExisteOtraConMismaZonaCategoriaYEquipo(padreId, dto.CategoriaId, dto.EquipoId,
                dto.EsDeTablaAnual, id))
            throw new ExcepcionControlada(MensajeDuplicada);

        entidadNueva.ZonaId = padreId;
    }

    /// <summary>
    /// Las leyendas de la tabla Anual van en la zona de Apertura que tiene una zona homónima en Clausura, y son solo
    /// texto: la tabla Anual ya hereda las quitas de puntos de Apertura y Clausura.
    /// </summary>
    private async Task ValidarLeyendaDeTablaAnual(int zonaId, LeyendaTablaPosicionesDTO dto)
    {
        if (dto.EquipoId.HasValue || dto.QuitaDePuntos != 0)
            throw new ExcepcionControlada("Las leyendas de la tabla Anual no pueden quitar puntos.");

        var par = await _fechaRepo.ObtenerIdsZonasAnualPorZonaReferenciaAsync(zonaId);
        if (par == null || par.Value.ZonaAperturaId != zonaId)
            throw new ExcepcionControlada(
                "Las leyendas de la tabla Anual se cargan en una zona de la fase Apertura que tenga una zona con el mismo nombre en la fase Clausura.");
    }

    private async Task ValidarEquipoYQuita(int zonaId, int? equipoId, int quitaDePuntos)
    {
        if (!equipoId.HasValue)
        {
            if (quitaDePuntos != 0)
                throw new ExcepcionControlada(
                    "Sin equipo asociado, la quita de puntos debe ser cero.");
            return;
        }

        if (quitaDePuntos <= 0)
            throw new ExcepcionControlada("Si se indica un equipo, la quita de puntos debe ser mayor que cero.");

        if (!await _equipoRepo.EquipoPerteneceAZonaAsync(equipoId.Value, zonaId))
            throw new ExcepcionControlada("El equipo no está asignado a esta zona.");
    }

    private async Task ValidarZonaYCategoria(int zonaId, int? categoriaId)
    {
        var zona = await _zonaRepo.ObtenerPorId(zonaId);
        if (zona == null)
            throw new ExcepcionControlada("La zona no existe.");

        if (!categoriaId.HasValue)
            return;

        var categoria = await _faseCategoriaRepo.ObtenerPorId(categoriaId.Value);
        if (categoria == null)
            throw new ExcepcionControlada("La categoría no existe.");

        if (categoria.FaseId != zona.FaseId)
            throw new ExcepcionControlada("La categoría debe pertenecer a la misma fase que la zona.");
    }
}
