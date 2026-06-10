using System.Net;
using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Api.TestsUtilidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion;

public class ArbitroAsignacionIT : TestBase
{
    public ArbitroAsignacionIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private sealed record EscenarioAsignacion(
        int AgrupadorId,
        int Anio,
        int JornadaProximaId,
        int JornadaPasadaId,
        int Arbitro1Id,
        int Arbitro2Id,
        int ArbitroSinAgrupadorId);

    private async Task<EscenarioAsignacion> CrearEscenario()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var util = new Utilidades(context);

        var club = util.DadoQueExisteElClub();
        club.Localidad = "Rosario";
        context.SaveChanges();

        var eq1 = util.DadoQueExisteElEquipo(club);
        var eq2 = new Equipo { Id = 0, Nombre = "Visitante FC", ClubId = club.Id, Jugadores = [] };
        context.Equipos.Add(eq2);
        context.SaveChanges();

        var anio = DateTime.Today.Year;
        var torneo = new Torneo
        {
            Id = 0,
            Nombre = "Torneo Asignación Árbitros",
            Anio = anio,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };
        context.Torneos.Add(torneo);
        context.SaveChanges();

        var fase = new FaseTodosContraTodos
        {
            Id = 0,
            Nombre = "Apertura",
            Numero = 1,
            TorneoId = torneo.Id,
            EstadoFaseId = 100,
            EsVisibleEnApp = true
        };
        context.Fases.Add(fase);
        context.SaveChanges();

        var zona = new ZonaTodosContraTodos { Id = 0, FaseId = fase.Id, Nombre = "Zona única", Orden = 1 };
        context.Zonas.Add(zona);
        context.SaveChanges();

