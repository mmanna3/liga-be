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

public class TorneoFechaCore : ABMCoreAnidado<ITorneoFechaRepo, TorneoFecha, TorneoFechaDTO, int>, ITorneoFechaCore
{
    private readonly ITorneoZonaRepo _torneoZonaRepo;
    private readonly AppDbContext _context;

    public TorneoFechaCore(IBDVirtual bd, ITorneoFechaRepo repo, ITorneoZonaRepo torneoZonaRepo,
        AppDbContext context, IMapper mapper)
        : base(bd, repo, mapper)
    {
        _torneoZonaRepo = torneoZonaRepo;
        _context = context;
    }

    protected override async Task<TorneoFecha> AntesDeCrear(int padreId, TorneoFechaDTO dto, TorneoFecha entidad)
    {
        var zona = await _torneoZonaRepo.ObtenerPorId(padreId);
        if (zona == null)
            throw new ExcepcionControlada("La zona indicada no existe.");
        if (zona is not ZonaTodosContraTodos)
            throw new ExcepcionControlada("Solo las zonas todos contra todos admiten fechas.");

        entidad.ZonaId = padreId;
        return entidad;
    }

    public override async Task<int> Crear(int padreId, TorneoFechaDTO dto)
    {
        var id = await base.Crear(padreId, dto);

        if (dto.Jornadas != null)
        {
            await AplicarJornadasEnFecha(id, dto.Jornadas);
            await BDVirtual.GuardarCambios();
        }

        return id;
    }

    protected override Task AntesDeModificar(int padreId, int id, TorneoFechaDTO dto, TorneoFecha entidadAnterior, TorneoFecha entidadNueva)
    {
        entidadNueva.ZonaId = padreId;
        return Task.CompletedTask;
    }

    public override async Task<int> Modificar(int padreId, int id, TorneoFechaDTO dto)
    {
        await base.Modificar(padreId, id, dto);

        if (dto.Jornadas != null)
        {
            await AplicarJornadasEnFecha(id, dto.Jornadas);
            await BDVirtual.GuardarCambios();
        }

        return id;
    }

    public async Task<IEnumerable<TorneoFechaDTO>> CrearMasivamente(int padreId, IEnumerable<TorneoFechaDTO> dtos)
    {
        var creados = new List<TorneoFechaDTO>();
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

        var numeroEliminado = entidad.Numero;
        await AntesDeEliminar(padreId, id, entidad);
        Repo.Eliminar(entidad);
        await BDVirtual.GuardarCambios();

        await RenumerarFechasConsecutivas(padreId, numeroEliminado);

        return id;
    }

    private async Task RenumerarFechasConsecutivas(int zonaId, int numeroEliminado)
    {
        var fechasARenumerar = await _context.TorneoFechas
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
        var fechas = await _context.TorneoFechas
            .Where(f => f.ZonaId == zonaId)
            .OrderBy(f => f.Numero)
            .ThenBy(f => f.Id)
            .ToListAsync();

        for (var i = 0; i < fechas.Count; i++)
        {
            fechas[i].Numero = i + 1;
        }
    }

    public async Task ModificarMasivamente(int padreId, IEnumerable<TorneoFechaDTO> dtos)
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

        await RenumerarTodasLasFechasConsecutivas(padreId);
        await BDVirtual.GuardarCambios();
    }

    private async Task AplicarJornadasEnFecha(int fechaId, List<JornadaDTO> jornadasDtos)
    {
        var idsEnRequest = jornadasDtos.Where(j => j.Id > 0).Select(j => j.Id).ToHashSet();

        var jornadasExistentes = await _context.Jornadas
            .Where(j => j.FechaId == fechaId)
            .ToListAsync();
        var idsExistentes = jornadasExistentes.Select(j => j.Id).ToHashSet();
        var idsAEliminar = idsExistentes.Where(id => !idsEnRequest.Contains(id)).ToList();

        foreach (var jornada in jornadasExistentes.Where(j => idsAEliminar.Contains(j.Id)))
        {
            _context.Jornadas.Remove(jornada);
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
                EquipoId = dto.EquipoId ?? 0
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
                if (dto.EquipoId.HasValue) libre.EquipoId = dto.EquipoId.Value;
                break;
            case JornadaInterzonal interzonal:
                if (dto.EquipoId.HasValue) interzonal.EquipoId = dto.EquipoId.Value;
                if (dto.LocalOVisitante.HasValue) interzonal.LocalOVisitanteId = (int)dto.LocalOVisitante.Value;
                break;
        }
    }
}
