using Api.Core.DTOs;
using Api.Core.DTOs.AppCarnetDigital;
using Api.Core.Entidades;
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

    public AppCarnetDigitalCore(IBDVirtual bd, IDelegadoRepo delegadoRepo, IClubRepo clubRepo, IMapper mapper)
    {
        _bdVirtual = bd;
        _delegadoRepo = delegadoRepo;
        _clubRepo = clubRepo;
        _mapper = mapper;
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
}