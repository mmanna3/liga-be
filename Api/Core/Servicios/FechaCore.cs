using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
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
        var zona = await _torneoZonaRepo.ObtenerPorId(padreId);
        if (zona == null)
            throw new ExcepcionControlada("La zona indicada no existe.");

        Fecha entidad = zona switch
        {
            ZonaTodosContraTodos => new FechaTodosContraTodos
            {
                Id = 0,
                Dia = dto.Dia,
                Numero = dto.Numero,
                EsVisibleEnApp = dto.EsVisibleEnApp,
                ZonaId = padreId
            },
            ZonaEliminacionDirecta => new FechaEliminacionDirecta
            {
                Id = 0,
                Dia = dto.Dia,
                EsVisibleEnApp = dto.EsVisibleEnApp,
                ZonaId = padreId,
                InstanciaId = dto.InstanciaId
                    ?? throw new ExcepcionControlada("La instancia de eliminación directa es obligatoria.")
            },
            _ => throw new ExcepcionControlada("Tipo de zona no soportado para fechas.")
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

        Fecha entidadNueva = entidadAnterior switch
        {
            FechaTodosContraTodos => new FechaTodosContraTodos
            {
                Id = id,
                Dia = dto.Dia,
                Numero = dto.Numero,
                EsVisibleEnApp = dto.EsVisibleEnApp,
                ZonaId = padreId
            },
            FechaEliminacionDirecta => new FechaEliminacionDirecta
            {
                Id = id,
                Dia = dto.Dia,
                EsVisibleEnApp = dto.EsVisibleEnApp,
                ZonaId = padreId,
                InstanciaId = dto.InstanciaId
                    ?? throw new ExcepcionControlada("La instancia de eliminación directa es obligatoria.")
            },
            _ => throw new ExcepcionControlada("Tipo de fecha no soportado.")
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

    public async Task<IEnumerable<FechaDTO>> CrearMasivamente(int padreId, IEnumerable<FechaDTO> dtos)
    {
        var creados = new List<FechaDTO>();
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
    /// Un partido por cada par (jornada, categoría del torneo), con resultados vacíos.
    /// Idempotente: no duplica si ya existía el partido.
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

        var categoriaIds = await _context.TorneoCategorias
            .Where(tc => tc.TorneoId == torneoId)
            .Select(tc => tc.Id)
            .ToListAsync();

        if (categoriaIds.Count == 0)
            return;

        var jornadaIds = await _context.Jornadas
            .Where(j => j.FechaId == fechaId)
            .Select(j => j.Id)
            .ToListAsync();

        if (jornadaIds.Count == 0)
            return;

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
                var jornada = CrearJornadaDesdeDto(fechaId, dto);
                jornada.FechaId = fechaId;
                _context.Jornadas.Add(jornada);
            }
            else
            {
                var existente = jornadasExistentes.FirstOrDefault(j => j.Id == dto.Id);
                if (existente != null)
                {
                    ActualizarJornadaDesdeDto(existente, dto);
                }
            }
        }
    }

    private static Jornada CrearJornadaDesdeDto(int fechaId, JornadaDTO dto)
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
                EquipoLocalId = dto.EquipoLocalId ?? 0
            },
            "Interzonal" => new JornadaInterzonal
            {
                Id = 0,
                FechaId = fechaId,
                ResultadosVerificados = dto.ResultadosVerificados,
                EquipoId = dto.EquipoId ?? 0,
                LocalOVisitanteId = (int)(dto.LocalOVisitante ?? LocalVisitanteEnum.Local)
            },
            _ => throw new ExcepcionControlada($"Tipo de jornada no válido: '{dto.Tipo}'. Debe ser Normal, Libre o Interzonal.")
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
                if (dto.EquipoLocalId.HasValue) libre.EquipoLocalId = dto.EquipoLocalId.Value;
                break;
            case JornadaInterzonal interzonal:
                if (dto.EquipoId.HasValue) interzonal.EquipoId = dto.EquipoId.Value;
                if (dto.LocalOVisitante.HasValue) interzonal.LocalOVisitanteId = (int)dto.LocalOVisitante.Value;
                break;
        }
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

            entidad.ResultadoLocal = partidoDto.ResultadoLocal.Trim();
            entidad.ResultadoVisitante = partidoDto.ResultadoVisitante.Trim();
        }

        if (idsEnRequest.Count != partidosDb.Count)
            throw new ExcepcionControlada("Debe enviar exactamente un partido por cada categoría de la jornada.");

        await BDVirtual.GuardarCambios();
    }
}
