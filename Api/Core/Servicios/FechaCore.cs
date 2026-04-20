using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Logica;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using Api.Persistencia._Config;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Servicios;

public class FechaCore : ABMCoreAnidado<IFechaRepo, Fecha, FechaDTO, int>, IFechaCore
{
    private readonly IZonaRepo _torneoZonaRepo;
    private readonly AppDbContext _context;

    public FechaCore(IBDVirtual bd, IFechaRepo repo, IZonaRepo torneoZonaRepo,
        AppDbContext context, IMapper mapper)
        : base(bd, repo, mapper)
    {
        _torneoZonaRepo = torneoZonaRepo;
        _context = context;
    }

    protected override async Task<Fecha> AntesDeCrear(int padreId, FechaDTO dto, Fecha entidad)
    {
        var zona = await _torneoZonaRepo.ObtenerPorId(padreId);
        if (zona == null)
            throw new ExcepcionControlada("La zona indicada no existe.");

        if (entidad is FechaTodosContraTodos && zona is not ZonaTodosContraTodos)
            throw new ExcepcionControlada("Solo las zonas todos contra todos admiten fechas con número.");
        if (entidad is FechaEliminacionDirecta && zona is not ZonaEliminacionDirecta)
            throw new ExcepcionControlada("Solo las zonas de eliminación directa admiten fechas con instancia.");

        entidad.ZonaId = padreId;
        return entidad;
    }

    public override async Task<int> Crear(int padreId, FechaDTO dto)
    {
        return dto switch
        {
            FechaTodosContraTodosDTO t => await CrearFechaTodosContraTodos(padreId, t),
            FechaEliminacionDirectaDTO e => await CrearFechaEliminacionDirecta(padreId, e),
            _ => throw new ExcepcionControlada("Tipo de fecha no reconocido. Use todosContraTodos o eliminacionDirecta.")
        };
    }

    public async Task<int> CrearFechaTodosContraTodos(int padreId, FechaTodosContraTodosDTO dto)
    {
        var zona = await _torneoZonaRepo.ObtenerPorId(padreId);
        if (zona == null)
            throw new ExcepcionControlada("La zona indicada no existe.");
        if (zona is not ZonaTodosContraTodos)
            throw new ExcepcionControlada("Solo las zonas todos contra todos admiten fechas con número.");

        Fecha entidad = new FechaTodosContraTodos
        {
            Id = 0,
            Dia = dto.Dia,
            Numero = dto.Numero,
            EsVisibleEnApp = dto.EsVisibleEnApp,
            ZonaId = padreId
        };

        entidad = await AntesDeCrear(padreId, dto, entidad);
        Repo.Crear(entidad);
        await BDVirtual.GuardarCambios();
        var id = entidad.Id;

        if (dto.Jornadas != null)
        {
            await AplicarJornadasEnFecha(id, dto.Jornadas);
            await BDVirtual.GuardarCambios();
            await AsegurarPartidosPorCategoriaPorJornada(id, padreId);
            await BDVirtual.GuardarCambios();
        }

        return id;
    }

    public async Task<int> CrearFechaEliminacionDirecta(int padreId, FechaEliminacionDirectaDTO dto)
    {
        var zona = await _torneoZonaRepo.ObtenerPorId(padreId);
        if (zona == null)
            throw new ExcepcionControlada("La zona indicada no existe.");
        if (zona is not ZonaEliminacionDirecta)
            throw new ExcepcionControlada("Solo las zonas de eliminación directa admiten fechas con instancia.");

        Fecha entidad = new FechaEliminacionDirecta
        {
            Id = 0,
            Dia = dto.Dia,
            EsVisibleEnApp = dto.EsVisibleEnApp,
            ZonaId = padreId,
            InstanciaId = dto.InstanciaId
        };

        entidad = await AntesDeCrear(padreId, dto, entidad);
        Repo.Crear(entidad);
        await BDVirtual.GuardarCambios();
        var id = entidad.Id;

        if (dto.Jornadas != null)
        {
            await AplicarJornadasEnFecha(id, dto.Jornadas);
            await BDVirtual.GuardarCambios();
            await AsegurarPartidosPorCategoriaPorJornada(id, padreId);
            await BDVirtual.GuardarCambios();
        }

        return id;
    }

