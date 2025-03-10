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

    protected override JugadorDTO AntesDeObtenerPorId(Jugador entidad, JugadorDTO dto)
    {
        if (entidad.JugadorEquipos.Any(x =>
                x.EstadoJugadorId is (int)EstadoJugadorEnum.FichajePendienteDeAprobacion or (int)EstadoJugadorEnum.FichajeRechazado))
        {
            dto.FotoCarnet = ImagenUtility.AgregarMimeType(_imagenJugadorRepo.GetFotoEnBase64ConPathAbsoluto($"{_paths.ImagenesTemporalesJugadorCarnetAbsolute}/{dto.DNI}.jpg"));    
            dto.FotoDNIDorso = ImagenUtility.AgregarMimeType(_imagenJugadorRepo.GetFotoEnBase64ConPathAbsoluto($"{_paths.ImagenesTemporalesJugadorDNIDorsoAbsolute}/{dto.DNI}.jpg"));    
            dto.FotoDNIFrente = ImagenUtility.AgregarMimeType(_imagenJugadorRepo.GetFotoEnBase64ConPathAbsoluto($"{_paths.ImagenesTemporalesJugadorDNIFrenteAbsolute}/{dto.DNI}.jpg"));    
        }
        else
        {
            dto.FotoCarnet = ImagenUtility.AgregarMimeType(_imagenJugadorRepo.GetFotoCarnetEnBase64(dto.DNI));    
        }
        
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

    public async Task<int> Gestionar(GestionarJugadorDTO dto)
    {
        var jugadorAnterior = await Repo.ObtenerPorId(dto.Id);
        
        if (jugadorAnterior != null) { 
            var jugadorNuevo = Mapper.Map<Jugador>(jugadorAnterior);

            if (jugadorNuevo.DNI != dto.DNI)
            {
                jugadorNuevo.DNI = dto.DNI;
                _imagenJugadorRepo.RenombrarFotosTemporalesPorCambioDeDNI(jugadorAnterior.DNI, dto.DNI);
            }

            jugadorNuevo.Nombre = dto.Nombre;
            jugadorNuevo.Apellido = dto.Apellido;
            jugadorNuevo.FechaNacimiento = dto.FechaNacimiento;

            Repo.Modificar(jugadorAnterior, jugadorNuevo);
            Repo.CambiarEstado(dto.JugadorEquipoId, dto.Estado, dto.MotivoRechazo);
            await BDVirtual.GuardarCambios();
            
            if (dto.Estado == EstadoJugadorEnum.Activo)
                _imagenJugadorRepo.FicharJugadorTemporal(dto.DNI);
            
            return dto.JugadorEquipoId;
        }

        return -1;
    }

    public async Task<IEnumerable<JugadorDTO>> ListarConFiltro(IList<EstadoJugadorEnum> estados)
    {
        var jugadores = await Repo.ListarConFiltro(estados);
        var dtos = Mapper.Map<List<JugadorDTO>>(jugadores);
        return dtos;
    }
}