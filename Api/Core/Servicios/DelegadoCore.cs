using Api.Core.DTOs;
using Api.Core.DTOs.CambiosDeEstadoDelegado;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Logica;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;
using System.Text;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Servicios;

public class DelegadoCore : ABMCore<IDelegadoRepo, Delegado, DelegadoDTO>, IDelegadoCore
{
    private readonly AppDbContext _context;
    private readonly IImagenDelegadoRepo _imagenDelegadoRepo;
    private readonly IJugadorRepo _jugadorRepo;
    private readonly IUsuarioRepo _usuarioRepo;
    private readonly AppPaths _paths;

    public DelegadoCore(IBDVirtual bd, IDelegadoRepo repo, IMapper mapper, AppDbContext context, IImagenDelegadoRepo imagenDelegadoRepo, IJugadorRepo jugadorRepo, IUsuarioRepo usuarioRepo, AppPaths paths) : base(bd, repo, mapper)
    {
        _context = context;
        _imagenDelegadoRepo = imagenDelegadoRepo;
        _jugadorRepo = jugadorRepo;
        _usuarioRepo = usuarioRepo;
        _paths = paths;
    }

    private static string QuitarCaracteresNoNumericos(string dni) => new string(dni.Where(char.IsDigit).ToArray());

    private static string NormalizarTexto(string texto)
    {
        texto = EliminarEspacios(texto);
        
        var normalizedString = NormalizarLetrasConTilde(texto);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
    }

    private static string NormalizarLetrasConTilde(string texto)
    {
        var normalizedString = texto.Normalize(NormalizationForm.FormD);
        return normalizedString;
    }

    private static string EliminarEspacios(string texto)
    {
        return texto.Replace(" ", "");
    }

    public override async Task<IEnumerable<DelegadoDTO>> Listar()
    {
        var delegadosConJugador = await Repo.ListarConJugadorIds();
        var dtos = delegadosConJugador.Select(x =>
        {
            var dto = Mapper.Map<DelegadoDTO>(x.Delegado);
            dto.JugadorId = x.JugadorId;
            return dto;
        }).ToList();
        return dtos;
    }

    public async Task<int> Aprobar(AprobarDelegadoDTO dto)
    {
        var delegado = await Repo.ObtenerPorId(dto.Id);
        if (delegado == null)
            return -1;

        if (delegado.EstadoDelegadoId != (int)EstadoDelegadoEnum.PendienteDeAprobacion)
            throw new ExcepcionControlada("Solo se pueden aprobar delegados pendientes de aprobación");

        var usuario = await CrearUsuarioParaElDelegado(delegado.Nombre, delegado.Apellido);
        delegado.UsuarioId = usuario.Id;
        delegado.Usuario = usuario;
        delegado.EstadoDelegadoId = (int)EstadoDelegadoEnum.Activo;
        delegado.EstadoDelegado = null!; // EF usa la navegación si está cargada; al limpiarla usa el FK EstadoDelegadoId

        _imagenDelegadoRepo.FicharPersonaTemporal(delegado.DNI);

        Repo.Modificar(delegado, delegado);
        await BDVirtual.GuardarCambios();
        return delegado.Id;
    }

    public override async Task<DelegadoDTO> ObtenerPorId(int id)
    {
        var dto = await base.ObtenerPorId(id);
        dto.JugadorId = await Repo.ObtenerJugadorIdPorDNI(dto.DNI);
        return dto;
    }

    protected override DelegadoDTO AntesDeObtenerPorId(Delegado entidad, DelegadoDTO dto)
    {
        dto.FotoCarnet = ImagenUtility.AgregarMimeType(_imagenDelegadoRepo.GetFotoCarnetEnBase64(dto.DNI));
        dto.FotoDNIFrente = ImagenUtility.AgregarMimeType(
            _imagenDelegadoRepo.GetFotoEnBase64ConPathAbsoluto($"{_paths.ImagenesTemporalesDNIFrenteAbsolute}/{dto.DNI}.jpg"));
        dto.FotoDNIDorso = ImagenUtility.AgregarMimeType(
            _imagenDelegadoRepo.GetFotoEnBase64ConPathAbsoluto($"{_paths.ImagenesTemporalesDNIDorsoAbsolute}/{dto.DNI}.jpg"));
        return dto;
    }

    protected override Task<Delegado> AntesDeModificar(int id, DelegadoDTO dto, Delegado entidadAnterior, Delegado entidadNueva)
    {
        entidadNueva.Usuario = entidadAnterior.Usuario;
        entidadNueva.UsuarioId = entidadAnterior.UsuarioId;
        entidadNueva.EstadoDelegadoId = entidadAnterior.EstadoDelegadoId;
        return Task.FromResult(entidadNueva);
    }
    