    protected override Task AntesDeModificar(int padreId, int id, FechaDTO dto, Fecha entidadAnterior, Fecha entidadNueva)
    {
        entidadNueva.ZonaId = padreId;
        return Task.CompletedTask;
    }

    public override async Task<int> Modificar(int padreId, int id, FechaDTO dto)
    {
        var entidadAnterior = await Repo.ObtenerPorIdYPadre(padreId, id);
        if (entidadAnterior == null)
            throw new ExcepcionControlada("No existe la entidad a modificar o no pertenece al recurso padre indicado.");

        Fecha entidadNueva = (entidadAnterior, dto) switch
        {
            (FechaTodosContraTodos, FechaTodosContraTodosDTO t) => new FechaTodosContraTodos
            {
                Id = id,
                Dia = dto.Dia,
                Numero = t.Numero,
                EsVisibleEnApp = dto.EsVisibleEnApp,
                ZonaId = padreId
            },
            (FechaEliminacionDirecta, FechaEliminacionDirectaDTO e) => new FechaEliminacionDirecta
            {
                Id = id,
                Dia = dto.Dia,
                EsVisibleEnApp = dto.EsVisibleEnApp,
                ZonaId = padreId,
                InstanciaId = e.InstanciaId
            },
            _ => throw new ExcepcionControlada(
                "El tipo de fecha del cuerpo no coincide con el registro existente (todos contra todos vs eliminación directa).")
        };

        await AntesDeModificar(padreId, id, dto, entidadAnterior, entidadNueva);
        Repo.Modificar(entidadAnterior, entidadNueva);
        await BDVirtual.GuardarCambios();

        if (dto.Jornadas != null)
        {
            await AplicarJornadasEnFecha(id, dto.Jornadas);
            await BDVirtual.GuardarCambios();
            await AsegurarPartidosPorCategoriaPorJornada(id, padreId);
            await BDVirtual.GuardarCambios();
        }

        return id;
    }

    public async Task<IEnumerable<FechaTodosContraTodosDTO>> CrearFechasTodosContraTodosMasivamente(
        int padreId, IEnumerable<FechaTodosContraTodosDTO> dtos)
    {
        var creados = new List<FechaTodosContraTodosDTO>();
        foreach (var dto in dtos)
        {
            dto.EsVisibleEnApp = true;
            var id = await CrearFechaTodosContraTodos(padreId, dto);
            var creado = await ObtenerPorId(padreId, id);
            if (creado is FechaTodosContraTodosDTO f)
                creados.Add(f);
        }
        return creados;
    }

    public async Task<FechaEliminacionDirectaDTO> CrearFechasEliminacionDirectaMasivamente(
        int padreId, FechaEliminacionDirectaDTO dto)
    {
        dto.EsVisibleEnApp = true;

        var instanciasOrdenadas = await _context.InstanciaEliminacionDirecta
            .OrderByDescending(i => i.Id)
            .AsNoTracking()
            .ToListAsync();

        var idxInicio = instanciasOrdenadas.FindIndex(i => i.Id == dto.InstanciaId);
        if (idxInicio < 0)
            throw new ExcepcionControlada("La instancia de eliminación directa no es válida.");

        var desdeInicio = instanciasOrdenadas.Skip(idxInicio).ToList();

        var idPrimera = await CrearFechaEliminacionDirecta(padreId, dto);

        foreach (var inst in desdeInicio.Skip(1))
        {
            var cantidadJornadas = CantidadJornadasPorInstanciaEliminacionDirecta(inst.Id);
            var jornadasPlaceholder = Enumerable.Range(0, cantidadJornadas)
                .Select(_ => new JornadaDTO
                {
                    Tipo = "SinEquipos",
                    ResultadosVerificados = false
                })
                .ToList();

            var dtoPlaceholder = new FechaEliminacionDirectaDTO
            {
                Dia = dto.Dia,
                EsVisibleEnApp = dto.EsVisibleEnApp,
                InstanciaId = inst.Id,
                Jornadas = jornadasPlaceholder
            };

            await CrearFechaEliminacionDirecta(padreId, dtoPlaceholder);
        }

        var creado = await ObtenerPorId(padreId, idPrimera);
        if (creado is FechaEliminacionDirectaDTO f)
            return f;
        throw new ExcepcionControlada("No se pudo obtener la fecha de eliminación directa recién creada.");
    }

