using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Entidades.EntidadesConValoresPredefinidos;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Core.Servicios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Api.TestsUnitarios;

public class AuthCorePermisosTests
{
    [Fact]
    public async Task Login_UsuarioConAccesos_DevuelvePermisosEnResponse()
    {
        await using var context = CrearContexto();
        var usuario = await context.Usuarios
            .Include(u => u.Rol)
            .FirstAsync(u => u.NombreUsuario == "permuser");

        var authCore = CrearAuthCore(context);
        var response = await authCore.Login(new LoginDTO { Usuario = "permuser", Password = "clave123" });

        Assert.True(response.Exito);
        Assert.Single(response.Permisos);
        Assert.Equal(ModuloSistema.Torneos, response.Permisos[0].Modulo);
        Assert.Equal(NivelAcceso.Edicion, response.Permisos[0].Nivel);
    }

    [Fact]
    public async Task Login_UsuarioSinAccesos_DevuelveListaVacia()
    {
        await using var context = CrearContexto();
        var authCore = CrearAuthCore(context);

        var response = await authCore.Login(new LoginDTO { Usuario = "sinperm", Password = "clave123" });

        Assert.True(response.Exito);
        Assert.Empty(response.Permisos);
    }

    private static AuthCore CrearAuthCore(AppDbContext context)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AppSettings:Token"] = new string('x', 64) })
            .Build();
        var accessor = new Microsoft.AspNetCore.Http.HttpContextAccessor();
        var permisoServicio = new PermisoServicio(accessor);
        return new AuthCore(context, config, permisoServicio);
    }

    private static AppDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);

        context.Roles.AddRange(
            new Rol { Id = 2, Nombre = "Usuario" });
        context.Usuarios.AddRange(
            new Usuario
            {
                Id = 1,
                NombreUsuario = "permuser",
                Password = AuthCore.HashPassword("clave123"),
                RolId = 2
            },
            new Usuario
            {
                Id = 2,
                NombreUsuario = "sinperm",
                Password = AuthCore.HashPassword("clave123"),
                RolId = 2
            });
        context.SaveChanges();

        context.UsuarioAccesoModulo.Add(new UsuarioAccesoModulo
        {
            Id = 1,
            UsuarioId = 1,
            Modulo = ModuloSistema.Torneos,
            Nivel = NivelAcceso.Edicion
        });
        context.SaveChanges();

        return context;
    }
}
