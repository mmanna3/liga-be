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
    private readonly ITorneoRepo _torneoRepo;

    public EquipoCore(IBDVirtual bd, IEquipoRepo repo, IMapper mapper, IJugadorRepo jugadorRepo, IImagenJugadorRepo imagenJugadorRepo, ITorneoRepo torneoRepo) : base(bd, repo, mapper)
    {
        _jugadorRepo = jugadorRepo;
        _imagenJugadorRepo = imagenJugadorRepo;
        _torneoRepo = torneoRepo;
    }

    protected override async Task<Equipo> AntesDeCrear(EquipoDTO dto, Equipo entidad)
    {
        if (dto.TorneoId.HasValue)
        {
            var zona = await _torneoRepo.ObtenerZonaUnicaDeTorneo(dto.TorneoId.Value);
            if (zona == null)
                throw new ExcepcionControlada("El torneo indicado no tiene la estructura de fase/zona esperada.");
            entidad.ZonaActualId = zona.Id;
        }

        if (await Repo.ExisteEquipoConMismoNombreEnZona(entidad.Nombre, entidad.ZonaActualId))
        {
            throw new ExcepcionControlada("Ya existe un equipo con el mismo nombre en este torneo.");
        }

        return await base.AntesDeCrear(dto, entidad);
    }

    protected override async Task<Equipo> AntesDeModificar(int id, EquipoDTO dto, Equipo entidadAnterior, Equipo entidadNueva)
    {
        if (dto.TorneoId.HasValue)
        {
            var zona = await _torneoRepo.ObtenerZonaUnicaDeTorneo(dto.TorneoId.Value);
            if (zona == null)
                throw new ExcepcionControlada("El torneo indicado no tiene la estructura de fase/zona esperada.");
            entidadNueva.ZonaActualId = zona.Id;
        }
        else
        {
            entidadNueva.ZonaActualId = entidadAnterior.ZonaActualId;
        }

        if ((entidadAnterior.Nombre != entidadNueva.Nombre || entidadAnterior.ZonaActualId != entidadNueva.ZonaActualId) &&
            await Repo.ExisteEquipoConMismoNombreEnZona(entidadNueva.Nombre, entidadNueva.ZonaActualId, id))
        {
            throw new ExcepcionControlada("Ya existe un equipo con el mismo nombre en este torneo.");
        }

        return await base.AntesDeModificar(id, dto, entidadAnterior, entidadNueva);
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