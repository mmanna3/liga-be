using System.Net;
using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Servicios;
using Api.Core.Servicios.Interfaces;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Api.TestsDeIntegracion;

public class LogsIT : TestBase
{
    public LogsIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Buscar_SinAuth_401()
    {
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/Logs/buscar?texto=30111222");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Buscar_ComoAdministrador_403()
    {
        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync("/api/Logs/buscar?texto=30111222");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Buscar_ComoSuperAdmin_200YEncuentraDni()
    {
        var logsDir = Path.Combine(Path.GetTempPath(), "liga-logs-it-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(logsDir);
        try
        {
            var hoy = DateTime.Today.ToString("yyyy-MM-dd");
            File.WriteAllText(
                Path.Combine(logsDir, $"nlog-all-{hoy}.log"),
                $"{hoy} 12:00:00.1234|ERROR|Api|Fallo con DNI 44556677{Environment.NewLine}");

            var factory = Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<ILogsCore>();
                    services.AddScoped<ILogsCore>(_ => new LogsCore(logsDir));
                });
            });

            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var client = factory.CreateClient();
            client = await CrearSuperAdminYAutenticar(client, context, "superlogs", "clave123");

            var response = await client.GetAsync("/api/Logs/buscar?texto=44556677&dias=7");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<BusquedaLogsDTO>();
            Assert.NotNull(body);
            Assert.Single(body.Resultados);
            Assert.Contains("44556677", body.Resultados[0].Contenido);
        }
        finally
        {
            if (Directory.Exists(logsDir))
                Directory.Delete(logsDir, recursive: true);
        }
    }

    [Fact]
    public async Task Buscar_TextoCorto_ComoSuperAdmin_400()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var client = Factory.CreateClient();
        client = await CrearSuperAdminYAutenticar(client, context, "superlogs400", "clave123");

        var response = await client.GetAsync("/api/Logs/buscar?texto=ab");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Archivos_ComoSuperAdmin_200()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var client = Factory.CreateClient();
        client = await CrearSuperAdminYAutenticar(client, context, "superlogsarch", "clave123");

        var response = await client.GetAsync("/api/Logs/archivos");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// SuperAdministrador tiene Id=0; EF trata RolId=0 como "unset" por HasDefaultValue(1),
    /// así que hay que forzar el rol por SQL tras el insert.
    /// </summary>
    private static async Task<HttpClient> CrearSuperAdminYAutenticar(
        HttpClient client,
        AppDbContext context,
        string nombreUsuario,
        string password)
    {
        if (!await context.Roles.AnyAsync(r => r.Id == (int)RolEnum.SuperAdministrador))
        {
            await context.Database.ExecuteSqlRawAsync(
                """INSERT INTO "_Rol" ("Id", "Nombre") VALUES (0, 'SuperAdministrador')""");
        }

        var usuario = new Usuario
        {
            Id = 0,
            NombreUsuario = nombreUsuario,
            Password = AuthCore.HashPassword(password),
            RolId = (int)RolEnum.Administrador,
            DelegadoId = null
        };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        await context.Database.ExecuteSqlRawAsync(
            """UPDATE "Usuarios" SET "RolId" = 0 WHERE "Id" = {0}""", usuario.Id);
        context.ChangeTracker.Clear();

        return await AuthTestHelper.GetAuthenticatedClient(client, nombreUsuario, password);
    }
}
