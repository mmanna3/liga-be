using Api.Core.DTOs;
using Api.Core.DTOs.CambiosDeEstadoJugador;
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
    private readonly IHistorialDePagosRepo _historialDePagosRepo;

    public JugadorCore(IBDVirtual bd, IJugadorRepo repo, IMapper mapper, IEquipoRepo equipoRepo, IImagenJugadorRepo imagenJugadorRepo, AppPaths paths, IHistorialDePagosRepo historialDePagosRepo) : base(bd, repo, mapper)
    {
        _equipoRepo = equipoRepo;
        _imagenJugadorRepo = imagenJugadorRepo;
        _paths = paths;
        _historialDePagosRepo = historialDePagosRepo;
    }
    
    protected override async Task<Jugador> AntesDeCrear(JugadorDTO dto, Jugador entidad)
    {
        dto.EquipoInicialId = GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos(dto.CodigoAlfanumerico);
        
        var resultado = await MapearEquipoInicial(dto, entidad);
        
        Repo.SiElDNISeHabiaFichadoYEstaRechazadoEliminarJugador(entidad.DNI);
        await BDVirtual.GuardarCambios();
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

    public async Task<IEnumerable<JugadorDTO>> ListarConFiltro(IList<EstadoJugadorEnum> estados)
    {
        var jugadores = await Repo.ListarConFiltro(estados);
        var dtos = Mapper.Map<List<JugadorDTO>>(jugadores);
        return dtos;
    }

    public async Task<int> Eliminar(int id)
    {
        var jugador = await Repo.ObtenerPorId(id);
        if (jugador == null)
            return -1;

        // Eliminar las imágenes asociadas al jugador
        _imagenJugadorRepo.EliminarFotosDelJugador(jugador.DNI);
        
        // Eliminar el jugador y sus relaciones
        Repo.Eliminar(jugador);
        await BDVirtual.GuardarCambios();
        
        return id;
    }

    public async Task<int> Aprobar(AprobarJugadorDTO dto)
    {
        var jugadorAnterior = await Repo.ObtenerPorId(dto.Id);
        
        if (jugadorAnterior != null) { 
            ModicarDatosBase(dto, jugadorAnterior);
            Repo.CambiarEstado(dto.JugadorEquipoId, EstadoJugadorEnum.AprobadoPendienteDePago);
            await BDVirtual.GuardarCambios();
            _imagenJugadorRepo.FicharJugadorTemporal(dto.DNI);
            
            return dto.JugadorEquipoId;
        }

        return -1;
    }

    public async Task<int> Rechazar(RechazarJugadorDTO dto)
    {
        var jugadorAnterior = await Repo.ObtenerPorId(dto.Id);
        
        if (jugadorAnterior != null) { 
            ModicarDatosBase(dto, jugadorAnterior);
            Repo.CambiarEstado(dto.JugadorEquipoId, EstadoJugadorEnum.FichajeRechazado, dto.Motivo);
            await BDVirtual.GuardarCambios();
            
            return dto.JugadorEquipoId;
        }

        return -1;
    }

    private void ModicarDatosBase(JugadorBaseDTO dto, Jugador jugadorAnterior)
    {
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
    }

    public async Task<int> Activar(List<CambiarEstadoDelJugadorDTO> dtos)
    {
        foreach (var dto in dtos) await CambiarEstado(dto, EstadoJugadorEnum.Activo);
        return dtos.Count;
    }

    private async Task CambiarEstado(CambiarEstadoDelJugadorDTO dto, EstadoJugadorEnum estado)
    {
           var jugadorAnterior = await Repo.ObtenerPorId(dto.JugadorId);
            if (jugadorAnterior != null) { 
                Repo.CambiarEstado(dto.JugadorEquipoId, estado, dto.Motivo);
                await BDVirtual.GuardarCambios();
            }
    }

    public async Task<int> Suspender(List<CambiarEstadoDelJugadorDTO> dtos)
    {
        foreach (var dto in dtos) await CambiarEstado(dto, EstadoJugadorEnum.Suspendido);
        await BDVirtual.GuardarCambios();
        return dtos.Count;
    }

    public async Task<int> Inhabilitar(List<CambiarEstadoDelJugadorDTO> dtos)
    {
        foreach (var dto in dtos) await CambiarEstado(dto, EstadoJugadorEnum.Inhabilitado);
        await BDVirtual.GuardarCambios();
        return dtos.Count;
    }

    public async Task<int> PagarFichaje(CambiarEstadoDelJugadorDTO dto)
    {
        var jugadorAnterior = await Repo.ObtenerPorId(dto.JugadorId);
        if (jugadorAnterior != null) { 
            Repo.CambiarEstado(dto.JugadorEquipoId, EstadoJugadorEnum.Activo, dto.Motivo);
            
            await _historialDePagosRepo.RegistrarPago(dto.JugadorEquipoId);
            
            await BDVirtual.GuardarCambios();
            
            return dto.JugadorEquipoId;
        }

        return -1;
    }
}