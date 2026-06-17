using Api.Core.DTOs;
using Api.Core.Entidades.EntidadesConValoresPredefinidos;
using System.Security.Claims;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios;
using Api.Persistencia._Config;
using Api.Persistencia.Repositorios;
using Api.TestsUtilidades;
using Api._Config;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Api.TestsUnitarios;

public class UsuarioCorePermisosTests
{
    [Fact]
    public async Task Crear_ConAccesosModuloValidos_PersisteAccesos()
    {
        await using var context = CrearContexto();
        var core = CrearCore(context, rolActor: "SuperAdministrador");

        var id = await core.Crear(new UsuarioAdminDTO
        {
            NombreUsuario = "nuevo.user",
            RolId = (int)RolEnum.Usuario,
            AccesosModulo = PermisosDePrueba.AccesoSolo(ModuloSistema.Torneos, NivelAcceso.Edicion)
        });

        var accesos = await context.UsuarioAccesoModulo.Where(a => a.UsuarioId == id).ToListAsync();
        Assert.Single(accesos);
        Assert.Equal(ModuloSistema.Torneos, accesos[0].Modulo);
        Assert.Equal(NivelAcceso.Edicion, accesos[0].Nivel);
    }

    [Fact]
    public async Task Crear_ModuloDuplicadoEnDto_ExcepcionControlada()
    {
        await using var context = CrearContexto();
        var core = CrearCore(context, rolActor: "SuperAdministrador");

        var dto = new UsuarioAdminDTO
        {
            NombreUsuario = "dup.user",
            RolId = (int)RolEnum.Usuario,
            AccesosModulo =
            [
                new UsuarioAccesoModuloDTO { Modulo = ModuloSistema.Torneos, Nivel = NivelAcceso.Edicion },
                new UsuarioAccesoModuloDTO { Modulo = ModuloSistema.Torneos, Nivel = NivelAcceso.ControlTotal }
            ]
        };

        await Assert.ThrowsAsync<ExcepcionControlada>(() => core.Crear(dto));
    }

    private static UsuarioCore CrearCore(AppDbContext context, string rolActor)
    {
        var bd = new Mock<IBDVirtual>();
        bd.Setup(b => b.GuardarCambios()).Returns(() => context.SaveChangesAsync());

        var repo = new UsuarioRepo(context);
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MapperConfig>()).CreateMapper();

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Role, rolActor),
                new Claim(ClaimTypes.NameIdentifier, "1")
            ], "Test"))
        };
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);

        return new UsuarioCore(bd.Object, repo, mapper, context, accessor.Object);
    }

    private static AppDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);
        context.Roles.AddRange(
            new Rol { Id = 0, Nombre = "SuperAdministrador" },
            new Rol { Id = 2, Nombre = "Usuario" });
        context.SaveChanges();
        return context;
    }
}
