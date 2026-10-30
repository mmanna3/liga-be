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

public class ArbitroCore : ABMCore<IArbitroRepo, Arbitro, ArbitroDTO>, IArbitroCore
{
    private readonly AppDbContext _context;

    public ArbitroCore(IBDVirtual bd, IArbitroRepo repo, IMapper mapper, AppDbContext context) : base(bd, repo, mapper)
    {
        _context = context;
    }

    public override async Task<IEnumerable<ArbitroDTO>> Listar()
    {
        var activos = await Repo.ListarPorEstado(EstadoArbitroEnum.Activo);
        return Mapper.Map<List<ArbitroDTO>>(activos);
    }

    public async Task<IEnumerable<ArbitroDTO>> ListarPendientes()
    {
        var pendientes = await Repo.ListarPorEstado(EstadoArbitroEnum.PendienteDeAprobacion);
        return Mapper.Map<List<ArbitroDTO>>(pendientes);
    }

    public async Task<int> Registrar(RegistroArbitroDTO dto)
    {
        var dni = dto.DNI.Trim();
        if (await Repo.ExisteConDni(dni))
            throw new ExcepcionControlada("Ya hay un árbitro registrado con ese DNI");

        var tieneRelacionInstitucional = dto.TieneRelacionInstitucional!.Value;
        var institucion = dto.Institucion?.Trim();
        if (tieneRelacionInstitucional && string.IsNullOrEmpty(institucion))
            throw new ExcepcionControlada("La institución es obligatoria si tenés relación con alguna institución.");

        ValidarTipoMovilidad(dto.TipoMovilidad);
        var franjas = ValidarYObtenerFranjasHorarias(dto.FranjasHorarias);
        var agrupadorIds = await ValidarYObtenerTorneoAgrupadorIds(dto.TorneoAgrupadorIds);

        var arbitro = new Arbitro
        {
            Id = 0,
            DNI = dni,
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            TelefonoCelular = dto.TelefonoCelular,
            Direccion = dto.Direccion.Trim(),
            Localidad = dto.Localidad.Trim(),
            FechaNacimiento = dto.FechaNacimiento!.Value.Date,
            TipoMovilidadArbitroId = (int)dto.TipoMovilidad!.Value,
            TieneRelacionInstitucional = tieneRelacionInstitucional,
            Institucion = tieneRelacionInstitucional ? institucion : null,
            EstadoArbitroId = (int)EstadoArbitroEnum.PendienteDeAprobacion,
            ArbitroTorneoAgrupadores = ConstruirAgrupadores(agrupadorIds),
            ArbitroFranjasHorarias = ConstruirFranjasHorarias(franjas)
        };

        Repo.Crear(arbitro);
        await BDVirtual.GuardarCambios();
        return arbitro.Id;
    }

    public async Task<int> Aprobar(int id, AprobarArbitroDTO dto)
    {
        var arbitro = await _context.Arbitros.SingleOrDefaultAsync(a => a.Id == id);
        if (arbitro == null)
            return -1;

        if (arbitro.EstadoArbitroId != (int)EstadoArbitroEnum.PendienteDeAprobacion)
            throw new ExcepcionControlada("El árbitro no está pendiente de aprobación.");

        var agrupadorIds = await ValidarYObtenerTorneoAgrupadorIds(dto.TorneoAgrupadorIds);
        var equipoIds = await ValidarYObtenerEquipoProhibidoIds(dto.EquipoProhibidoIds);

        await Repo.EliminarAgrupadoresDelArbitro(id);
        await Repo.EliminarEquiposProhibidosDelArbitro(id);
        _context.ArbitroTorneoAgrupador.AddRange(agrupadorIds.Select(agrupadorId => new ArbitroTorneoAgrupador
        {
            Id = 0,
            ArbitroId = id,
            TorneoAgrupadorId = agrupadorId
        }));
        _context.ArbitroEquipoProhibido.AddRange(equipoIds.Select(equipoId => new ArbitroEquipoProhibido
        {
            Id = 0,
            ArbitroId = id,
            EquipoId = equipoId
        }));

        arbitro.EstadoArbitroId = (int)EstadoArbitroEnum.Activo;
        await BDVirtual.GuardarCambios();
        return id;
    }

