using System.Security.Claims;
using Api.Core.DTOs;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Api.Core.Servicios;

public class PermisoServicio : IPermisoServicio
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private IReadOnlyList<UsuarioAccesoModuloDTO>? _permisosCache;

    public PermisoServicio(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool EsSuperAdministrador() =>
        _httpContextAccessor.HttpContext?.User.IsInRole("SuperAdministrador") == true;

    public NivelAcceso? ObtenerNivel(ModuloSistema modulo)
    {
        if (EsSuperAdministrador())
            return NivelAcceso.ControlTotal;

        return ObtenerPermisosDelUsuario()
            .FirstOrDefault(p => p.Modulo == modulo)
            ?.Nivel;
    }

    public bool TieneAcceso(ModuloSistema modulo) => ObtenerNivel(modulo) != null;

    public bool PuedeEditar(ModuloSistema modulo)
    {
        var nivel = ObtenerNivel(modulo);
        return nivel is NivelAcceso.Edicion or NivelAcceso.ControlTotal;
    }

    public bool PuedeEliminar(ModuloSistema modulo) =>
        ObtenerNivel(modulo) == NivelAcceso.ControlTotal;

    public IReadOnlyList<UsuarioAccesoModuloDTO> ObtenerPermisosDelUsuario()
    {
        if (_permisosCache != null)
            return _permisosCache;

        if (EsSuperAdministrador())
        {
            _permisosCache = [];
            return _permisosCache;
        }

        var claim = _httpContextAccessor.HttpContext?.User.FindFirstValue(PermisosClaimHelper.ClaimType);
        _permisosCache = PermisosClaimHelper.Deserializar(claim);
        return _permisosCache;
    }
}
