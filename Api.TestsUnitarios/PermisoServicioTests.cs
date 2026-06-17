using System.Security.Claims;
using Api.Core.DTOs;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Core.Servicios;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Api.TestsUnitarios;

public class PermisoServicioTests
{
    [Fact]
    public void EsSuperAdministrador_RolSuperAdmin_DevuelveTrue()
    {
        var servicio = CrearServicio(rol: "SuperAdministrador");

        Assert.True(servicio.EsSuperAdministrador());
        Assert.True(servicio.PuedeEliminar(ModuloSistema.Torneos));
    }

    [Fact]
    public void TieneAcceso_SinPermisosEnClaim_DevuelveFalse()
    {
        var servicio = CrearServicio(rol: "Usuario");

        Assert.False(servicio.TieneAcceso(ModuloSistema.Torneos));
    }

    [Fact]
    public void PuedeEditar_NivelEdicion_DevuelveTrue()
    {
        var servicio = CrearServicio(
            rol: "Usuario",
            permisos: [new UsuarioAccesoModuloDTO { Modulo = ModuloSistema.Torneos, Nivel = NivelAcceso.Edicion }]);

        Assert.True(servicio.PuedeEditar(ModuloSistema.Torneos));
        Assert.False(servicio.PuedeEliminar(ModuloSistema.Torneos));
    }

    [Fact]
    public void PuedeEliminar_NivelControlTotal_DevuelveTrue()
    {
        var servicio = CrearServicio(
            rol: "Usuario",
            permisos: [new UsuarioAccesoModuloDTO { Modulo = ModuloSistema.Clubes, Nivel = NivelAcceso.ControlTotal }]);

        Assert.True(servicio.PuedeEditar(ModuloSistema.Clubes));
        Assert.True(servicio.PuedeEliminar(ModuloSistema.Clubes));
    }

    [Fact]
    public void ObtenerNivel_ModuloInexistente_DevuelveNull()
    {
        var servicio = CrearServicio(
            rol: "Usuario",
            permisos: [new UsuarioAccesoModuloDTO { Modulo = ModuloSistema.Torneos, Nivel = NivelAcceso.Edicion }]);

        Assert.Null(servicio.ObtenerNivel(ModuloSistema.Reportes));
    }

    private static PermisoServicio CrearServicio(string rol, List<UsuarioAccesoModuloDTO>? permisos = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, rol)
        };

        if (permisos is { Count: > 0 })
            claims.Add(new Claim(PermisosClaimHelper.ClaimType, PermisosClaimHelper.Serializar(permisos)));

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = principal };
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(context);

        return new PermisoServicio(accessor.Object);
    }
}