    protected override async Task<Arbitro> AntesDeCrear(ArbitroDTO dto, Arbitro entidad)
    {
        ValidarTipoMovilidad(dto.TipoMovilidad);
        entidad.ArbitroFranjasHorarias = ConstruirFranjasHorarias(ValidarYObtenerFranjasHorarias(dto.FranjasHorarias));

        var agrupadorIds = await ValidarYObtenerTorneoAgrupadorIds(dto.TorneoAgrupadorIds);
        entidad.ArbitroTorneoAgrupadores = ConstruirAgrupadores(agrupadorIds);

        var equipoIds = await ValidarYObtenerEquipoProhibidoIds(dto.EquipoProhibidoIds);
        entidad.ArbitroEquiposProhibidos = ConstruirEquiposProhibidos(equipoIds);

        entidad.EstadoArbitroId = (int)EstadoArbitroEnum.Activo;
        return entidad;
    }

    protected override async Task<Arbitro> AntesDeModificar(int id, ArbitroDTO dto, Arbitro entidadAnterior, Arbitro entidadNueva)
    {
        ValidarTipoMovilidad(dto.TipoMovilidad);
        var franjas = ValidarYObtenerFranjasHorarias(dto.FranjasHorarias);
        await Repo.EliminarFranjasHorariasDelArbitro(id);
        entidadNueva.ArbitroFranjasHorarias = ConstruirFranjasHorarias(franjas);

        var agrupadorIds = await ValidarYObtenerTorneoAgrupadorIds(dto.TorneoAgrupadorIds);
        await Repo.EliminarAgrupadoresDelArbitro(id);
        entidadNueva.ArbitroTorneoAgrupadores = ConstruirAgrupadores(agrupadorIds);

        var equipoIds = await ValidarYObtenerEquipoProhibidoIds(dto.EquipoProhibidoIds);
        await Repo.EliminarEquiposProhibidosDelArbitro(id);
        entidadNueva.ArbitroEquiposProhibidos = ConstruirEquiposProhibidos(equipoIds);

        entidadNueva.EstadoArbitroId = entidadAnterior.EstadoArbitroId;
        entidadNueva.Id = id;
        return entidadNueva;
    }

    private static void ValidarTipoMovilidad(TipoMovilidadArbitroEnum? tipoMovilidad)
    {
        if (tipoMovilidad.HasValue && !Enum.IsDefined(tipoMovilidad.Value))
            throw new ExcepcionControlada("El tipo de movilidad no es válido.");
    }

    private static List<FranjaHorariaArbitroEnum> ValidarYObtenerFranjasHorarias(List<FranjaHorariaArbitroEnum>? franjasHorarias)
    {
        var franjas = (franjasHorarias ?? []).Distinct().ToList();
        if (franjas.Any(f => !Enum.IsDefined(f)))
            throw new ExcepcionControlada("Una o más franjas horarias no son válidas.");
        return franjas;
    }

    private static List<ArbitroFranjaHoraria> ConstruirFranjasHorarias(IEnumerable<FranjaHorariaArbitroEnum> franjas)
    {
        return franjas.Select(franja => new ArbitroFranjaHoraria
        {
            Id = 0,
            FranjaHorariaArbitroId = (int)franja
        }).ToList();
    }

    private async Task<List<int>> ValidarYObtenerTorneoAgrupadorIds(List<int>? torneoAgrupadorIds)
    {
        var ids = (torneoAgrupadorIds ?? []).Distinct().ToList();
        if (ids.Count == 0)
            return ids;

        var existentes = await _context.TorneoAgrupadores
            .Where(t => ids.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync();
        var inexistentes = ids.Except(existentes).ToList();
        if (inexistentes.Count > 0)
            throw new ExcepcionControlada("Uno o más agrupadores de torneo no existen en el sistema.");

        return ids;
    }

    private async Task<List<int>> ValidarYObtenerEquipoProhibidoIds(List<int>? equipoProhibidoIds)
    {
        var ids = (equipoProhibidoIds ?? []).Distinct().ToList();
        if (ids.Count == 0)
            return ids;

        var existentes = await _context.Equipos
            .Where(e => ids.Contains(e.Id))
            .Select(e => e.Id)
            .ToListAsync();
        var inexistentes = ids.Except(existentes).ToList();
        if (inexistentes.Count > 0)
            throw new ExcepcionControlada("Uno o más equipos prohibidos no existen en el sistema.");

        return ids;
    }

    private static List<ArbitroTorneoAgrupador> ConstruirAgrupadores(IEnumerable<int> torneoAgrupadorIds)
    {
        return torneoAgrupadorIds.Select(torneoAgrupadorId => new ArbitroTorneoAgrupador
        {
            Id = 0,
            TorneoAgrupadorId = torneoAgrupadorId
        }).ToList();
    }

    private static List<ArbitroEquipoProhibido> ConstruirEquiposProhibidos(IEnumerable<int> equipoIds)
    {
        return equipoIds.Select(equipoId => new ArbitroEquipoProhibido
        {
            Id = 0,
            EquipoId = equipoId
        }).ToList();
    }
}
