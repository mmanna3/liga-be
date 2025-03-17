using Api.Core.DTOs;
using Api.Core.Entidades;
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

    public DelegadoCore(IBDVirtual bd, IDelegadoRepo repo, IMapper mapper, AppDbContext context) : base(bd, repo, mapper)
    {
        _context = context;
    }

    private string NormalizarTexto(string texto)
    {
        // Eliminar espacios
        texto = texto.Replace(" ", "");
        
        // Normalizar caracteres con tilde
        var normalizedString = texto.Normalize(NormalizationForm.FormD);
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

    protected override async Task<Delegado> AntesDeCrear(DelegadoDTO dto, Delegado entidad)
    {
        // Generar el nombre de usuario tomando la primera letra del nombre y el apellido completo
        var nombreNormalizado = NormalizarTexto(dto.Nombre);
        var apellidoNormalizado = NormalizarTexto(dto.Apellido);
        var nombreUsuario = nombreNormalizado[0] + apellidoNormalizado;
        
        // Crear el usuario asociado
        var usuario = new Usuario
        {
            Id = 0, // Este valor será ignorado por EF Core al hacer el insert
            NombreUsuario = nombreUsuario,
            Password = null // El password se deja en null como se solicitó
        };
        
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        
        // Asignar el usuario al delegado
        entidad.UsuarioId = usuario.Id;
        
        return entidad;
    }

    protected override async Task<int> AntesDeModificar(int id, DelegadoDTO dto, Delegado entidadAnterior, Delegado entidadNueva)
    {
        // Mantener el mismo UsuarioId que tenía antes
        entidadNueva.UsuarioId = entidadAnterior.UsuarioId;
        
        // Actualizar el nombre de usuario
        var nombreNormalizado = NormalizarTexto(dto.Nombre);
        var apellidoNormalizado = NormalizarTexto(dto.Apellido);
        var nombreUsuario = nombreNormalizado[0] + apellidoNormalizado;

        var usuario = await _context.Usuarios.FindAsync(entidadAnterior.UsuarioId);
        if (usuario != null)
        {
            usuario.NombreUsuario = nombreUsuario;
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }

        return id;
    }
}