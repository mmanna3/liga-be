using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Logica;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class JugadorCore : ABMCore<IJugadorRepo, Jugador, JugadorDTO>, IJugadorCore
{
    private readonly IEquipoRepo _equipoRepo;
    private readonly IImagenJugadorRepo _imagenJugadorRepo;
    private static AppPaths _paths = null!;

    public JugadorCore(IBDVirtual bd, IJugadorRepo repo, IMapper mapper, IEquipoRepo equipoRepo, IImagenJugadorRepo imagenJugadorRepo, AppPaths paths) : base(bd, repo, mapper)
    {
        _equipoRepo = equipoRepo;
        _imagenJugadorRepo = imagenJugadorRepo;
        _paths = paths;
    }
    
    protected override async Task<Jugador> AntesDeCrear(JugadorDTO dto, Jugador entidad)
    {
        // try
        // {
            dto.EquipoInicialId = GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos(dto.CodigoAlfanumerico);
        // }
        // catch (ExcepcionControlada e)
        // {
        //     return FicharJugadorDTO.Error(e.Message);
        // }
        
        var resultado = await MapearEquipoInicial(dto, entidad);
        
        Repo.SiElDNISeHabiaFichadoYEstaRechazadoEliminarJugador(entidad.DNI);
        _imagenJugadorRepo.GuardarFotosTemporalesDeJugadorAutofichado(dto);
        
        return resultado;
    }

    protected override JugadorDTO AntesDeObtenerPorId(JugadorDTO dto)
    {
        dto.FotoCarnet = ImagenUtility.AgregarMimeType(_imagenJugadorRepo.GetFotoEnBase64ConPathAbsoluto($"{_paths.ImagenesTemporalesJugadorCarnetAbsolute}/{dto.DNI}.jpg"));
        return dto;
    }
    
    private async Task<Jugador> MapearEquipoInicial(JugadorDTO dto, Jugador entidad)
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