    protected override async Task<Delegado> AntesDeCrear(DelegadoDTO dto, Delegado entidad)
    {
        if (string.IsNullOrEmpty(dto.FotoCarnet) || string.IsNullOrEmpty(dto.FotoDNIFrente) || string.IsNullOrEmpty(dto.FotoDNIDorso))
            throw new ExcepcionControlada("Las fotos de carnet, DNI frente y DNI dorso son obligatorias");

        dto.DNI = QuitarCaracteresNoNumericos(dto.DNI);

        var delegadoExistente = await Repo.ObtenerPorDNI(dto.DNI);
        if (delegadoExistente != null)
        {
            if (PersonaExisteHelper.DelegadoEstaPendiente(delegadoExistente))
                throw new ExcepcionControlada("El DNI está pendiente de aprobación como delegado. La administración debe aprobarlo antes de poder fichar.");
            if (PersonaExisteHelper.DelegadoExiste(delegadoExistente))
                throw new ExcepcionControlada("DNI ya existente en el sistema. Probá ficharte desde el flujo 'Solo con DNI'.");
            throw new ExcepcionControlada("DNI ya existente como delegado en el sistema.");
        }

        var jugadorExistente = await _jugadorRepo.ObtenerPorDNI(dto.DNI);
        if (PersonaExisteHelper.JugadorExiste(jugadorExistente))
            throw new ExcepcionControlada("DNI ya existente en el sistema. Probá ficharte desde el flujo 'Solo con DNI'.");

        entidad.DNI = dto.DNI;
        entidad.ClubId = dto.ClubId;
        entidad.EstadoDelegadoId = (int)EstadoDelegadoEnum.PendienteDeAprobacion;

        _imagenDelegadoRepo.GuardarFotosTemporalesDePersonaFichada(dto.DNI, dto);

        return entidad;
    }

    private async Task<Usuario> CrearUsuarioParaElDelegado(string nombre, string apellido)
    {
        var nombreUsuario = await ObtenerNombreUsuarioDisponible(nombre, apellido);
        var usuario = new Usuario
        {
            Id = 0,
            NombreUsuario = nombreUsuario,
            Password = null,
            RolId = (int)RolEnum.Delegado
        };

        await _context.Usuarios.AddAsync(usuario);
        await BDVirtual.GuardarCambios();
        return usuario;
    }

    public async Task<string> ObtenerNombreUsuarioDisponible(string nombre, string apellido)
    {
        return await GeneradorNombreUsuario.ObtenerDisponible(nombre, apellido, _usuarioRepo.ExisteNombreUsuario);
    }

    public async Task<bool> BlanquearClave(int id)
    {
        var delegado = await Repo.ObtenerPorId(id);
        if (delegado?.UsuarioId == null)
            return false;

        var usuario = await _context.Usuarios.FindAsync(delegado.UsuarioId);
        if (usuario == null)
            return false;

        usuario.Password = null;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> FicharDelegadoSoloConDniYClub(FicharDelegadoSoloConDniYClubDTO dto)
    {
        var dni = QuitarCaracteresNoNumericos(dto.DNI);

        var delegadoExistente = await Repo.ObtenerPorDNI(dni);
        if (PersonaExisteHelper.DelegadoEstaPendiente(delegadoExistente))
            throw new ExcepcionControlada("El DNI está pendiente de aprobación como delegado. La administración debe aprobarlo antes de poder fichar.");

        var jugadorExistente = await _jugadorRepo.ObtenerPorDNI(dni);
        if (PersonaExisteHelper.JugadorEstaPendiente(jugadorExistente))
            throw new ExcepcionControlada("El DNI está pendiente de aprobación como jugador. La administración debe aprobarlo antes de poder fichar como delegado.");

        if (PersonaExisteHelper.DelegadoExiste(delegadoExistente))
        {
            return await FicharDesdeDelegadoExistente(delegadoExistente!, dto.ClubId);
        }

        if (PersonaExisteHelper.JugadorExiste(jugadorExistente))
        {
            return await FicharDesdeJugadorExistente(jugadorExistente!, dto);
        }

        throw new ExcepcionControlada("No existe ni un delegado ni un jugador con el DNI indicado.");
    }

    private async Task<int> FicharDesdeDelegadoExistente(Delegado delegadoExistente, int clubId)
    {
        _imagenDelegadoRepo.CopiarFotosDeDelegadoExistenteATemporales(delegadoExistente.DNI);

        var delegado = await _context.Delegados.FindAsync(delegadoExistente.Id);
        if (delegado == null)
            throw new InvalidOperationException("Delegado no encontrado");
        delegado.ClubId = clubId;
        delegado.EstadoDelegadoId = (int)EstadoDelegadoEnum.PendienteDeAprobacion;
        await BDVirtual.GuardarCambios();
        return delegado.Id;
    }

    private async Task<int> FicharDesdeJugadorExistente(Jugador jugador, FicharDelegadoSoloConDniYClubDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.TelefonoCelular))
            throw new ExcepcionControlada("Email y Teléfono son requeridos.");

        _imagenDelegadoRepo.CopiarFotosDeJugadorATemporales(jugador.DNI);

        var nuevoDelegado = new Delegado
        {
            Id = 0,
            DNI = jugador.DNI,
            Nombre = jugador.Nombre,
            Apellido = jugador.Apellido,
            FechaNacimiento = jugador.FechaNacimiento,
            TelefonoCelular = dto.TelefonoCelular,
            Email = dto.Email,
            ClubId = dto.ClubId,
            EstadoDelegadoId = (int)EstadoDelegadoEnum.PendienteDeAprobacion
        };

        Repo.Crear(nuevoDelegado);
        await BDVirtual.GuardarCambios();
        return nuevoDelegado.Id;
    }
    
    public async Task<int> Eliminar(int id)
    {
        var delegado = await Repo.ObtenerPorId(id);
        if (delegado == null)
            return -1;

        _imagenDelegadoRepo.EliminarTodasLasFotos(delegado.DNI);

        Repo.Eliminar(delegado);
        await BDVirtual.GuardarCambios();

        return id;
    }
}