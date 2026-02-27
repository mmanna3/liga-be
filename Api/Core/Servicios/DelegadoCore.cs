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

namespace Api.Core.Servicios;

public class DelegadoCore : ABMCore<IDelegadoRepo, Delegado, DelegadoDTO>, IDelegadoCore
{
    private readonly AppDbContext _context;
    private readonly IImagenDelegadoRepo _imagenDelegadoRepo;
    private readonly AppPaths _paths;

    public DelegadoCore(IBDVirtual bd, IDelegadoRepo repo, IMapper mapper, AppDbContext context, IImagenDelegadoRepo imagenDelegadoRepo, AppPaths paths) : base(bd, repo, mapper)
    {
        _context = context;
        _imagenDelegadoRepo = imagenDelegadoRepo;
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

        _imagenDelegadoRepo.FicharPersonaTemporal(delegado.DNI);

        Repo.Modificar(delegado, delegado);
        await BDVirtual.GuardarCambios();
        return delegado.Id;
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
        if (delegadoExistente != null && delegadoExistente.EstadoDelegadoId != (int)EstadoDelegadoEnum.Rechazado)
            throw new ExcepcionControlada("Ya existe un delegado con este DNI");

        entidad.DNI = dto.DNI;
        entidad.EstadoDelegadoId = (int)EstadoDelegadoEnum.PendienteDeAprobacion;

        _imagenDelegadoRepo.GuardarFotosTemporalesDePersonaFichada(dto.DNI, dto);

        return entidad;
    }

    private async Task<Usuario> CrearUsuarioParaElDelegado(string nombre, string apellido)
    {
        var nombreUsuario = ObtenerNombreUsuario(nombre, apellido);
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

    private static string ObtenerNombreUsuario(string nombre, string apellido)
    {
        // Generar el nombre de usuario tomando la primera letra del nombre y el apellido completo
        var nombreNormalizado = NormalizarTexto(nombre);
        var apellidoNormalizado = NormalizarTexto(apellido);
        
        if (string.IsNullOrEmpty(nombreNormalizado) || string.IsNullOrEmpty(apellidoNormalizado))
        {
            throw new ArgumentException("El nombre y el apellido no pueden estar vacíos");
        }
        
        var nombreUsuario = nombreNormalizado[0] + apellidoNormalizado;
        return nombreUsuario;
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
    
    public async Task<int> Eliminar(int id)
    {
        var delegado = await Repo.ObtenerPorId(id);
        if (delegado == null)
            return -1;
        
        Repo.Eliminar(delegado);
        await BDVirtual.GuardarCambios();
        
        return id;
    }
}