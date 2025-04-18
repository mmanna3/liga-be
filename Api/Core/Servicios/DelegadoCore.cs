using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;
using System.Text;
using Api.Persistencia._Config;

namespace Api.Core.Servicios;

public class DelegadoCore : ABMCore<IDelegadoRepo, Delegado, DelegadoDTO>, IDelegadoCore
{
    private readonly AppDbContext _context;

    public DelegadoCore(IBDVirtual bd, IDelegadoRepo repo, IMapper mapper, AppDbContext context) : base(bd, repo, mapper)
    {
        _context = context;
    }

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

    protected override Task<Delegado> AntesDeModificar(int id, DelegadoDTO dto, Delegado entidadAnterior, Delegado entidadNueva)
    {
        entidadNueva.Usuario = null!;
        entidadNueva.UsuarioId = entidadAnterior.UsuarioId;
        return Task.FromResult(entidadNueva);
    }
    
    protected override async Task<Delegado> AntesDeCrear(DelegadoDTO dto, Delegado entidad)
    {
        var nombreUsuario = ObtenerNombreUsuario(dto);
        
        var usuario = new Usuario
        {
            Id = 0,
            NombreUsuario = nombreUsuario,
            Password = null
        };
        
        await _context.Usuarios.AddAsync(usuario);
        
        entidad.Usuario = usuario;
        
        return entidad;
    }

    private static string ObtenerNombreUsuario(DelegadoDTO dto)
    {
        // Generar el nombre de usuario tomando la primera letra del nombre y el apellido completo
        var nombreNormalizado = NormalizarTexto(dto.Nombre);
        var apellidoNormalizado = NormalizarTexto(dto.Apellido);
        
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

        if (delegado != null)
        {
            delegado.Usuario.Password = null;
            _context.Update(delegado);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
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