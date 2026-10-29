using System.Net;
using System.Net.Http.Json;
using Api.Core.Enums;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;

namespace Api.TestsDeIntegracion;

public class PermisosDelegadoIT : TestBase
{
    public PermisosDelegadoIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private async Task<HttpClient> GetClientDelegado()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await AuthTestHelper.CrearUsuarioYAutenticar(
            Factory.CreateClient(), context, "delegado.permisos", "clave123", RolEnum.Delegado);
    }

    private static HttpRequestMessage Request(string metodo, string url) =>
        new(new HttpMethod(metodo), url)
        {
            Content = metodo is "POST" or "PUT" ? JsonContent.Create(new { }) : null
        };

    [Theory]
    // ABM base
    [InlineData("DELETE", "/api/Club/1")]
    [InlineData("DELETE", "/api/Equipo/1")]
    [InlineData("DELETE", "/api/Jugador/1")]
    [InlineData("DELETE", "/api/Delegado/1")]
    [InlineData("DELETE", "/api/Arbitro/1")]
    [InlineData("DELETE", "/api/Torneo/1")]
    [InlineData("DELETE", "/api/TorneoAgrupador/1")]
    [InlineData("DELETE", "/api/Configuracion/1")]
    [InlineData("DELETE", "/api/DniExpulsadoDeLaLiga/1")]
    [InlineData("DELETE", "/api/FixtureAlgoritmo/1")]
    [InlineData("DELETE", "/api/SponsorWebPublica/1")]
    [InlineData("GET", "/api/Club")]
    [InlineData("POST", "/api/SponsorWebPublica")]
    // Endpoints propios de cada controller ABM
    [InlineData("GET", "/api/Club/por-ids?ids=1")]
    [InlineData("POST", "/api/Club/1/cambiar-escudo")]
    [InlineData("GET", "/api/Equipo/equipos-para-zonas")]
    [InlineData("GET", "/api/Equipo/1/jugadores-que-solo-juegan-en-este-equipo")]
    [InlineData("POST", "/api/Jugador/aprobar-jugador")]
    [InlineData("POST", "/api/Jugador/pagar-fichaje-del-jugador")]
    [InlineData("GET", "/api/Jugador/listar-con-filtro")]
    [InlineData("POST", "/api/Jugador/actualizar-tarjetas")]
    [InlineData("POST", "/api/Delegado/aprobar-delegado-en-el-club")]
    [InlineData("POST", "/api/Delegado/blanquear-clave?delegadoId=1")]
    [InlineData("GET", "/api/Delegado/listar-delegados-con-filtro")]
    [InlineData("GET", "/api/Arbitro/asignacion-por-agrupador?agrupadorId=1")]
    [InlineData("GET", "/api/Arbitro/asignacion-historica-por-agrupador?agrupadorId=1")]
    [InlineData("PUT", "/api/Arbitro/jornada/1/arbitros")]
    [InlineData("PUT", "/api/Arbitro/jornada/1/arbitros/1/whatsapp-enviado")]
    [InlineData("GET", "/api/Torneo/filtrar")]
    [InlineData("PUT", "/api/Torneo/1/visibilidad-en-app")]
    [InlineData("GET", "/api/TorneoAgrupador/por-ids?ids=1")]
    [InlineData("PUT", "/api/Configuracion/cambiar-escudo-por-defecto")]
    [InlineData("GET", "/api/DniExpulsadoDeLaLiga/por-ids?ids=1")]
    [InlineData("GET", "/api/FixtureAlgoritmo/por-ids?ids=1")]
    // ABM anidados
    [InlineData("GET", "/api/Torneo/1/fases")]
    [InlineData("DELETE", "/api/Torneo/1/fases/1")]
    [InlineData("GET", "/api/Torneo/1/fases/1/categorias")]
    [InlineData("GET", "/api/Torneo/1/categorias")]
    [InlineData("GET", "/api/Torneo/1/grupos-de-fases")]
    [InlineData("GET", "/api/Fase/1/zonas")]
    [InlineData("GET", "/api/Zona/1/fechas")]
    [InlineData("GET", "/api/Zona/1/leyendas")]
    // Controllers que no son ABM
    [InlineData("GET", "/api/Usuario")]
    [InlineData("POST", "/api/Usuario")]
    [InlineData("GET", "/api/Backup/generar-backup-base-de-datos")]
    [InlineData("GET", "/api/Reporte/obtener-reporte-pagos")]
    [InlineData("GET", "/api/Logs/archivos")]
    [InlineData("GET", "/api/Auth/permisos")]
    public async Task Delegado_EndpointQueNoUsaLaApp_Devuelve403(string metodo, string url)
    {
        var client = await GetClientDelegado();

        var response = await client.SendAsync(Request(metodo, url));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("GET", "/api/carnet-digital/carnets?equipoId=999", HttpStatusCode.NoContent)]
    [InlineData("GET", "/api/carnet-digital/jugadores-pendientes?equipoId=999", HttpStatusCode.NoContent)]
    [InlineData("GET", "/api/carnet-digital/info-inicial-de-torneos", HttpStatusCode.OK)]
    [InlineData("GET", "/api/Configuracion/fichaje-esta-habilitado", HttpStatusCode.OK)]
    [InlineData("GET", "/api/publico/el-dni-esta-fichado?dni=12345678", HttpStatusCode.OK)]
    public async Task Delegado_EndpointQueUsaLaApp_NoDevuelve403(string metodo, string url, HttpStatusCode esperado)
    {
        var client = await GetClientDelegado();

        var response = await client.SendAsync(Request(metodo, url));

        Assert.Equal(esperado, response.StatusCode);
    }

    [Fact]
    public async Task Delegado_EfectuarPases_Devuelve200()
    {
        var client = await GetClientDelegado();

        var response = await client.PostAsJsonAsync("/api/Jugador/efectuar-pases", Array.Empty<object>());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("DELETE", "/api/Club/999999", HttpStatusCode.NotFound)]
    [InlineData("GET", "/api/Usuario", HttpStatusCode.OK)]
    [InlineData("GET", "/api/Jugador/listar-con-filtro", HttpStatusCode.OK)]
    [InlineData("GET", "/api/carnet-digital/carnets?equipoId=999", HttpStatusCode.NoContent)]
    public async Task Administrador_SigueTeniendoAcceso(string metodo, string url, HttpStatusCode esperado)
    {
        var client = await GetAuthenticatedClient();

        var response = await client.SendAsync(Request(metodo, url));

        Assert.Equal(esperado, response.StatusCode);
    }
}