    private static int CantidadJornadasPorInstanciaEliminacionDirecta(int instanciaId)
    {
        if (instanciaId < 2 || instanciaId % 2 != 0)
            throw new ExcepcionControlada("Instancia de eliminación directa no válida.");
        return instanciaId / 2;
    }

    public async Task BorrarFechasEliminacionDirectaMasivamente(int padreId)
    {
        var zona = await _torneoZonaRepo.ObtenerPorId(padreId);
        if (zona == null)
            throw new ExcepcionControlada("La zona indicada no existe.");
        if (zona is not ZonaEliminacionDirecta)
            throw new ExcepcionControlada("Solo las zonas de eliminación directa admiten esta operación.");

        var ids = (await Repo.ListarIdsPorPadre(padreId)).ToList();
        foreach (var id in ids)
            await Eliminar(padreId, id);
    }

    public async Task BorrarFechasTodosContraTodosMasivamente(int padreId)
    {
        var zona = await _torneoZonaRepo.ObtenerPorId(padreId);
        if (zona == null)
            throw new ExcepcionControlada("La zona indicada no existe.");
        if (zona is not ZonaTodosContraTodos)
            throw new ExcepcionControlada("Solo las zonas todos contra todos admiten esta operación.");

        var ids = (await Repo.ListarIdsPorPadre(padreId)).ToList();
        foreach (var id in ids)
            await Eliminar(padreId, id);
    }

    public override async Task<int> Eliminar(int padreId, int id)
    {
        var entidad = await Repo.ObtenerPorIdYPadreParaEliminar(padreId, id);
        if (entidad == null)
            return -1;

        int? numeroEliminado = entidad is FechaTodosContraTodos fct ? fct.Numero : null;

        await AntesDeEliminar(padreId, id, entidad);
        Repo.Eliminar(entidad);
        await BDVirtual.GuardarCambios();

        if (numeroEliminado.HasValue)
            await RenumerarFechasConsecutivas(padreId, numeroEliminado.Value);

        return id;
    }

    private async Task RenumerarFechasConsecutivas(int zonaId, int numeroEliminado)
    {
        var fechasARenumerar = await _context.Fechas
            .OfType<FechaTodosContraTodos>()
            .Where(f => f.ZonaId == zonaId && f.Numero > numeroEliminado)
            .OrderBy(f => f.Numero)
            .ToListAsync();

        foreach (var fecha in fechasARenumerar)
        {
            fecha.Numero--;
        }

        await BDVirtual.GuardarCambios();

        foreach (var fecha in fechasARenumerar)
        {
            _context.Entry(fecha).State = EntityState.Detached;
        }
    }

    private async Task RenumerarTodasLasFechasConsecutivas(int zonaId)
    {
        var fechas = await _context.Fechas
            .OfType<FechaTodosContraTodos>()
            .Where(f => f.ZonaId == zonaId)
            .OrderBy(f => f.Numero)
            .ThenBy(f => f.Id)
            .ToListAsync();

        for (var i = 0; i < fechas.Count; i++)
        {
            fechas[i].Numero = i + 1;
        }
    }

    public async Task ModificarMasivamente(int padreId, IEnumerable<FechaDTO> dtos)
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