        context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = eq1.Id, ZonaId = zona.Id });
        context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = eq2.Id, ZonaId = zona.Id });
        context.SaveChanges();

        var diaPasado = DateOnly.FromDateTime(DateTime.Today.AddDays(-7));
        var diaProximo = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var diaPosterior = DateOnly.FromDateTime(DateTime.Today.AddDays(8));

        var fechaPasada = new FechaTodosContraTodos
        {
            Id = 0,
            Dia = diaPasado,
            Numero = 1,
            ZonaId = zona.Id,
            EsVisibleEnApp = true
        };
        var fechaProxima = new FechaTodosContraTodos
        {
            Id = 0,
            Dia = diaProximo,
            Numero = 2,
            ZonaId = zona.Id,
            EsVisibleEnApp = true
        };
        var fechaPosterior = new FechaTodosContraTodos
        {
            Id = 0,
            Dia = diaPosterior,
            Numero = 3,
            ZonaId = zona.Id,
            EsVisibleEnApp = true
        };
        context.Fechas.AddRange(fechaPasada, fechaProxima, fechaPosterior);
        context.SaveChanges();

        var jornadaPasada = new JornadaNormal
        {
            Id = 0,
            FechaId = fechaPasada.Id,
            ResultadosVerificados = true,
            LocalEquipoId = eq1.Id,
            VisitanteEquipoId = eq2.Id,
            Partidos = []
        };
        var jornadaProxima = new JornadaNormal
        {
            Id = 0,
            FechaId = fechaProxima.Id,
            ResultadosVerificados = false,
            LocalEquipoId = eq1.Id,
            VisitanteEquipoId = eq2.Id,
            Partidos = []
        };
        var jornadaPosterior = new JornadaNormal
        {
            Id = 0,
            FechaId = fechaPosterior.Id,
            ResultadosVerificados = false,
            LocalEquipoId = eq1.Id,
            VisitanteEquipoId = eq2.Id,
            Partidos = []
        };
        context.Jornadas.AddRange(jornadaPasada, jornadaProxima, jornadaPosterior);
        context.SaveChanges();

        var arbitro1 = new Arbitro { Id = 0, DNI = "30111222", Nombre = "Juan", Apellido = "Uno" };
        var arbitro2 = new Arbitro { Id = 0, DNI = "30222333", Nombre = "Pedro", Apellido = "Dos" };
        var arbitroSinAgrupador = new Arbitro { Id = 0, DNI = "30333444", Nombre = "Luis", Apellido = "Tres" };
        context.Arbitros.AddRange(arbitro1, arbitro2, arbitroSinAgrupador);
        context.SaveChanges();

        context.ArbitroTorneoAgrupador.AddRange(
            new ArbitroTorneoAgrupador { Id = 0, ArbitroId = arbitro1.Id, TorneoAgrupadorId = 1 },
            new ArbitroTorneoAgrupador { Id = 0, ArbitroId = arbitro2.Id, TorneoAgrupadorId = 1 });
        context.SaveChanges();

        return new EscenarioAsignacion(
            1,
            anio,
            jornadaProxima.Id,
            jornadaPasada.Id,
            arbitro1.Id,
            arbitro2.Id,
            arbitroSinAgrupador.Id);
    }

    [Fact]
    public async Task ObtenerAsignacionPorAgrupador_DevuelveProximaFechaPorCalendario()
    {
        var escenario = await CrearEscenario();
        var client = await GetAuthenticatedClient();

        var response = await client.GetAsync(
            $"/api/arbitro/asignacion-por-agrupador?agrupadorId={escenario.AgrupadorId}&anio={escenario.Anio}");

        response.EnsureSuccessStatusCode();
        var content = JsonConvert.DeserializeObject<AsignacionArbitrosPorAgrupadorDTO>(
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);

        var torneo = Assert.Single(content.Torneos);
        var fase = Assert.Single(torneo.Fases);
        var zona = Assert.Single(fase.Zonas);
        Assert.NotNull(zona.ProximaFecha);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), zona.ProximaFecha.Dia);
        Assert.Equal(2, zona.ProximaFecha.Numero);

        var jornada = Assert.Single(zona.ProximaFecha.Jornadas);
        Assert.Equal(escenario.JornadaProximaId, jornada.Id);
        Assert.Equal("Rosario", jornada.LocalidadLocal);
        Assert.Equal(2, content.ArbitrosElegibles.Count);
    }

    [Fact]
    public async Task AsignarArbitros_DosArbitros_204()
    {
        var escenario = await CrearEscenario();
        var client = await GetAuthenticatedClient();

        var response = await client.PutAsJsonAsync(
            $"/api/arbitro/jornada/{escenario.JornadaProximaId}/arbitros",
            new AsignarArbitrosJornadaDTO { ArbitroIds = [escenario.Arbitro1Id, escenario.Arbitro2Id] });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var asignaciones = await context.ArbitroJornada
            .Where(a => a.JornadaId == escenario.JornadaProximaId)
            .OrderBy(a => a.Orden)
            .ToListAsync();
        Assert.Equal(2, asignaciones.Count);
        Assert.Equal(escenario.Arbitro1Id, asignaciones[0].ArbitroId);
        Assert.Equal(1, asignaciones[0].Orden);
        Assert.Equal(escenario.Arbitro2Id, asignaciones[1].ArbitroId);
        Assert.Equal(2, asignaciones[1].Orden);
    }

    [Fact]
    public async Task AsignarArbitros_SinArbitros_EliminaAsignaciones()
    {
        var escenario = await CrearEscenario();
        var client = await GetAuthenticatedClient();

        await client.PutAsJsonAsync(
            $"/api/arbitro/jornada/{escenario.JornadaProximaId}/arbitros",
            new AsignarArbitrosJornadaDTO { ArbitroIds = [escenario.Arbitro1Id] });

        var response = await client.PutAsJsonAsync(
            $"/api/arbitro/jornada/{escenario.JornadaProximaId}/arbitros",
            new AsignarArbitrosJornadaDTO { ArbitroIds = [] });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Empty(await context.ArbitroJornada.Where(a => a.JornadaId == escenario.JornadaProximaId).ToListAsync());
    }

    [Fact]
    public async Task AsignarArbitros_TresArbitros_400()
    {
        var escenario = await CrearEscenario();
        var client = await GetAuthenticatedClient();

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var arbitro3 = new Arbitro { Id = 0, DNI = "30444555", Nombre = "Ana", Apellido = "Cuatro" };
        context.Arbitros.Add(arbitro3);
        context.SaveChanges();
        context.ArbitroTorneoAgrupador.Add(new ArbitroTorneoAgrupador
        {
            Id = 0,
            ArbitroId = arbitro3.Id,
            TorneoAgrupadorId = 1
        });
        context.SaveChanges();

        var response = await client.PutAsJsonAsync(
            $"/api/arbitro/jornada/{escenario.JornadaProximaId}/arbitros",
            new AsignarArbitrosJornadaDTO
            {
                ArbitroIds = [escenario.Arbitro1Id, escenario.Arbitro2Id, arbitro3.Id]
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AsignarArbitros_ArbitroFueraDelAgrupador_400()
    {
        var escenario = await CrearEscenario();
        var client = await GetAuthenticatedClient();

        var response = await client.PutAsJsonAsync(
            $"/api/arbitro/jornada/{escenario.JornadaProximaId}/arbitros",
            new AsignarArbitrosJornadaDTO { ArbitroIds = [escenario.ArbitroSinAgrupadorId] });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AsignacionEnJornadaPasada_PersisteComoHistorico()
    {
        var escenario = await CrearEscenario();
        var client = await GetAuthenticatedClient();

        await client.PutAsJsonAsync(
            $"/api/arbitro/jornada/{escenario.JornadaPasadaId}/arbitros",
            new AsignarArbitrosJornadaDTO { ArbitroIds = [escenario.Arbitro1Id] });

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var historico = await context.ArbitroJornada
            .SingleAsync(a => a.JornadaId == escenario.JornadaPasadaId);
        Assert.Equal(escenario.Arbitro1Id, historico.ArbitroId);

        var getResponse = await client.GetAsync(
            $"/api/arbitro/asignacion-por-agrupador?agrupadorId={escenario.AgrupadorId}&anio={escenario.Anio}");
        getResponse.EnsureSuccessStatusCode();
        var content = JsonConvert.DeserializeObject<AsignacionArbitrosPorAgrupadorDTO>(
            await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(content);

        var arbitro = content.ArbitrosConJornadas.Single(a => a.ArbitroId == escenario.Arbitro1Id);
        Assert.Empty(arbitro.JornadasProximaFecha);
    }

    [Fact]
    public async Task ObtenerAsignacion_VistaPorArbitro_MuestraJornadasAsignadas()
    {
        var escenario = await CrearEscenario();
        var client = await GetAuthenticatedClient();

        await client.PutAsJsonAsync(
            $"/api/arbitro/jornada/{escenario.JornadaProximaId}/arbitros",
            new AsignarArbitrosJornadaDTO { ArbitroIds = [escenario.Arbitro1Id] });

        var response = await client.GetAsync(
            $"/api/arbitro/asignacion-por-agrupador?agrupadorId={escenario.AgrupadorId}&anio={escenario.Anio}");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<AsignacionArbitrosPorAgrupadorDTO>(
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);

        var arbitroConJornada = content.ArbitrosConJornadas.Single(a => a.ArbitroId == escenario.Arbitro1Id);
        var jornada = Assert.Single(arbitroConJornada.JornadasProximaFecha);
        Assert.Equal(escenario.JornadaProximaId, jornada.JornadaId);

        var arbitroSinJornada = content.ArbitrosConJornadas.Single(a => a.ArbitroId == escenario.Arbitro2Id);
        Assert.Empty(arbitroSinJornada.JornadasProximaFecha);
    }
}
