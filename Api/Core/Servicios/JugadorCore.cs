using Api.Core.DTOs;
using Api.Core.DTOs.CambiosDeEstadoJugador;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Logica;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

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
        if (string.IsNullOrEmpty(dto.FotoCarnet) || string.IsNullOrEmpty(dto.FotoDNIFrente) || string.IsNullOrEmpty(dto.FotoDNIDorso))
            throw new ExcepcionControlada("Las fotos de carnet, DNI frente y DNI dorso son obligatorias");

        dto.EquipoInicialId = GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos(dto.CodigoAlfanumerico);
        dto.DNI = QuitarCaracteresNoNumericos(dto.DNI);

        var jugadorExistente = await Repo.ObtenerPorDNI(dto.DNI);
        if (jugadorExistente != null && jugadorExistente.JugadorEquipos.Any(je => je.EstadoJugadorId != (int)EstadoJugadorEnum.FichajeRechazado))
            throw new ExcepcionControlada("Ya existe un jugador activo con este DNI");
        
        var resultado = await FicharJugadorEnElEquipo(dto.EquipoInicialId, entidad);
        
        Repo.SiElDNISeHabiaFichadoYEstaRechazadoEliminarJugador(entidad.DNI);
        await BDVirtual.GuardarCambios();
        _imagenJugadorRepo.GuardarFotosTemporalesDeJugadorAutofichado(dto);
        
        return resultado;
    }

    private static string QuitarCaracteresNoNumericos(string dni)
    {
        return new string(dni.Where(char.IsDigit).ToArray());
    }

    public override async Task<JugadorDTO> ObtenerPorId(int id)
    {
        var dto = await base.ObtenerPorId(id);
        dto.DelegadoId = await Repo.ObtenerDelegadoIdPorDNI(dto.DNI);
        return dto;
    }

    protected override JugadorDTO AntesDeObtenerPorId(Jugador entidad, JugadorDTO dto)
    {
        dto.FotoCarnet = ImagenUtility.AgregarMimeType(_imagenJugadorRepo.GetFotoCarnetEnBase64(dto.DNI));

        dto.FotoDNIDorso = ImagenUtility.AgregarMimeType(
            _imagenJugadorRepo.GetFotoEnBase64ConPathAbsoluto(
                    $"{_paths.ImagenesTemporalesDNIDorsoAbsolute}/{dto.DNI}.jpg"));

        dto.FotoDNIFrente = ImagenUtility.AgregarMimeType(
            _imagenJugadorRepo.GetFotoEnBase64ConPathAbsoluto(
                $"{_paths.ImagenesTemporalesDNIFrenteAbsolute}/{dto.DNI}.jpg"));

        return dto;
    }
    
    public async Task<Jugador> FicharJugadorEnElEquipo(int equipoId, Jugador jugador)
    {
        var equipo = await _equipoRepo.ObtenerPorId(equipoId);

        if (jugador.JugadorEquipos.Any(x => x.EquipoId == equipoId))
            throw new ExcepcionControlada("El jugador ya está fichado en el equipo");
        
        if (equipo == null)
            throw new ExcepcionControlada("El equipo no existe");

        var jugadorEquipo = new JugadorEquipo
        {
            Id = 0,
            EquipoId = equipoId,
            FechaFichaje = DateTime.Now,
            EstadoJugadorId = (int)EstadoJugadorEnum.FichajePendienteDeAprobacion 
        };
        
        jugador.JugadorEquipos.Add(jugadorEquipo);
        return jugador;
    }

    public async Task<int> DesvincularJugadorDelEquipo(DesvincularJugadorDelEquipoDTO dto)
    {
        var jugador = await Repo.ObtenerPorIdParaEliminar(dto.JugadorId);
        if (jugador == null)
            return -1;

        if (jugador.JugadorEquipos.Count == 1)
            return await Eliminar(dto.JugadorId);

        var jugadorEquipoId = jugador.JugadorEquipos.Single(x => x.EquipoId == dto.EquipoId).Id;
        Repo.EliminarJugadorEquipo(jugadorEquipoId);
        await BDVirtual.GuardarCambios();
        return dto.JugadorId;
    }

    public async Task<IEnumerable<JugadorDTO>> ListarConFiltro(IList<EstadoJugadorEnum> estados)
    {
        var jugadoresConDelegado = await Repo.ListarConFiltroConDelegadoIds(estados);

        var dtos = new List<JugadorDTO>();
        foreach (var (jug, delegadoId) in jugadoresConDelegado)
        {
            foreach (var jugEquipo in jug.JugadorEquipos)
            {
                var jugadorConUnSoloEquipo = jugEquipo.Jugador;
                jugadorConUnSoloEquipo.JugadorEquipos = new List<JugadorEquipo> { jugEquipo };
                var dto = Mapper.Map<JugadorDTO>(jugadorConUnSoloEquipo);
                dto.DelegadoId = delegadoId;
                dtos.Add(dto);
            }
        }

        return dtos;
    }

    public async Task<int> Eliminar(int id)
    {
        var jugador = await Repo.ObtenerPorIdParaEliminar(id);
        if (jugador == null)
            return -1;
        
        foreach (var jugadorEquipo in jugador.JugadorEquipos.ToList())
            Repo.EliminarJugadorEquipo(jugadorEquipo.Id);
        
        Repo.Eliminar(jugador);
        await BDVirtual.GuardarCambios();
        
        _imagenJugadorRepo.EliminarFotosDelJugador(jugador.DNI);
        
        return id;
    }

    public async Task<int> Aprobar(AprobarJugadorDTO dto)
    {
        var jugador = await Repo.ObtenerPorId(dto.Id);
        
        if (jugador != null) { 
            ModicarDatosBase(dto, jugador);
            Repo.CambiarEstado(dto.JugadorEquipoId, EstadoJugadorEnum.AprobadoPendienteDePago);                        
            
            if (EsElPrimerEquipoEnElQueSeFicha(jugador))
                _imagenJugadorRepo.FicharJugadorTemporal(dto.DNI);
            
            await BDVirtual.GuardarCambios();
            
            return dto.JugadorEquipoId;
        }

        return -1;
    }

    private static bool EsElPrimerEquipoEnElQueSeFicha(Jugador jugador)
    {
        return jugador.JugadorEquipos.Count == 1;
    }

    public async Task<int> Rechazar(RechazarJugadorDTO dto)
    {
        var jugador = await Repo.ObtenerPorId(dto.Id);
        
        if (jugador != null) { 
            ModicarDatosBase(dto, jugador);
            Repo.CambiarEstado(dto.JugadorEquipoId, EstadoJugadorEnum.FichajeRechazado, dto.Motivo);
            await BDVirtual.GuardarCambios();
            
            return dto.JugadorEquipoId;
        }

        return -1;
    }

    private void ModicarDatosBase(JugadorBaseDTO dto, Jugador jugadorAnterior)
    {
        // Create a new Jugador instance with only the necessary properties
        var jugadorNuevo = new Jugador
        {
            Id = jugadorAnterior.Id,
            DNI = dto.DNI,
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            FechaNacimiento = dto.FechaNacimiento
        };

        if (jugadorNuevo.DNI != dto.DNI)
        {
            jugadorNuevo.DNI = dto.DNI;
            _imagenJugadorRepo.RenombrarFotosTemporalesPorCambioDeDNI(jugadorAnterior.DNI, dto.DNI);
        }

        Repo.Modificar(jugadorAnterior, jugadorNuevo);
    }

    public async Task<int> Activar(List<CambiarEstadoDelJugadorDTO> dtos)
    {
        foreach (var dto in dtos) await CambiarEstado(dto, EstadoJugadorEnum.Activo);
        return dtos.Count;
    }
    
    public async Task<int> EfectuarPases(List<EfectuarPaseDTO> dtos)
    {
        foreach (var dto in dtos)
        {
            // Usamos una entidad trackeada para que EF persista las modificaciones en la colección JugadorEquipos
            var jugador = await Repo.ObtenerPorIdParaEliminar(dto.JugadorId);
            if (jugador == null) continue;

            if (jugador.JugadorEquipos.Any(je => je.EquipoId == dto.EquipoDestinoId))
                throw new ExcepcionControlada("El jugador ya juega en el equipo destino.");

            if (await Repo.JugadorYaJuegaEnTorneoDelEquipoDestino(dto.JugadorId, dto.EquipoOrigenId, dto.EquipoDestinoId))
                throw new ExcepcionControlada("El jugador ya juega en otro equipo del mismo torneo.");

            var jugadorEquipoOrigen = jugador.JugadorEquipos.Single(x => x.EquipoId == dto.EquipoOrigenId);

            var jugadorEquipoDestino = new JugadorEquipo
            {
                Id = 0,
                EquipoId = dto.EquipoDestinoId,
                FechaFichaje = DateTime.Now,
                EstadoJugadorId = (int)EstadoJugadorEnum.AprobadoPendienteDePago
            };

            jugador.JugadorEquipos.Add(jugadorEquipoDestino);
            Repo.EliminarJugadorEquipo(jugadorEquipoOrigen.Id);

            await BDVirtual.GuardarCambios();
        }
        await BDVirtual.GuardarCambios();
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