        var zona = await _torneoZonaRepo.ObtenerPorId(padreId);
        if (zona is ZonaTodosContraTodos)
        {
            await RenumerarTodasLasFechasConsecutivas(padreId);
            await BDVirtual.GuardarCambios();
        }
    }

    /// <summary>
    /// Ids de jornadas de una fecha: consulta a BD + entidades rastreadas (evita desajustes tras guardar jornadas nuevas).
    /// </summary>
    private async Task<List<int>> ListarIdsJornadasDeFecha(int fechaId)
    {
        _context.ChangeTracker.DetectChanges();

        var desdeDb = await _context.Jornadas
            .Where(j => j.FechaId == fechaId)
            .Select(j => j.Id)
            .ToListAsync();

        var desdeTracker = _context.ChangeTracker
            .Entries<Jornada>()
            .Where(e => e.Entity.FechaId == fechaId && e.State != EntityState.Deleted)
            .Select(e => e.Entity.Id)
            .Where(id => id > 0)
            .ToList();

        return desdeDb.Union(desdeTracker).Distinct().ToList();
    }

    /// <summary>
    /// Zona todos contra todos: un partido por cada par (jornada, categoría del torneo).
    /// Zona eliminación directa: un solo partido por jornada, con la categoría de la zona.
    /// Resultados vacíos. Idempotente: no duplica si ya existía el partido.
    /// </summary>
    private async Task AsegurarPartidosPorCategoriaPorJornada(int fechaId, int zonaId)
    {
        var torneoId = await (
            from z in _context.Zonas
            join f in _context.Fases on z.FaseId equals f.Id
            where z.Id == zonaId
            select f.TorneoId).FirstOrDefaultAsync();

        if (torneoId == 0)
            return;

        var zonaEliminacionDirecta = await _context.Zonas
            .OfType<ZonaEliminacionDirecta>()
            .AsNoTracking()
            .FirstOrDefaultAsync(z => z.Id == zonaId);

        List<int> categoriaIds;
        if (zonaEliminacionDirecta != null)
            categoriaIds = [zonaEliminacionDirecta.CategoriaId];
        else
            categoriaIds = await _context.TorneoCategorias
                .Where(tc => tc.TorneoId == torneoId)
                .Select(tc => tc.Id)
                .ToListAsync();

        if (categoriaIds.Count == 0)
            return;

        var jornadaIds = await ListarIdsJornadasDeFecha(fechaId);

        if (jornadaIds.Count == 0)
            return;

        if (zonaEliminacionDirecta != null)
        {
            var categoriaZona = zonaEliminacionDirecta.CategoriaId;
            var partidosOtraCategoria = await _context.Partidos
                .Where(p => jornadaIds.Contains(p.JornadaId) && p.CategoriaId != categoriaZona)
                .ToListAsync();
            if (partidosOtraCategoria.Count > 0)
            {
                _context.Partidos.RemoveRange(partidosOtraCategoria);
                await BDVirtual.GuardarCambios();
            }
        }

        var existentes = await _context.Partidos
            .Where(p => jornadaIds.Contains(p.JornadaId))
            .Select(p => new { p.JornadaId, p.CategoriaId })
            .ToListAsync();

        var existentesSet = existentes.Select(x => (x.JornadaId, x.CategoriaId)).ToHashSet();

        foreach (var jornadaId in jornadaIds)
        {
            foreach (var categoriaId in categoriaIds)
            {
                if (existentesSet.Contains((jornadaId, categoriaId)))
                    continue;

                _context.Partidos.Add(new Partido
                {
                    Id = 0,
                    CategoriaId = categoriaId,
                    JornadaId = jornadaId,
                    ResultadoLocal = "",
                    ResultadoVisitante = ""
                });
                existentesSet.Add((jornadaId, categoriaId));
            }
        }
    }

    private async Task AplicarJornadasEnFecha(int fechaId, List<JornadaDTO> jornadasDtos)
    {
        var idsEnRequest = jornadasDtos.Where(j => j.Id > 0).Select(j => j.Id).ToHashSet();

        var jornadasExistentes = await _context.Jornadas
            .Where(j => j.FechaId == fechaId)
            .ToListAsync();
        var idsExistentes = jornadasExistentes.Select(j => j.Id).ToHashSet();
        var idsAEliminar = idsExistentes.Where(id => !idsEnRequest.Contains(id)).ToList();

        if (idsAEliminar.Count > 0)
        {
            var partidosDeJornadasEliminadas = await _context.Partidos
                .Where(p => idsAEliminar.Contains(p.JornadaId))
                .ToListAsync();
            _context.Partidos.RemoveRange(partidosDeJornadasEliminadas);

            foreach (var jornada in jornadasExistentes.Where(j => idsAEliminar.Contains(j.Id)))
            {
                _context.Jornadas.Remove(jornada);
            }
        }

        foreach (var dto in jornadasDtos)
        {
            if (dto.Id <= 0)
            {
                var tipo = (dto.Tipo ?? "").Trim();
                int? numeroInterzonal = null;
                if (tipo == "Interzonal")
                {
                    if (dto.Numero is >= 1)
                        numeroInterzonal = dto.Numero.Value;
                    else
                        numeroInterzonal = await SiguienteNumeroInterzonalAsync(fechaId);
                }

                var jornada = CrearJornadaDesdeDto(fechaId, dto, numeroInterzonal);
                jornada.FechaId = fechaId;
                _context.Jornadas.Add(jornada);
                // Un SaveChanges por jornada nueva: si se agrupan varias inserciones en un solo guardado,
                // EF puede ordenar filas por tipo (TPH) y los IDs identity dejan de seguir el orden del payload.
                await BDVirtual.GuardarCambios();
            }
            else
            {
                var existente = jornadasExistentes.FirstOrDefault(j => j.Id == dto.Id);
                if (existente != null)
                    ActualizarJornadaDesdeDto(existente, dto);
            }
        }
    }

    private async Task<int> SiguienteNumeroInterzonalAsync(int fechaId)
    {
        var max = await _context.Jornadas.OfType<JornadaInterzonal>()
            .Where(j => j.FechaId == fechaId)
            .MaxAsync(j => (int?)j.Numero);
        return (max ?? 0) + 1;
    }

    private static Jornada CrearJornadaDesdeDto(int fechaId, JornadaDTO dto, int? numeroInterzonalParaInterzonal)
    {
        var tipo = (dto.Tipo ?? "").Trim();
        return tipo switch
        {
            "Normal" => new JornadaNormal
            {
                Id = 0,
                FechaId = fechaId,
                ResultadosVerificados = dto.ResultadosVerificados,
                LocalEquipoId = dto.LocalId ?? 0,
                VisitanteEquipoId = dto.VisitanteId ?? 0
            },
            "Libre" => new JornadaLibre
            {
                Id = 0,
                FechaId = fechaId,
                ResultadosVerificados = dto.ResultadosVerificados,
                EquipoId = dto.EquipoId ?? 0,
                LocalOVisitanteId = (int)(dto.LocalOVisitante ?? LocalVisitanteEnum.Local)
            },
            "Interzonal" => new JornadaInterzonal
            {
                Id = 0,
                FechaId = fechaId,
                ResultadosVerificados = dto.ResultadosVerificados,
                Numero = numeroInterzonalParaInterzonal
                         ?? throw new ExcepcionControlada("No se pudo asignar el número de jornada interzonal."),
                EquipoId = dto.EquipoId ?? 0,
                LocalOVisitanteId = (int)(dto.LocalOVisitante ?? LocalVisitanteEnum.Local)
            },
            "SinEquipos" => new JornadaSinEquipos
            {
                Id = 0,
                FechaId = fechaId,
                ResultadosVerificados = dto.ResultadosVerificados
            },
            _ => throw new ExcepcionControlada($"Tipo de jornada no válido: '{dto.Tipo}'. Debe ser Normal, Libre, Interzonal o SinEquipos.")
        };
    }

    private static void ActualizarJornadaDesdeDto(Jornada existente, JornadaDTO dto)
    {
        existente.ResultadosVerificados = dto.ResultadosVerificados;

        switch (existente)
        {
            case JornadaNormal normal:
                if (dto.LocalId.HasValue) normal.LocalEquipoId = dto.LocalId.Value;
                if (dto.VisitanteId.HasValue) normal.VisitanteEquipoId = dto.VisitanteId.Value;
                break;
            case JornadaLibre libre:
                if (dto.EquipoId.HasValue) libre.EquipoId = dto.EquipoId.Value;
                if (dto.LocalOVisitante.HasValue) libre.LocalOVisitanteId = (int)dto.LocalOVisitante.Value;
                break;
            case JornadaInterzonal interzonal:
                if (dto.EquipoId.HasValue) interzonal.EquipoId = dto.EquipoId.Value;
                if (dto.LocalOVisitante.HasValue) interzonal.LocalOVisitanteId = (int)dto.LocalOVisitante.Value;
                if (dto.Numero.HasValue)
                {
                    if (dto.Numero.Value < 1)
                        throw new ExcepcionControlada("El número de jornada interzonal debe ser mayor a cero.");
                    interzonal.Numero = dto.Numero.Value;
                }

                break;
            case JornadaSinEquipos:
                break;
        }
    }

    public async Task CambiarVisibilidadEnApp(int zonaId, int fechaId, bool esVisibleEnApp)
    {
        var filas = await Repo.ActualizarEsVisibleEnApp(zonaId, fechaId, esVisibleEnApp);
        if (filas == 0)
            throw new ExcepcionControlada("No existe la fecha a modificar o no pertenece a la zona indicada.");
    }

    public async Task CargarResultados(int zonaId, int jornadaId, CargarResultadosDTO dto)
    {
        if (dto.JornadaId != jornadaId)
            throw new ExcepcionControlada("El identificador de jornada no coincide con la ruta.");

        var jornada = await _context.Jornadas
            .Include(j => j.Fecha)
            .FirstOrDefaultAsync(j => j.Id == jornadaId);

        if (jornada == null)
            throw new ExcepcionControlada("La jornada indicada no existe.");

        if (jornada.Fecha.ZonaId != zonaId)
            throw new ExcepcionControlada("La jornada no pertenece a la zona indicada.");

        jornada.ResultadosVerificados = dto.ResultadosVerificados;

        var partidos = dto.Partidos?.ToList() ?? new List<PartidoDTO>();

        var partidosDb = await _context.Partidos
            .Where(p => p.JornadaId == jornadaId)
            .ToListAsync();

        if (partidosDb.Count == 0 && partidos.Count == 0)
        {
            await BDVirtual.GuardarCambios();
            return;
        }

        if (partidosDb.Count != partidos.Count)
            throw new ExcepcionControlada("Debe enviar exactamente un partido por cada categoría de la jornada.");

        var zonaEsEliminacionDirecta = await _context.Zonas
            .OfType<ZonaEliminacionDirecta>()
            .AnyAsync(z => z.Id == zonaId);

        var porId = partidosDb.ToDictionary(p => p.Id);
        var idsEnRequest = new HashSet<int>();

        foreach (var partidoDto in partidos)
        {
            if (partidoDto.Id <= 0)
                throw new ExcepcionControlada("Cada partido debe tener un identificador válido.");

            if (!idsEnRequest.Add(partidoDto.Id))
                throw new ExcepcionControlada("Hay partidos duplicados en la solicitud.");

            if (!porId.TryGetValue(partidoDto.Id, out var entidad))
                throw new ExcepcionControlada("Uno de los partidos no pertenece a esta jornada.");

            if (entidad.CategoriaId != partidoDto.CategoriaId)
                throw new ExcepcionControlada("La categoría de un partido no coincide con el registro existente.");

            PartidoResultadoValidador.ValidarParResultados(partidoDto.ResultadoLocal, partidoDto.ResultadoVisitante);
            PartidoResultadoValidador.ValidarPenalesSegunZonaYResultado(
                zonaEsEliminacionDirecta,
                partidoDto.ResultadoLocal,
                partidoDto.ResultadoVisitante,
                partidoDto.PenalesLocal,
                partidoDto.PenalesVisitante);

            entidad.ResultadoLocal = partidoDto.ResultadoLocal.Trim();
            entidad.ResultadoVisitante = partidoDto.ResultadoVisitante.Trim();
            entidad.PenalesLocal = string.IsNullOrWhiteSpace(partidoDto.PenalesLocal)
                ? null
                : partidoDto.PenalesLocal.Trim();
            entidad.PenalesVisitante = string.IsNullOrWhiteSpace(partidoDto.PenalesVisitante)
                ? null
                : partidoDto.PenalesVisitante.Trim();
        }

        if (idsEnRequest.Count != partidosDb.Count)
            throw new ExcepcionControlada("Debe enviar exactamente un partido por cada categoría de la jornada.");

        await BDVirtual.GuardarCambios();

        if (zonaEsEliminacionDirecta)
        {
            await PropagarGanadoresAFechaSiguienteSiCorresponde(zonaId, jornada.FechaId);
            await BDVirtual.GuardarCambios();
        }
    }

    private void ReemplazarJornadaSiguienteConEquipos(Jornada jNext, int? g1, int? g2)
    {
        var fechaId = jNext.FechaId;

        if (g1.HasValue && g2.HasValue)
        {
            switch (jNext)
            {
                case JornadaSinEquipos sin:
                    _context.Jornadas.Remove(sin);
                    _context.Jornadas.Add(new JornadaNormal
                    {
                        Id = 0,
                        FechaId = fechaId,
                        ResultadosVerificados = false,
                        LocalEquipoId = g1.Value,
                        VisitanteEquipoId = g2.Value
                    });
                    break;
                case JornadaNormal n:
                    n.LocalEquipoId = g1.Value;
                    n.VisitanteEquipoId = g2.Value;
                    break;
            }

            return;
        }

        switch (jNext)
        {
            case JornadaNormal n:
                _context.Jornadas.Remove(n);
                _context.Jornadas.Add(new JornadaSinEquipos
                {
                    Id = 0,
                    FechaId = fechaId,
                    ResultadosVerificados = false
                });
                break;
            case JornadaSinEquipos:
            case JornadaLibre:
            case JornadaInterzonal:
                break;
        }
    }

    /// <summary>
    /// Empareja jornadas de la fecha actual con las de la siguiente solo por orden de id de jornada (ascendente).
    /// Los equipos que pasan se resuelven por id (sin nombres ni otros criterios).
    /// </summary>
    private async Task PropagarGanadoresAFechaSiguienteSiCorresponde(int zonaId, int fechaId)
    {
        var todasVerificadas = await _context.Jornadas
            .Where(j => j.FechaId == fechaId)
            .AllAsync(j => j.ResultadosVerificados);
        if (!todasVerificadas)
            return;

        var fechaActual = await _context.Fechas
            .OfType<FechaEliminacionDirecta>()
            .FirstOrDefaultAsync(f => f.Id == fechaId);
        if (fechaActual == null)
            return;

        var instanciasOrdenadasDesc = await _context.InstanciaEliminacionDirecta
            .OrderByDescending(i => i.Id)
            .Select(i => i.Id)
            .ToListAsync();

        var idx = instanciasOrdenadasDesc.IndexOf(fechaActual.InstanciaId);
        if (idx < 0 || idx + 1 >= instanciasOrdenadasDesc.Count)
            return;

        var siguienteInstanciaId = instanciasOrdenadasDesc[idx + 1];

        var fechaSiguiente = await _context.Fechas
            .OfType<FechaEliminacionDirecta>()
            .FirstOrDefaultAsync(f => f.ZonaId == zonaId && f.InstanciaId == siguienteInstanciaId);
        if (fechaSiguiente == null)
            return;

        var esperadoActual = CantidadJornadasPorInstanciaEliminacionDirecta(fechaActual.InstanciaId);
        var esperadoSiguiente = CantidadJornadasPorInstanciaEliminacionDirecta(siguienteInstanciaId);

        var jornadasActual = await _context.Jornadas
            .Where(j => j.FechaId == fechaId)
            .OrderBy(j => j.Id)
            .ToListAsync();

        var jornadasSiguiente = await _context.Jornadas
            .Where(j => j.FechaId == fechaSiguiente.Id)
            .OrderBy(j => j.Id)
            .ToListAsync();

        if (jornadasActual.Count != esperadoActual || jornadasSiguiente.Count != esperadoSiguiente)
            return;

        if (jornadasActual.Count != 2 * jornadasSiguiente.Count)
            return;

        var idsJornadasActual = jornadasActual.Select(j => j.Id).ToList();
        var partidosPorJornadaId = await _context.Partidos
            .Where(p => idsJornadasActual.Contains(p.JornadaId))
            .ToDictionaryAsync(p => p.JornadaId);

        for (var k = 0; k < jornadasSiguiente.Count; k++)
        {
            var j1 = jornadasActual[2 * k];
            var j2 = jornadasActual[2 * k + 1];
            if (!partidosPorJornadaId.TryGetValue(j1.Id, out var p1) ||
                !partidosPorJornadaId.TryGetValue(j2.Id, out var p2))
                continue;

            var entrada1 = EliminacionDirectaLogica.CrearEntrada(j1, p1);
            var entrada2 = EliminacionDirectaLogica.CrearEntrada(j2, p2);
            var g1 = EliminacionDirectaLogica.DecidirQueEquipoPasaALaSiguienteInstancia(entrada1);
            var g2 = EliminacionDirectaLogica.DecidirQueEquipoPasaALaSiguienteInstancia(entrada2);

            var jNext = jornadasSiguiente[k];
            ReemplazarJornadaSiguienteConEquipos(jNext, g1, g2);
        }

        // Las jornadas nuevas (p. ej. Normal reemplazando SinEquipos) tienen Id=0 hasta guardar;
        // sin esto, ListarIdsJornadasDeFecha no las ve y no se crean partidos en la instancia siguiente.
        await BDVirtual.GuardarCambios();
        await AsegurarPartidosPorCategoriaPorJornada(fechaSiguiente.Id, zonaId);
    }
}
