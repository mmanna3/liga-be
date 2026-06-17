using System.Security.Claims;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Entidades.EntidadesConValoresPredefinidos;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using Api.Persistencia._Config;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Servicios;

public class UsuarioCore : ABMCore<IUsuarioRepo, Usuario, UsuarioAdminDTO>, IUsuarioCore
{
    private static readonly int[] RolesAdmin = [(int)RolEnum.SuperAdministrador, (int)RolEnum.Administrador];

    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioCore(
        IBDVirtual bd,
        IUsuarioRepo repo,
        IMapper mapper,
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor) : base(bd, repo, mapper)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<RolDTO>> ListarRolesAsignables()
    {
        var rolIds = RolesAsignablesPorActor(ObtenerRolActor());
        var roles = await _context.Roles
            .Where(r => rolIds.Contains(r.Id))
            .OrderBy(r => r.Id)
            .ToListAsync();
        return Mapper.Map<List<RolDTO>>(roles);
    }

    public async Task<bool> BlanquearClave(int id)
    {
        var actorId = ObtenerUsuarioIdActor();
        if (actorId == id)
            throw new ExcepcionControlada("No podés blanquear tu propia clave.");

        var usuario = await Repo.ObtenerPorId(id);
        if (usuario == null)
            return false;

        ValidarActorPuedeGestionarUsuario(usuario);

        var usuarioTracked = await _context.Usuarios.FindAsync(id);
        if (usuarioTracked == null)
            return false;

        usuarioTracked.Password = null;
        await _context.SaveChangesAsync();
        return true;
    }

    public override async Task<int> Crear(UsuarioAdminDTO dto)
    {
        ValidarAccesosModulo(dto.AccesosModulo);
        var id = await base.Crear(dto);
        await SincronizarAccesosModulo(id, dto.AccesosModulo);
        await BDVirtual.GuardarCambios();
        return id;
    }

    public override async Task<int> Modificar(int id, UsuarioAdminDTO nuevo)
    {
        ValidarAccesosModulo(nuevo.AccesosModulo);
        var resultado = await base.Modificar(id, nuevo);
        await SincronizarAccesosModulo(id, nuevo.AccesosModulo);
        await BDVirtual.GuardarCambios();
        return resultado;
    }

    protected override async Task<Usuario> AntesDeCrear(UsuarioAdminDTO dto, Usuario entidad)
    {
        dto.NombreUsuario = NormalizarNombreUsuario(dto.NombreUsuario);
        ValidarNombreUsuario(dto.NombreUsuario);

        if (await Repo.ExisteNombreUsuario(dto.NombreUsuario))
            throw new ExcepcionControlada("El nombre de usuario ya existe.");

        ValidarRolAsignable(dto.RolId);

        entidad.NombreUsuario = dto.NombreUsuario;
        entidad.RolId = dto.RolId;
        entidad.Password = null;
        entidad.DelegadoId = null;

        return entidad;
    }

    protected override async Task<Usuario> AntesDeModificar(
        int id,
        UsuarioAdminDTO dto,
        Usuario entidadAnterior,
        Usuario entidadNueva)
    {
        dto.NombreUsuario = NormalizarNombreUsuario(dto.NombreUsuario);
        ValidarNombreUsuario(dto.NombreUsuario);

        if (await Repo.ExisteNombreUsuarioExceptoId(dto.NombreUsuario, id))
            throw new ExcepcionControlada("El nombre de usuario ya existe.");

        ValidarActorPuedeGestionarUsuario(entidadAnterior);
        ValidarRolAsignable(dto.RolId);

        entidadNueva.NombreUsuario = dto.NombreUsuario;
        entidadNueva.RolId = dto.RolId;
        entidadNueva.Password = entidadAnterior.Password;
        entidadNueva.DelegadoId = entidadAnterior.DelegadoId;

        return entidadNueva;
    }

