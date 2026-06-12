using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class FaseCategoriaCore : ABMCoreAnidado<IFaseCategoriaRepo, FaseCategoria, FaseCategoriaDTO, int>, IFaseCategoriaCore
{
    private readonly IFaseRepo _faseRepo;
    private readonly ITorneoCategoriaRepo _torneoCategoriaRepo;
    private readonly ITorneoRepo _torneoRepo;

    public FaseCategoriaCore(
        IBDVirtual bd,
        IFaseCategoriaRepo repo,
        IFaseRepo faseRepo,
        ITorneoCategoriaRepo torneoCategoriaRepo,
        ITorneoRepo torneoRepo,
        IMapper mapper)
        : base(bd, repo, mapper)
    {
        _faseRepo = faseRepo;
        _torneoCategoriaRepo = torneoCategoriaRepo;
        _torneoRepo = torneoRepo;
    }

    protected override async Task<FaseCategoria> AntesDeCrear(int padreId, FaseCategoriaDTO dto, FaseCategoria entidad)
    {
        await AsegurarFaseExiste(padreId);
        CategoriaPayloadValidador.ValidarAnios(dto.AnioDesde, dto.AnioHasta);
        CategoriaPayloadValidador.ValidarOrdenesUnicos([dto.Orden]);

        var existentes = (await Repo.ListarPorPadre(padreId)).ToList();
        if (existentes.Any(c => c.Orden == dto.Orden))
            throw new ExcepcionControlada("Ya existe una categoría con ese orden en la fase.");

        entidad.FaseId = padreId;
        return entidad;
    }

    protected override async Task AntesDeModificar(int padreId, int id, FaseCategoriaDTO dto, FaseCategoria entidadAnterior, FaseCategoria entidadNueva)
    {
        CategoriaPayloadValidador.ValidarAnios(dto.AnioDesde, dto.AnioHasta);
        CategoriaPayloadValidador.ValidarOrdenesUnicos([dto.Orden]);

        var demas = (await Repo.ListarPorPadre(padreId)).Where(c => c.Id != id).ToList();
        if (demas.Any(c => c.Orden == dto.Orden))
            throw new ExcepcionControlada("Ya existe otra categoría con ese orden en la fase.");

        entidadNueva.FaseId = padreId;
    }

    protected override async Task AntesDeEliminar(int padreId, int id, FaseCategoria entidad)
    {
        if (await Repo.AlgunaTienePartidosOZonasOLeyendas([id]))
            throw new ExcepcionControlada("No se pueden eliminar categorías con partidos, zonas o leyendas asociadas.");
    }

    public async Task ReemplazarCategorias(int faseId, List<FaseCategoriaDTO> categoriasDto, int? torneoIdParaValidarAnual = null)
    {
        CategoriaPayloadValidador.ExigirAlMenosUna(categoriasDto);
        CategoriaPayloadValidador.ValidarOrdenesUnicos(categoriasDto.Select(c => c.Orden).ToList());

        var categoriasExistentes = await Repo.ListarPorPadreOrdenadasParaEditar(faseId);
        var porId = categoriasExistentes.ToDictionary(c => c.Id);

        var idsEnPayload = categoriasDto.Where(d => d.Id > 0).Select(d => d.Id).ToHashSet();
        var idsAEliminar = categoriasExistentes.Select(c => c.Id).Where(id => !idsEnPayload.Contains(id)).ToList();

        if (idsAEliminar.Count > 0 &&
            await Repo.AlgunaTienePartidosOZonasOLeyendas(idsAEliminar))
            throw new ExcepcionControlada("No se pueden eliminar categorías con partidos, zonas o leyendas asociadas.");

        foreach (var id in idsAEliminar)
        {
            Repo.Eliminar(porId[id]);
            porId.Remove(id);
        }

        await BDVirtual.GuardarCambios();

        foreach (var dto in categoriasDto.Where(d => d.Id > 0))
        {
            if (!porId.TryGetValue(dto.Id, out var existente))
                throw new ExcepcionControlada("Categoría no encontrada para esta fase.");
            existente.Orden = -existente.Id;
        }

        await BDVirtual.GuardarCambios();

        foreach (var dto in categoriasDto)
        {
            CategoriaPayloadValidador.ValidarAnios(dto.AnioDesde, dto.AnioHasta);

            if (dto.Id > 0)
            {
                if (!porId.TryGetValue(dto.Id, out var existente))
                    throw new ExcepcionControlada("Categoría no encontrada para esta fase.");

                existente.Nombre = dto.Nombre;
                existente.AnioDesde = dto.AnioDesde;
                existente.AnioHasta = dto.AnioHasta;
                existente.Orden = dto.Orden;
            }
            else
            {
                Repo.Crear(new FaseCategoria
                {
                    Id = 0,
                    FaseId = faseId,
                    Nombre = dto.Nombre,
                    AnioDesde = dto.AnioDesde,
                    AnioHasta = dto.AnioHasta,
                    Orden = dto.Orden
                });
            }
        }

        await BDVirtual.GuardarCambios();

        if (torneoIdParaValidarAnual.HasValue)
            await ValidarCategoriasAnualSiAplica(faseId, torneoIdParaValidarAnual.Value);
    }

    public async Task CopiarDesdePlantillaTorneo(int faseId, int torneoId)
    {
        var fase = await _faseRepo.ObtenerPorId(faseId);
        if (fase == null)
            throw new ExcepcionControlada("La fase indicada no existe.");

        var plantilla = (await _torneoCategoriaRepo.ListarPorPadre(torneoId)).ToList();
        if (plantilla.Count == 0)
            return;

        var dtos = plantilla.Select(c => new FaseCategoriaDTO
        {
            Id = 0,
            FaseId = faseId,
            Nombre = c.Nombre,
            AnioDesde = c.AnioDesde,
            AnioHasta = c.AnioHasta,
            Orden = c.Orden
        }).ToList();

        await ReemplazarCategorias(faseId, dtos, torneoId);
    }

    public async Task ValidarCategoriasAnualSiAplica(int faseId, int torneoId)
    {
        var torneo = await _torneoRepo.ObtenerPorId(torneoId);
        if (torneo?.FaseAperturaId is not { } idApertura || torneo.FaseClausuraId is not { } idClausura)
            return;

        if (faseId != idApertura && faseId != idClausura)
            return;

        var otraFaseId = faseId == idApertura ? idClausura : idApertura;
        var catsActuales = (await Repo.ListarPorPadre(faseId)).ToList();
        var catsOtra = (await Repo.ListarPorPadre(otraFaseId)).ToList();

        if (catsOtra.Count == 0)
            return;

        ValidadorCategoriasAnual.ValidarSetsIdenticos(
            faseId == idApertura ? catsActuales : catsOtra,
            faseId == idApertura ? catsOtra : catsActuales);
    }

    private async Task AsegurarFaseExiste(int faseId)
    {
        var fase = await _faseRepo.ObtenerPorId(faseId);
        if (fase == null)
            throw new ExcepcionControlada("La fase indicada no existe.");
    }
}
