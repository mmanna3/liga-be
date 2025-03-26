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

    public AppCarnetDigitalCore(IDelegadoRepo delegadoRepo, IEquipoRepo equipoRepo, IMapper mapper, IImagenJugadorRepo imagenJugadorRepo)
    {
        _delegadoRepo = delegadoRepo;
        _mapper = mapper;
        _imagenJugadorRepo = imagenJugadorRepo;
        _equipoRepo = equipoRepo;
    }

    public async Task<EquiposDelDelegadoDTO> ObtenerEquiposPorUsuarioDeDelegado(string usuario)
    {
        var delegado = await _delegadoRepo.ObtenerPorUsuario(usuario);
        var lista = new List<EquipoBaseDTO>();
        foreach (var equipo in delegado.Club.Equipos)
        {
            var equipoMinimo = _mapper.Map<EquipoBaseDTO>(equipo);    
            lista.Add(equipoMinimo);
        }

        var resultado = new EquiposDelDelegadoDTO
        {
            Club = delegado.Club.Nombre,
            Equipos = lista
        };

        return resultado;
    }

    public async Task<ICollection<CarnetDigitalDTO>> Carnets(int equipoId)
    {
        var equipo = await _equipoRepo.ObtenerPorId(equipoId);
        if (equipo == null)
            return null;

        var lista = new List<CarnetDigitalDTO>();
        foreach (var jugador in equipo.Jugadores.Where(x => x.EstadoJugadorId != (int)EstadoJugadorEnum.FichajeRechazado && x.EstadoJugadorId != (int)EstadoJugadorEnum.FichajePendienteDeAprobacion))
        {
            var carnet = _mapper.Map<CarnetDigitalDTO>(jugador);
            carnet.FotoCarnet = ImagenUtility.AgregarMimeType(_imagenJugadorRepo.GetFotoCarnetEnBase64(carnet.DNI));
            carnet.Equipo = equipo.Nombre;  // Porque le cuesta por referencia circular para hacerlo con Automapper
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
        foreach (var jugador in equipo.Jugadores.Where(x => x.EstadoJugadorId is (int)EstadoJugadorEnum.FichajeRechazado or (int)EstadoJugadorEnum.FichajePendienteDeAprobacion))
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