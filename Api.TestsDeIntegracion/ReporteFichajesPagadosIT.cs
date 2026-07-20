using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion;

public class ReporteFichajesPagadosIT : TestBase
{
    public ReporteFichajesPagadosIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task ObtenerReporte_UnTorneoUnPago_CuentaCorrectamente()
    {
        var (torneoId, jugadorEquipoId) = await SeedTorneoConPago(
            nombreTorneo: "Torneo Test",
            anioTorneo: 2026,
            fechaPago: new DateTime(2026, 4, 15));

        var reporte = await ObtenerReporte(2026);

        var fila = Assert.Single(reporte.SelectMany(r => r.Torneos));
        Assert.Equal(torneoId, fila.TorneoId);
        Assert.Equal("Torneo Test", fila.NombreTorneo);
        Assert.Equal(1, fila.Abril);
        Assert.Equal(1, fila.TotalEnElAnio);
    }

    [Fact]
    public async Task ObtenerReporte_EquipoEnDosZonasDelMismoTorneo_CuentaUnaSolaVez()
    {
        int torneoId;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var club = new Club { Id = 0, Nombre = "Club Zonas" };
            context.Clubs.Add(club);
            context.SaveChanges();

            torneoId = CrearTorneoConDosZonas(context, "Torneo Multi Zona", 2026, club.Id, out var zona1Id, out var zona2Id);

            var equipo = new Equipo
            {
                Id = 0,
                Nombre = "Equipo Multi Zona",
                ClubId = club.Id,
                Jugadores = [],
                Zonas = []
            };
            context.Equipos.Add(equipo);
            context.SaveChanges();

            context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = equipo.Id, ZonaId = zona1Id });
            context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = equipo.Id, ZonaId = zona2Id });
            context.SaveChanges();

            var jugador = new Jugador
            {
                Id = 0,
                DNI = "99998888",
                Nombre = "Juan",
                Apellido = "Multi",
                FechaNacimiento = new DateTime(2010, 1, 1)
            };
            context.Jugadores.Add(jugador);
            context.SaveChanges();

            var je = new JugadorEquipo
            {
                Id = 0,
                JugadorId = jugador.Id,
                EquipoId = equipo.Id,
                FechaFichaje = DateTime.Now,
                EstadoJugadorId = (int)EstadoJugadorEnum.Activo
            };
            context.JugadorEquipo.Add(je);
            context.SaveChanges();

            context.HistorialDePagos.Add(new HistorialDePagos
            {
                JugadorEquipoId = je.Id,
                Fecha = new DateTime(2026, 4, 10)
            });
            context.SaveChanges();
        }

        var reporte = await ObtenerReporte(2026);
        var fila = Assert.Single(reporte.SelectMany(r => r.Torneos).Where(t => t.TorneoId == torneoId));

        Assert.Equal(1, fila.Abril);
        Assert.Equal(1, fila.TotalEnElAnio);
    }

    [Fact]
    public async Task ObtenerReporte_TorneoRenombradoEntreAnios_SoloCuentaTorneoDelAnioFiltrado()
    {
        int torneo2025Id;
        int torneo2026Id;

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var club = new Club { Id = 0, Nombre = "Club Renombrado" };
            context.Clubs.Add(club);
            context.SaveChanges();

            torneo2025Id = CrearTorneoConZona(context, "Fútbol 11 Infantiles", 2025, club.Id, out var zona2025Id);
            torneo2026Id = CrearTorneoConZona(context, "Futbol 11 Infantiles", 2026, club.Id, out var zona2026Id);

            CrearEquipoConPago(context, club.Id, zona2025Id, "11110001", new DateTime(2026, 4, 1));
            CrearEquipoConPago(context, club.Id, zona2026Id, "11110002", new DateTime(2026, 4, 1));
            var equipoEnAmbos = CrearEquipoConPago(context, club.Id, zona2026Id, "11110003", new DateTime(2026, 5, 1));

            context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = equipoEnAmbos.equipoId, ZonaId = zona2025Id });
            context.SaveChanges();
        }

        var reporte = await ObtenerReporte(2026);
        var torneos = reporte.SelectMany(r => r.Torneos).ToList();

        Assert.Single(torneos);
        var fila = torneos[0];
        Assert.Equal(torneo2026Id, fila.TorneoId);
        Assert.Equal("Futbol 11 Infantiles", fila.NombreTorneo);
        Assert.Equal(1, fila.Abril);
        Assert.Equal(1, fila.Mayo);
        Assert.Equal(2, fila.TotalEnElAnio);
        Assert.DoesNotContain(torneos, t => t.TorneoId == torneo2025Id);
    }

    [Fact]
    public async Task ObtenerReporte_PagoEn2026DeEquipoSoloEnTorneo2025_NoApareceEnReporte2026()
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var club = new Club { Id = 0, Nombre = "Club Legacy" };
            context.Clubs.Add(club);
            context.SaveChanges();

            CrearTorneoConZona(context, "Fútbol 11 Infantiles", 2025, club.Id, out var zona2025Id);
            CrearEquipoConPago(context, club.Id, zona2025Id, "11110004", new DateTime(2026, 4, 1));
        }

        var reporte = await ObtenerReporte(2026);
        Assert.Empty(reporte.SelectMany(r => r.Torneos));
    }

    [Fact]
    public async Task ObtenerReporte_VariosMeses_AcumulaPorMes()
    {
        int torneoId;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var club = new Club { Id = 0, Nombre = "Club Meses" };
            context.Clubs.Add(club);
            context.SaveChanges();

            torneoId = CrearTorneoConZona(context, "Torneo Meses", 2026, club.Id, out var zonaId);

            CrearEquipoConPago(context, club.Id, zonaId, "11110005", new DateTime(2026, 3, 5));
            CrearEquipoConPago(context, club.Id, zonaId, "11110006", new DateTime(2026, 4, 5));
            CrearEquipoConPago(context, club.Id, zonaId, "11110007", new DateTime(2026, 4, 20));
        }

        var reporte = await ObtenerReporte(2026);
        var fila = Assert.Single(reporte.SelectMany(r => r.Torneos).Where(t => t.TorneoId == torneoId));

        Assert.Equal(1, fila.Marzo);
        Assert.Equal(2, fila.Abril);
        Assert.Equal(3, fila.TotalEnElAnio);
    }

    private Task<(int torneoId, int jugadorEquipoId)> SeedTorneoConPago(
        string nombreTorneo,
        int anioTorneo,
        DateTime fechaPago)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var club = new Club { Id = 0, Nombre = "Club Reporte" };
        context.Clubs.Add(club);
        context.SaveChanges();

        var torneoId = CrearTorneoConZona(context, nombreTorneo, anioTorneo, club.Id, out var zonaId);
        var (_, jugadorEquipoId) = CrearEquipoConPago(context, club.Id, zonaId, "11119999", fechaPago);

        return Task.FromResult((torneoId, jugadorEquipoId));
    }

    private static int CrearTorneoConZona(
        AppDbContext context,
        string nombreTorneo,
        int anio,
        int clubId,
        out int zonaId)
    {
        return CrearTorneoConDosZonas(context, nombreTorneo, anio, clubId, out zonaId, out _);
    }

    private static int CrearTorneoConDosZonas(
        AppDbContext context,
        string nombreTorneo,
        int anio,
        int clubId,
        out int zona1Id,
        out int zona2Id)
    {
        _ = clubId;

        var torneo = new Torneo
        {
            Id = 0,
            Nombre = nombreTorneo,
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
            Nombre = "",
            TorneoId = torneo.Id,
            Numero = 1,
            EstadoFaseId = 100,
            EsVisibleEnApp = true
        };
        context.Fases.Add(fase);
        context.SaveChanges();

        var zona1 = new ZonaTodosContraTodos
        {
            Id = 0,
            FaseId = fase.Id,
            Nombre = "Zona A",
            Orden = 1
        };
        var zona2 = new ZonaTodosContraTodos
        {
            Id = 0,
            FaseId = fase.Id,
            Nombre = "Zona B",
            Orden = 2
        };
        context.Zonas.Add(zona1);
        context.Zonas.Add(zona2);
        context.SaveChanges();

        zona1Id = zona1.Id;
        zona2Id = zona2.Id;
        return torneo.Id;
    }

    private static (int equipoId, int jugadorEquipoId) CrearEquipoConPago(
        AppDbContext context,
        int clubId,
        int zonaId,
        string dni,
        DateTime fechaPago)
    {
        var equipo = new Equipo
        {
            Id = 0,
            Nombre = $"Equipo {dni}",
            ClubId = clubId,
            Jugadores = [],
            Zonas = []
        };
        context.Equipos.Add(equipo);
        context.SaveChanges();

        context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = equipo.Id, ZonaId = zonaId });
        context.SaveChanges();

        var jugador = new Jugador
        {
            Id = 0,
            DNI = dni,
            Nombre = "Jugador",
            Apellido = dni,
            FechaNacimiento = new DateTime(2010, 1, 1)
        };
        context.Jugadores.Add(jugador);
        context.SaveChanges();

        var je = new JugadorEquipo
        {
            Id = 0,
            JugadorId = jugador.Id,
            EquipoId = equipo.Id,
            FechaFichaje = DateTime.Now,
            EstadoJugadorId = (int)EstadoJugadorEnum.Activo
        };
        context.JugadorEquipo.Add(je);
        context.SaveChanges();

        context.HistorialDePagos.Add(new HistorialDePagos
        {
            JugadorEquipoId = je.Id,
            Fecha = fechaPago
        });
        context.SaveChanges();

        return (equipo.Id, je.Id);
    }

    private async Task<List<ReporteFichajesPagadosPorAgrupadorDeTorneoDTO>> ObtenerReporte(int anio)
    {
        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/reporte/obtener-reporte-fichajes-pagados-por-torneo?anio={anio}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var reporte = JsonConvert.DeserializeObject<List<ReporteFichajesPagadosPorAgrupadorDeTorneoDTO>>(json);
        Assert.NotNull(reporte);
        return reporte;
    }
}
