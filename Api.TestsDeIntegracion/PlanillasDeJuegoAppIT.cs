using System.Net.Http.Json;
using Api.Core.DTOs.AppCarnetDigital;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Logica;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Api.TestsUtilidades;
using Microsoft.Extensions.DependencyInjection;

namespace Api.TestsDeIntegracion;

/// <summary>
/// Integración de <c>GET /api/carnet-digital/planillas-de-juego</c>: un jugador debe figurar en cada categoría
/// cuyo rango de años de nacimiento incluye su año (pueden solaparse varias categorías).
/// </summary>
public class PlanillasDeJuegoAppIT : TestBase
{
    private readonly string _codigoAlfanumericoEquipo;

    public PlanillasDeJuegoAppIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var util = new Utilidades(context);

        var club = util.DadoQueExisteElClub();
        context.SaveChanges();

        var equipo = util.DadoQueExisteElEquipo(club);
        context.SaveChanges();

        var anioActual = DateTime.Today.Year;
        var torneo = new Torneo
        {
            Id = 0,
            Nombre = "Torneo planillas categorías solapadas",
            Anio = anioActual,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };
        context.Torneos.Add(torneo);
        context.SaveChanges();

        var fase = new FaseTodosContraTodos
        {
            Id = 0,
            Nombre = "",
            Numero = 1,
            TorneoId = torneo.Id,
            EstadoFaseId = 100,
            EsVisibleEnApp = true
        };
        context.Fases.Add(fase);
        context.SaveChanges();

        var zona = new ZonaTodosContraTodos { Id = 0, FaseId = fase.Id, Nombre = "Zona planillas" };
        context.Zonas.Add(zona);
        context.SaveChanges();

        // Mismo ejemplo que el dominio: 1992 entra en 1991-1993, 1990-1994 y 1992-1992.
        context.TorneoCategorias.AddRange(
            new TorneoCategoria
            {
                Id = 0,
                Nombre = "Cat 1991-1993",
                AnioDesde = 1991,
                AnioHasta = 1993,
                TorneoId = torneo.Id
            },
            new TorneoCategoria
            {
                Id = 0,
                Nombre = "Cat 1990-1994",
                AnioDesde = 1990,
                AnioHasta = 1994,
                TorneoId = torneo.Id
            },
            new TorneoCategoria
            {
                Id = 0,
                Nombre = "Cat 1992",
                AnioDesde = 1992,
                AnioHasta = 1992,
                TorneoId = torneo.Id
            });
        context.SaveChanges();

        context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = equipo!.Id, ZonaId = zona.Id });

        var jugador = new Jugador
        {
            Id = 0,
            Nombre = "Ana",
            Apellido = "Solapada",
            DNI = "20991992",
            FechaNacimiento = new DateTime(1992, 7, 20)
        };
        context.Jugadores.Add(jugador);
        context.SaveChanges();

        context.JugadorEquipo.Add(new JugadorEquipo
        {
            Id = 0,
            JugadorId = jugador.Id,
            EquipoId = equipo.Id,
            EstadoJugadorId = (int)EstadoJugadorEnum.Activo,
            FechaFichaje = DateTime.UtcNow
        });
        context.SaveChanges();

        _codigoAlfanumericoEquipo = GeneradorDeHash.GenerarAlfanumerico7Digitos(equipo.Id);
    }

    [Fact]
    public async Task PlanillasDeJuego_JugadorEnVariasCategoriasSolapadas_ApareceEnTodasLasPlanillas()
    {
        var client = await GetAuthenticatedClient();
        var url =
            $"/api/carnet-digital/planillas-de-juego?codigoAlfanumerico={Uri.EscapeDataString(_codigoAlfanumericoEquipo)}";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<PlanillaDeJuegoDTO>();
        Assert.NotNull(dto);
        Assert.NotNull(dto.Planillas);
        Assert.Equal(3, dto.Planillas.Count);

        foreach (var planilla in dto.Planillas)
        {
            Assert.Contains(planilla.Jugadores, j => j.DNI == "20991992" && j.Nombre.Contains("Ana", StringComparison.Ordinal));
        }
    }
}
