using Api.Core.DTOs.AppCarnetDigital;
using Api.Core.Enums;
using Api.Core.Logica;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class AppCarnetDigitalCore : IAppCarnetDigitalCore
{
    private readonly IDelegadoRepo _delegadoRepo;
    private readonly IMapper _mapper;
    private readonly IEquipoRepo _equipoRepo;
    private readonly IImagenJugadorRepo _imagenJugadorRepo;
    private readonly IImagenDelegadoRepo _imagenDelegadoRepo;

    public AppCarnetDigitalCore(IDelegadoRepo delegadoRepo, IEquipoRepo equipoRepo, IMapper mapper, IImagenJugadorRepo imagenJugadorRepo, IImagenDelegadoRepo imagenDelegadoRepo)
    {
        _delegadoRepo = delegadoRepo;
        _mapper = mapper;
        _imagenJugadorRepo = imagenJugadorRepo;
        _imagenDelegadoRepo = imagenDelegadoRepo;
        _equipoRepo = equipoRepo;
    }

    public async Task<EquiposDelDelegadoDTO> ObtenerEquiposPorUsuarioDeDelegado(string usuario)
    {
        var delegado = await _delegadoRepo.ObtenerPorUsuario(usuario);
        var clubsConEquipos = new List<ClubConEquiposDTO>();

        foreach (var delegadoClub in delegado.DelegadoClubs ?? [])
        {
            var club = delegadoClub.Club;
            if (club == null)
                continue;

            var equipos = (club.Equipos ?? [])
                .Select(equipo => _mapper.Map<EquipoBaseDTO>(equipo))
                .ToList();

            clubsConEquipos.Add(new ClubConEquiposDTO
            {
                Nombre = club.Nombre,
                Equipos = equipos
            });
        }

        return new EquiposDelDelegadoDTO
        {
            ClubsConEquipos = clubsConEquipos
        };
    }

    public async Task<ICollection<CarnetDigitalDTO>> Carnets(int equipoId)
    {
        var equipo = await _equipoRepo.ObtenerPorId(equipoId);
        if (equipo == null)
            return null;

        var lista = new List<CarnetDigitalDTO>();
        var club = equipo.Club ?? throw new InvalidOperationException("El equipo debe tener un club asociado.");

        foreach (var jugador in equipo.Jugadores.Where(x => x.EstadoJugadorId != (int)EstadoJugadorEnum.FichajeRechazado && x.EstadoJugadorId != (int)EstadoJugadorEnum.FichajePendienteDeAprobacion && x.EstadoJugadorId != (int)EstadoJugadorEnum.AprobadoPendienteDePago))
        {
            var carnet = _mapper.Map<CarnetDigitalDTO>(jugador);
            carnet.FotoCarnet = ImagenUtility.AgregarMimeType(_imagenJugadorRepo.GetFotoCarnetEnBase64(carnet.DNI));
            carnet.Equipo = equipo.Nombre;  // Porque le cuesta por referencia circular para hacerlo con Automapper
            carnet.EsDelegado = false;
            lista.Add(carnet);
        }

        var dnisYaIncluidos = lista.Select(c => c.DNI).ToHashSet();
        var delegados = await _delegadoRepo.ListarActivosDelClub(club.Id);
        foreach (var delegado in delegados)
        {
            if (dnisYaIncluidos.Contains(delegado.DNI))
                continue;
            dnisYaIncluidos.Add(delegado.DNI);
            var carnet = _mapper.Map<CarnetDigitalDTO>(delegado);
            carnet.FotoCarnet = ImagenUtility.AgregarMimeType(_imagenDelegadoRepo.GetFotoCarnetEnBase64(delegado.DNI));
            carnet.Equipo = club.Nombre;
            carnet.Torneo = "";
            carnet.Estado = (int)EstadoDelegadoEnum.Activo;
            carnet.EsDelegado = true;
            lista.Add(carnet);
        }

        return lista;
    }

    public async Task<ICollection<CarnetDigitalPendienteDTO>> JugadoresPendientes(int equipoId)
    {
        var equipo = await _equipoRepo.ObtenerPorId(equipoId);
        if (equipo == null)
            return null;

        var lista = new List<CarnetDigitalPendienteDTO>();
        foreach (var jugador in equipo.Jugadores.Where(x => x.EstadoJugadorId is (int)EstadoJugadorEnum.FichajeRechazado or (int)EstadoJugadorEnum.FichajePendienteDeAprobacion or (int)EstadoJugadorEnum.AprobadoPendienteDePago))
        {
            var carnet = _mapper.Map<CarnetDigitalPendienteDTO>(jugador);
            carnet.FotoCarnet = ImagenUtility.AgregarMimeType(_imagenJugadorRepo.GetFotoCarnetEnBase64(carnet.DNI));
            carnet.Equipo = equipo.Nombre;  // Porque le cuesta por referencia circular para hacerlo con Automapper
            lista.Add(carnet);
        }

        return lista;
    }

    public Task<ICollection<CarnetDigitalDTO>> CarnetsPorCodigoAlfanumerico(string codigoAlfanumerico)
    {
        var id = GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos(codigoAlfanumerico);
        return Carnets(id);
    }
}