using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class TorneoCore : ABMCore<ITorneoRepo, Torneo, TorneoDTO>, ITorneoCore
{
    private readonly IFaseRepo _torneoFaseRepo;
    private readonly ITorneoCategoriaRepo _torneoCategoriaRepo;
    private readonly IZonaRepo _torneoZonaRepo;

    public TorneoCore(IBDVirtual bd, ITorneoRepo repo, IFaseRepo torneoFaseRepo,
        ITorneoCategoriaRepo torneoCategoriaRepo, IZonaRepo torneoZonaRepo, IMapper mapper)
        : base(bd, repo, mapper)
    {
        _torneoFaseRepo = torneoFaseRepo;
        _torneoCategoriaRepo = torneoCategoriaRepo;
        _torneoZonaRepo = torneoZonaRepo;
    }

    protected override async Task<Torneo> AntesDeCrear(TorneoDTO dto, Torneo entidad)
    {
        if (await Repo.ExisteTorneoConNombreAnioYAgrupador(entidad.Nombre, entidad.Anio, entidad.TorneoAgrupadorId))
            throw new ExcepcionControlada("Ya existe un torneo con el mismo nombre, año y agrupador.");
        return entidad;
    }

    protected override async Task<Torneo> AntesDeModificar(int id, TorneoDTO dto, Torneo entidadAnterior, Torneo entidadNueva)
    {
        if (await Repo.ExisteTorneoConNombreAnioYAgrupador(entidadNueva.Nombre, entidadNueva.Anio, entidadNueva.TorneoAgrupadorId, id))
            throw new ExcepcionControlada("Ya existe un torneo con el mismo nombre, año y agrupador.");
        return entidadNueva;
    }

    public override async Task<int> Modificar(int id, TorneoDTO dto)
    {
        await base.Modificar(id, dto);

        if (dto.Fases != null)
        {
            await ReemplazarFases(id, dto.Fases);
        }

        if (dto.Categorias != null)
        {
            await ReemplazarCategorias(id, dto.Categorias);
        }

        return id;
    }

    private async Task ReemplazarFases(int torneoId, List<FaseDTO> fasesDto)
    {
        var fasesExistentes = await _torneoFaseRepo.ListarPorPadre(torneoId);
        foreach (var fase in fasesExistentes)
        {
            _torneoFaseRepo.Eliminar(fase);
        }
        await BDVirtual.GuardarCambios();

        foreach (var dto in fasesDto)
        {
            if (dto.TipoDeFase == TipoDeFaseEnum.TodosContraTodos && dto.InstanciaEliminacionDirectaId.HasValue)
                throw new ExcepcionControlada("La instancia de eliminación directa solo aplica cuando el tipo de fase es eliminación directa.");

            Fase fase = dto.TipoDeFase switch
            {
                TipoDeFaseEnum.TodosContraTodos => new FaseTodosContraTodos
                {
                    Id = 0,
                    TorneoId = torneoId,
                    Nombre = dto.Nombre ?? string.Empty,
                    Numero = dto.Numero,
                    EstadoFaseId = dto.EstadoFaseId,
                    EsVisibleEnApp = dto.EsVisibleEnApp
                },
                TipoDeFaseEnum.EliminacionDirecta => new FaseEliminacionDirecta
                {
                    Id = 0,
                    TorneoId = torneoId,
                    Nombre = dto.Nombre ?? string.Empty,
                    Numero = dto.Numero,
                    EstadoFaseId = dto.EstadoFaseId,
                    EsVisibleEnApp = dto.EsVisibleEnApp,
                    InstanciaEliminacionDirectaId = dto.InstanciaEliminacionDirectaId
                },
                _ => throw new ExcepcionControlada("Tipo de fase no válido.")
            };
            _torneoFaseRepo.Crear(fase);
            await BDVirtual.GuardarCambios();

            if (dto.TipoDeFase == TipoDeFaseEnum.TodosContraTodos)
            {
                var zona = new ZonaTodosContraTodos
                {
                    Id = 0,
                    FaseId = fase.Id,
                    Nombre = "Zona única"
                };
                _torneoZonaRepo.Crear(zona);
                await BDVirtual.GuardarCambios();
            }
        }
    }

    private async Task ReemplazarCategorias(int torneoId, List<TorneoCategoriaDTO> categoriasDto)
    {
        var categoriasExistentes = await _torneoCategoriaRepo.ListarPorPadre(torneoId);
        foreach (var cat in categoriasExistentes)
        {
            _torneoCategoriaRepo.Eliminar(cat);
        }
        await BDVirtual.GuardarCambios();

        foreach (var dto in categoriasDto)
        {
            if (dto.AnioDesde > dto.AnioHasta)
                throw new ExcepcionControlada("El año desde no puede ser mayor que el año hasta.");

            var categoria = new TorneoCategoria
            {
                Id = 0,
                TorneoId = torneoId,
                Nombre = dto.Nombre,
                AnioDesde = dto.AnioDesde,
                AnioHasta = dto.AnioHasta
            };
            _torneoCategoriaRepo.Crear(categoria);
            await BDVirtual.GuardarCambios();
        }
    }

    public override async Task<int> Crear(TorneoDTO dto)
    {
        var id = await base.Crear(dto);

        if (dto is CrearTorneoDTO crearDto)
        {
            if (crearDto.PrimeraFase != null)
            {
                await CrearFaseConDatos(id, crearDto.PrimeraFase);
            }
            else
            {
                await Repo.CrearFaseUnicaYZonaUnica(id);
            }

            if (crearDto.Categorias is { Count: > 0 })
            {
                foreach (var cat in crearDto.Categorias)
                {
                    if (cat.AnioDesde > cat.AnioHasta)
                        throw new ExcepcionControlada("El año desde no puede ser mayor que el año hasta.");
                    await CrearCategoria(id, cat);
                }
            }
        }
        else
        {
            await Repo.CrearFaseUnicaYZonaUnica(id);
        }

        return id;
    }

    private async Task CrearFaseConDatos(int torneoId, FaseDTO dto)
    {
        if (dto.TipoDeFase == TipoDeFaseEnum.TodosContraTodos && dto.InstanciaEliminacionDirectaId.HasValue)
            throw new ExcepcionControlada("La instancia de eliminación directa solo aplica cuando el tipo de fase es eliminación directa.");

        Fase fase = dto.TipoDeFase switch
        {
            TipoDeFaseEnum.TodosContraTodos => new FaseTodosContraTodos
            {
                Id = 0,
                TorneoId = torneoId,
                Nombre = dto.Nombre ?? string.Empty,
                Numero = dto.Numero,
                EstadoFaseId = dto.EstadoFaseId,
                EsVisibleEnApp = dto.EsVisibleEnApp
            },
            TipoDeFaseEnum.EliminacionDirecta => new FaseEliminacionDirecta
            {
                Id = 0,
                TorneoId = torneoId,
                Nombre = dto.Nombre ?? string.Empty,
                Numero = dto.Numero,
                EstadoFaseId = dto.EstadoFaseId,
                EsVisibleEnApp = dto.EsVisibleEnApp,
                InstanciaEliminacionDirectaId = dto.InstanciaEliminacionDirectaId
            },
            _ => throw new ExcepcionControlada("Tipo de fase no válido.")
        };
        _torneoFaseRepo.Crear(fase);
        await BDVirtual.GuardarCambios();

        if (dto.TipoDeFase == TipoDeFaseEnum.TodosContraTodos)
        {
            var zona = new ZonaTodosContraTodos
            {
                Id = 0,
                FaseId = fase.Id,
                Nombre = "Zona única"
            };
            _torneoZonaRepo.Crear(zona);
            await BDVirtual.GuardarCambios();
        }
    }

    private async Task CrearCategoria(int torneoId, TorneoCategoriaDTO dto)
    {
        var categoria = new TorneoCategoria
        {
            Id = 0,
            TorneoId = torneoId,
            Nombre = dto.Nombre,
            AnioDesde = dto.AnioDesde,
            AnioHasta = dto.AnioHasta
        };
        _torneoCategoriaRepo.Crear(categoria);
        await BDVirtual.GuardarCambios();
    }

    public async Task<IEnumerable<TorneoDTO>> Filtrar(int? anio, int? agrupadorId)
    {
        var entidades = await Repo.ListarFiltrado(anio, agrupadorId);
        return Mapper.Map<List<TorneoDTO>>(entidades);
    }
}