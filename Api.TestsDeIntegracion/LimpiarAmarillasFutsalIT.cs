using System.Net;
using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace Api.TestsDeIntegracion;

/// <summary>
/// Limpieza de tarjetas amarillas para torneos cuyo agrupador tiene PermiteLimpiarAmarillas (Futsal).
/// </summary>
public class LimpiarAmarillasFutsalIT : TestBase
{
    private const int AgrupadorFutsalId = 5001;
    private const int TorneoFutsalId = 5001;
    private const int TorneoNoFutsalId = 5002;

    // Fichas (JugadorEquipo) del torneo Futsal
    private const int JeConAmarillasA = 5001; // 3 amarillas, 1 roja
    private const int JeSinAmarillas = 5002;  // 0 amarillas
    private const int JeConAmarillasB = 5003; // 1 amarilla, 2 rojas
    // Ficha de un torneo distinto (no debe verse afectada)
    private const int JeOtroTorneo = 5004;    // 5 amarillas

    public LimpiarAmarillasFutsalIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedData(context);
    }

    private static void SeedData(AppDbContext context)
    {
        context.TorneoAgrupadores.Add(new TorneoAgrupador
        {
            Id = AgrupadorFutsalId,
            Nombre = "Futsal",
            EsVisibleEnApp = true,
            PermiteLimpiarAmarillas = true,
            ColorId = (int)ColorEnum.Negro
        });
        // El agrupador "General" (Id = 1, PermiteLimpiarAmarillas = false) ya existe por HasData.

        var club = new Club { Id = 5001, Nombre = "Club Futsal", Localidad = "Rosario" };
        var clubB = new Club { Id = 5002, Nombre = "Club Liga 11", Localidad = "Rosario" };
        context.Clubs.AddRange(club, clubB);

        context.Torneos.Add(new Torneo
        {
            Id = TorneoFutsalId,
            Nombre = "Futsal Apertura",
            Anio = 2026,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true,
            TorneoAgrupadorId = AgrupadorFutsalId
        });
        context.Torneos.Add(new Torneo
        {
            Id = TorneoNoFutsalId,
            Nombre = "Liga 11 Apertura",
            Anio = 2026,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true,
            TorneoAgrupadorId = 1
        });
        context.SaveChanges();

        var faseFutsal = new FaseTodosContraTodos
        {
            Id = 0, TorneoId = TorneoFutsalId, Nombre = "Fase única", Numero = 1,
            EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente, EsVisibleEnApp = true
        };
        var faseNoFutsal = new FaseTodosContraTodos
        {
            Id = 0, TorneoId = TorneoNoFutsalId, Nombre = "Fase única", Numero = 1,
            EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente, EsVisibleEnApp = true
        };
        context.Fases.AddRange(faseFutsal, faseNoFutsal);
        context.SaveChanges();

        var zonaFutsal = new ZonaTodosContraTodos { Id = 0, FaseId = faseFutsal.Id, Nombre = "Zona única", Orden = 1 };
        var zonaNoFutsal = new ZonaTodosContraTodos { Id = 0, FaseId = faseNoFutsal.Id, Nombre = "Zona única", Orden = 1 };
        context.Zonas.AddRange(zonaFutsal, zonaNoFutsal);
        context.SaveChanges();

        var equipoFutsal = new Equipo
        {
            Id = 5001, Nombre = "Los Amarillos", ClubId = club.Id,
            Jugadores = new List<JugadorEquipo>(), Zonas = new List<EquipoZona>()
        };
        var equipoNoFutsal = new Equipo
        {
            Id = 5002, Nombre = "Once Once", ClubId = clubB.Id,
            Jugadores = new List<JugadorEquipo>(), Zonas = new List<EquipoZona>()
        };
        context.Equipos.AddRange(equipoFutsal, equipoNoFutsal);
        context.SaveChanges();

        context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = equipoFutsal.Id, ZonaId = zonaFutsal.Id });
        context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = equipoNoFutsal.Id, ZonaId = zonaNoFutsal.Id });
        context.SaveChanges();

        context.Jugadores.AddRange(
            new Jugador { Id = 5001, DNI = "50000001", Nombre = "Ana", Apellido = "Zapata", FechaNacimiento = new DateTime(2000, 1, 1) },
            new Jugador { Id = 5002, DNI = "50000002", Nombre = "Beto", Apellido = "Ortiz", FechaNacimiento = new DateTime(2000, 2, 2) },
            new Jugador { Id = 5003, DNI = "50000003", Nombre = "Caro", Apellido = "Diaz", FechaNacimiento = new DateTime(2000, 3, 3) },
            new Jugador { Id = 5004, DNI = "50000004", Nombre = "Dani", Apellido = "Luna", FechaNacimiento = new DateTime(2000, 4, 4) });
        context.SaveChanges();

        context.JugadorEquipo.AddRange(
            new JugadorEquipo
            {
                Id = JeConAmarillasA, JugadorId = 5001, EquipoId = equipoFutsal.Id, FechaFichaje = DateTime.Now,
                EstadoJugadorId = (int)EstadoJugadorEnum.Activo, TarjetasAmarillas = 3, TarjetasRojas = 1
            },
            new JugadorEquipo
            {
                Id = JeSinAmarillas, JugadorId = 5002, EquipoId = equipoFutsal.Id, FechaFichaje = DateTime.Now,
                EstadoJugadorId = (int)EstadoJugadorEnum.Activo, TarjetasAmarillas = 0, TarjetasRojas = 0
            },
            new JugadorEquipo
            {
                Id = JeConAmarillasB, JugadorId = 5003, EquipoId = equipoFutsal.Id, FechaFichaje = DateTime.Now,
                EstadoJugadorId = (int)EstadoJugadorEnum.Activo, TarjetasAmarillas = 1, TarjetasRojas = 2
            },
            new JugadorEquipo
            {
                Id = JeOtroTorneo, JugadorId = 5004, EquipoId = equipoNoFutsal.Id, FechaFichaje = DateTime.Now,
                EstadoJugadorId = (int)EstadoJugadorEnum.Activo, TarjetasAmarillas = 5, TarjetasRojas = 0
            });
        context.SaveChanges();
    }

    [Fact]
    public async Task JugadoresConAmarillas_DevuelveSoloFichasConAmarillasDelTorneo()
    {
        var client = await GetAuthenticatedClient();

        var response = await client.GetAsync($"/api/torneo/{TorneoFutsalId}/jugadores-con-amarillas");
        response.EnsureSuccessStatusCode();

        var jugadores = JsonConvert.DeserializeObject<List<JugadorConAmarillasDTO>>(
            await response.Content.ReadAsStringAsync())!;

        Assert.Equal(2, jugadores.Count);
        Assert.All(jugadores, j => Assert.True(j.TarjetasAmarillas > 0));
        Assert.DoesNotContain(jugadores, j => j.JugadorEquipoId == JeSinAmarillas);
        Assert.DoesNotContain(jugadores, j => j.JugadorEquipoId == JeOtroTorneo);

        var ana = jugadores.Single(j => j.JugadorEquipoId == JeConAmarillasA);
        Assert.Equal("Ana Zapata", ana.NombreCompleto);
        Assert.Equal("50000001", ana.Dni);
        Assert.Equal(3, ana.TarjetasAmarillas);
        Assert.Equal("Los Amarillos", ana.NombreEquipo);
        Assert.Equal("Club Futsal", ana.NombreClub);
    }

    [Fact]
    public async Task LimpiarAmarillas_PoneEnCeroYRespetaRojasYOtrosTorneos()
    {
        var client = await GetAuthenticatedClient();

        var response = await client.PostAsync($"/api/torneo/{TorneoFutsalId}/limpiar-amarillas", null);
        response.EnsureSuccessStatusCode();

        var afectados = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
        Assert.Equal(2, afectados);

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var a = context.JugadorEquipo.Single(je => je.Id == JeConAmarillasA);
        Assert.Equal(0, a.TarjetasAmarillas);
        Assert.Equal(1, a.TarjetasRojas);

        var b = context.JugadorEquipo.Single(je => je.Id == JeConAmarillasB);
        Assert.Equal(0, b.TarjetasAmarillas);
        Assert.Equal(2, b.TarjetasRojas);

        var otroTorneo = context.JugadorEquipo.Single(je => je.Id == JeOtroTorneo);
        Assert.Equal(5, otroTorneo.TarjetasAmarillas);

        var listado = await client.GetAsync($"/api/torneo/{TorneoFutsalId}/jugadores-con-amarillas");
        listado.EnsureSuccessStatusCode();
        var jugadores = JsonConvert.DeserializeObject<List<JugadorConAmarillasDTO>>(
            await listado.Content.ReadAsStringAsync())!;
        Assert.Empty(jugadores);
    }

    [Fact]
    public async Task ObtenerTorneo_ExponePermiteLimpiarAmarillasSegunElAgrupador()
    {
        var client = await GetAuthenticatedClient();

        var futsal = JsonConvert.DeserializeObject<TorneoDTO>(
            await (await client.GetAsync($"/api/torneo/{TorneoFutsalId}")).Content.ReadAsStringAsync())!;
        var noFutsal = JsonConvert.DeserializeObject<TorneoDTO>(
            await (await client.GetAsync($"/api/torneo/{TorneoNoFutsalId}")).Content.ReadAsStringAsync())!;

        Assert.True(futsal.PermiteLimpiarAmarillas);
        Assert.False(noFutsal.PermiteLimpiarAmarillas);
    }

    [Fact]
    public async Task JugadoresConAmarillas_TorneoNoFutsal_Devuelve400()
    {
        var client = await GetAuthenticatedClient();

        var response = await client.GetAsync($"/api/torneo/{TorneoNoFutsalId}/jugadores-con-amarillas");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LimpiarAmarillas_TorneoNoFutsal_Devuelve400()
    {
        var client = await GetAuthenticatedClient();

        var response = await client.PostAsync($"/api/torneo/{TorneoNoFutsalId}/limpiar-amarillas", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LimpiarAmarillas_TorneoInexistente_Devuelve400()
    {
        var client = await GetAuthenticatedClient();

        var response = await client.PostAsync("/api/torneo/999999/limpiar-amarillas", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LimpiarAmarillas_UsuarioNoAdmin_Devuelve403()
    {
        var client = Factory.CreateClient();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var noAdmin = await AuthTestHelper.CrearUsuarioYAutenticar(
            client, context, "nofutsal", "clave123", RolEnum.Usuario, accesos: []);

        var response = await noAdmin.PostAsync($"/api/torneo/{TorneoFutsalId}/limpiar-amarillas", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        // Y no tocó nada.
        var a = context.JugadorEquipo.Single(je => je.Id == JeConAmarillasA);
        Assert.Equal(3, a.TarjetasAmarillas);
    }
}
