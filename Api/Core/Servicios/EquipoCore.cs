using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Logica;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class EquipoCore : ABMCore<IEquipoRepo, Equipo, EquipoDTO>, IEquipoCore
{
    private readonly IJugadorRepo _jugadorRepo;
    private readonly IImagenJugadorRepo _imagenJugadorRepo;

    public EquipoCore(IBDVirtual bd, IEquipoRepo repo, IMapper mapper, IJugadorRepo jugadorRepo, IImagenJugadorRepo imagenJugadorRepo) : base(bd, repo, mapper)
    {
        _jugadorRepo = jugadorRepo;
        _imagenJugadorRepo = imagenJugadorRepo;
    }

    protected override async Task<Equipo> AntesDeCrear(EquipoDTO dto, Equipo entidad)
    {
        var zonaIds = dto.Zonas?.Where(z => z.Id.HasValue).Select(z => z.Id!.Value).ToList() ?? [];
        if (await Repo.ExisteEquipoConMismoNombreEnZona(entidad.Nombre, zonaIds))
        {
            throw new ExcepcionControlada("Ya existe un equipo con el mismo nombre en este torneo.");
        }

        return await base.AntesDeCrear(dto, entidad);
    }

    public override async Task<int> Crear(EquipoDTO dto)
    {
        var id = await base.Crear(dto);
        await SincronizarZonas(id, dto.Zonas);
        await BDVirtual.GuardarCambios();
        return id;
    }

    protected override async Task<Equipo> AntesDeModificar(int id, EquipoDTO dto, Equipo entidadAnterior, Equipo entidadNueva)
    {
        var zonaIds = dto.Zonas?.Where(z => z.Id.HasValue).Select(z => z.Id!.Value).ToList() ?? [];
        var zonasAnteriores = entidadAnterior.Zonas?.Select(ez => ez.ZonaId).ToList() ?? [];
        var nombreCambio = entidadAnterior.Nombre != entidadNueva.Nombre;
        var zonasCambio = !zonaIds.SequenceEqual(zonasAnteriores);

        if ((nombreCambio || zonasCambio) && await Repo.ExisteEquipoConMismoNombreEnZona(entidadNueva.Nombre, zonaIds, id))
        {
            throw new ExcepcionControlada("Ya existe un equipo con el mismo nombre en este torneo.");
        }

        return await base.AntesDeModificar(id, dto, entidadAnterior, entidadNueva);
    }

    public override async Task<int> Modificar(int id, EquipoDTO dto)
    {
        var result = await base.Modificar(id, dto);
        await SincronizarZonas(id, dto.Zonas);
        await BDVirtual.GuardarCambios();
        return result;
    }

    private async Task SincronizarZonas(int equipoId, List<ZonaResumenDTO>? zonasDto)
    {
        var zonaIds = zonasDto?.Where(z => z.Id.HasValue).Select(z => z.Id!.Value).ToList() ?? [];
        await Repo.SincronizarZonasDelEquipo(equipoId, zonaIds);
        await Task.CompletedTask;
    }

    public async Task<ObtenerNombreEquipoDTO> ObtenerNombrePorCodigoAlfanumerico(string codigoAlfanumerico)
    {
        int id;
        try
        {
            id = GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos(codigoAlfanumerico);
        }
        catch (ExcepcionControlada e)
        {
            return ObtenerNombreEquipoDTO.Error(e.Message);
        }

        var equipo = await Repo.ObtenerPorId(id);
        if (equipo == null)
            return ObtenerNombreEquipoDTO.Error("El código alfanumérico no pertenece a ningún equipo.");

        return ObtenerNombreEquipoDTO.Exito(equipo.Nombre);
    }

    public async Task<ObtenerClubDTO> ObtenerClubPorCodigoAlfanumericoDelEquipo(string codigoAlfanumerico)
    {
        int id;
        try
        {
            id = GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos(codigoAlfanumerico);
        }
        catch (ExcepcionControlada e)
        {
            return ObtenerClubDTO.Error(e.Message);
        }

        var equipo = await Repo.ObtenerPorId(id);
        if (equipo == null)
            return ObtenerClubDTO.Error("El código alfanumérico no pertenece a ningún equipo.");

        var club = equipo.Club;
        if (club == null)
            return ObtenerClubDTO.Error("El equipo no tiene club asociado.");

        return ObtenerClubDTO.Exito(club.Id, club.Nombre);
    }

    public async Task<IEnumerable<EquipoParaZonasDTO>> EquiposParaZonas()
    {
        var equipos = await Repo.ListarConZonasParaEquiposParaZonas();
        var resultado = new List<EquipoParaZonasDTO>();

        foreach (var equipo in equipos)
        {
            var zonas = new List<ZonaResumenDTO>();
            if (equipo.Zonas != null)
            {
                foreach (var ez in equipo.Zonas)
                    zonas.Add(ZonaDesdeZona(ez.Zona));
            }

            var codigoAlfanumerico = equipo.Id > 0 && equipo.Id < 10000
                ? GeneradorDeHash.GenerarAlfanumerico7Digitos(equipo.Id)
                : string.Empty;

            resultado.Add(new EquipoParaZonasDTO
            {
                Id = equipo.Id,
                Nombre = equipo.Nombre,
                Club = equipo.Club?.Nombre ?? string.Empty,
                CodigoAlfanumerico = codigoAlfanumerico,
                Zonas = zonas
            });
        }

        return resultado;
    }

    private static ZonaResumenDTO ZonaDesdeZona(Zona zona)
    {
        Fase? fase = zona switch
        {
            ZonaTodosContraTodos z => z.Fase,
            ZonaEliminacionDirecta z => z.Fase,
            _ => null
        };
        var torneo = fase?.Torneo;
        return new ZonaResumenDTO
        {
            Id = zona.Id,
            Nombre = zona.Nombre,
            TorneoId = torneo?.Id,
            Torneo = torneo != null ? $"{torneo.Nombre} {torneo.Anio}" : string.Empty,
            Agrupador = torneo?.TorneoAgrupador?.Nombre ?? string.Empty,
            AgrupadorId = torneo?.TorneoAgrupadorId,
            Fase = fase?.Nombre,
            FaseId = fase?.Id
        };
    }

    public async Task<IEnumerable<JugadorBaseDTO>> JugadoresQueSoloJueganEnEsteEquipo(int equipoId)
    {
        var equipo = await Repo.ObtenerPorId(equipoId);
        if (equipo == null)
            return [];

        var jugadoresQueSoloJueganEnEsteEquipo = new List<Jugador>();
        foreach (var je in equipo.Jugadores)
        {
            var cantidadEquipos = await Repo.ContarEquiposDelJugador(je.JugadorId);
            if (cantidadEquipos == 1 && je.Jugador != null)
                jugadoresQueSoloJueganEnEsteEquipo.Add(je.Jugador);
        }

        return Mapper.Map<List<JugadorBaseDTO>>(jugadoresQueSoloJueganEnEsteEquipo);
    }

    protected override async Task AntesDeEliminar(int id, Equipo entidad)
    {
        var jugadoresQueSoloJueganEnEsteEquipo = new List<Jugador>();

        foreach (var je in entidad.Jugadores)
        {
            var cantidadEquipos = await Repo.ContarEquiposDelJugador(je.JugadorId);
            if (cantidadEquipos == 1 && je.Jugador != null)
                jugadoresQueSoloJueganEnEsteEquipo.Add(je.Jugador);
        }

        foreach (var je in entidad.Jugadores)
            _jugadorRepo.EliminarJugadorEquipo(je.Id);

        foreach (var jugador in jugadoresQueSoloJueganEnEsteEquipo)
        {
            _jugadorRepo.Eliminar(jugador);
            _imagenJugadorRepo.EliminarFotosDelJugador(jugador.DNI);
        }

        await Task.CompletedTask;
    }
}