    protected override async Task AntesDeEliminar(int id, Usuario entidad)
    {
        var actorId = ObtenerUsuarioIdActor();
        if (actorId == id)
            throw new ExcepcionControlada("No podés eliminarte a vos mismo.");

        ValidarActorPuedeGestionarUsuario(entidad);

        if (RolesAdmin.Contains(entidad.RolId))
        {
            var cantidad = await Repo.ContarUsuariosConRoles(RolesAdmin);
            if (cantidad <= 1)
                throw new ExcepcionControlada("No se puede eliminar el último administrador del sistema.");
        }
    }

    private void ValidarActorPuedeGestionarUsuario(Usuario usuario)
    {
        if (usuario.RolId == (int)RolEnum.SuperAdministrador && ObtenerRolActor() != "SuperAdministrador")
            throw new ExcepcionControlada("No tenés permisos para gestionar este usuario.");
    }

    private void ValidarRolAsignable(int rolId)
    {
        if (rolId == (int)RolEnum.Delegado)
            throw new ExcepcionControlada("No se puede asignar el rol Delegado desde este módulo.");

        var rolesAsignables = RolesAsignablesPorActor(ObtenerRolActor());
        if (!rolesAsignables.Contains(rolId))
            throw new ExcepcionControlada("No tenés permisos para asignar ese rol.");
    }

    private static int[] RolesAsignablesPorActor(string rolActor) =>
        rolActor switch
        {
            "SuperAdministrador" =>
            [
                (int)RolEnum.SuperAdministrador,
                (int)RolEnum.Administrador,
                (int)RolEnum.Usuario
            ],
            "Administrador" =>
            [
                (int)RolEnum.Administrador,
                (int)RolEnum.Usuario
            ],
            _ => []
        };

    private static string NormalizarNombreUsuario(string nombreUsuario) =>
        nombreUsuario.Trim().ToLowerInvariant();

    private static void ValidarNombreUsuario(string nombreUsuario)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario))
            throw new ExcepcionControlada("El nombre de usuario es obligatorio.");

        if (nombreUsuario.Length > 14)
            throw new ExcepcionControlada("El nombre de usuario no puede superar los 14 caracteres.");
    }

    private string ObtenerRolActor()
    {
        var rol = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(rol))
            throw new ExcepcionControlada("No se pudo determinar el rol del usuario autenticado.");
        return rol;
    }

    private int ObtenerUsuarioIdActor()
    {
        var idClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out var id))
            throw new ExcepcionControlada("No se pudo determinar el usuario autenticado.");
        return id;
    }

    private static void ValidarAccesosModulo(List<UsuarioAccesoModuloDTO> accesos)
    {
        if (accesos.Count == 0)
            return;

        var modulos = accesos.Select(a => a.Modulo).ToList();
        if (modulos.Distinct().Count() != modulos.Count)
            throw new ExcepcionControlada("No se puede asignar más de un nivel de acceso por módulo.");

        if (accesos.Any(a => !Enum.IsDefined(typeof(ModuloSistema), a.Modulo)))
            throw new ExcepcionControlada("Módulo de acceso inválido.");

        if (accesos.Any(a => !Enum.IsDefined(typeof(NivelAcceso), a.Nivel)))
            throw new ExcepcionControlada("Nivel de acceso inválido.");
    }

    private async Task SincronizarAccesosModulo(int usuarioId, List<UsuarioAccesoModuloDTO> accesos)
    {
        var existentes = await _context.UsuarioAccesoModulo
            .Where(a => a.UsuarioId == usuarioId)
            .ToListAsync();
        _context.UsuarioAccesoModulo.RemoveRange(existentes);

        foreach (var acceso in accesos)
        {
            _context.UsuarioAccesoModulo.Add(new UsuarioAccesoModulo
            {
                Id = 0,
                UsuarioId = usuarioId,
                Modulo = acceso.Modulo,
                Nivel = acceso.Nivel
            });
        }
    }
}
