using Api.Core.DTOs;
using Api.Core.DTOs.AppCarnetDigital;
using Api.Core.Entidades;
using Api.Core.Logica;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class AppCarnetDigitalCore : IAppCarnetDigitalCore
{
    private readonly IBDVirtual _bdVirtual;
    private readonly IDelegadoRepo _delegadoRepo;
    private readonly IClubRepo _clubRepo;
    private readonly IMapper _mapper;
    private readonly IEquipoRepo _equipoRepo;
    private readonly IImagenJugadorRepo _imagenJugadorRepo;

    public AppCarnetDigitalCore(IBDVirtual bd, IDelegadoRepo delegadoRepo, IClubRepo clubRepo, IEquipoRepo equipoRepo, IMapper mapper, IImagenJugadorRepo imagenJugadorRepo)
    {
        _bdVirtual = bd;
        _delegadoRepo = delegadoRepo;
        _clubRepo = clubRepo;
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
        foreach (var jugador in equipo.Jugadores)
        {
            var carnet = _mapper.Map<CarnetDigitalDTO>(jugador);
            carnet.FotoCarnet = ImagenUtility.AgregarMimeType(_imagenJugadorRepo.GetFotoCarnetEnBase64(carnet.DNI));
            lista.Add(carnet);
        }

        return lista;
    }
}