using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class JugadorCore : ABMCore<IJugadorRepo, Jugador, JugadorDTO>, IJugadorCore
{
    private readonly IEquipoRepo _equipoRepo;
    public JugadorCore(IBDVirtual bd, IJugadorRepo repo, IMapper mapper, IEquipoRepo equipoRepo) : base(bd, repo, mapper)
    {
        _equipoRepo = equipoRepo;
    }
    
    protected override async Task<Jugador> AntesDeCrear(JugadorDTO dto, Jugador entidad)
    {
        var equipo = await _equipoRepo.ObtenerPorId(dto.EquipoInicialId);

        if (equipo == null)
            throw new ExcepcionControlada("El equipo no existe");

        var jugadorEquipo = new JugadorEquipo
        {
            Id = 0,
            EquipoId = dto.EquipoInicialId,
            FechaFichaje = DateTime.Now,
            EstadoJugadorId = (int)EstadoJugadorEnum.FichajePendienteDeAprobacion 
        };

        entidad.JugadorEquipos = new List<JugadorEquipo> { jugadorEquipo };
        return entidad;
    }
}