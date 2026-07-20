using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion;

public class ReporteJugadoresActivosIT : TestBase
{
    public ReporteJugadoresActivosIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task ObtenerReporte_UnTorneoUnActivo_CuentaCorrectamente()
    {
        var torneoId = await SeedTorneoConJugadorActivo("Torneo Activos", 2026, "20000001");

        var reporte = await ObtenerReporte(2026, mostrarEquipos: false);
        var fila = Assert.Single(reporte.SelectMany(r => r.Torneos));

        Assert.Equal(torneoId, fila.TorneoId);
        Assert.Equal("Torneo Activos", fila.NombreTorneo);
        Assert.Equal(1, fila.CantidadJugadoresActivos);
        Assert.Empty(fila.Equipos);
    }

    [Fact]
    public async Task ObtenerReporte_MostrarEquipos_IncluyeDesglosePorEquipo()
    {
        int torneoId;
        int equipo1Id;
        int equipo2Id;

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var club = new Club { Id = 0, Nombre = "Club Activos" };
            context.Clubs.Add(club);
            context.SaveChanges();

            torneoId = CrearTorneoConZona(context, "Torneo Con Equipos", 2026, out var zonaId);
            equipo1Id = CrearEquipoConJugadoresActivos(context, club.Id, zonaId, "Equipo Uno", ["20000011", "20000012"]).equipoId;
            equipo2Id = CrearEquipoConJugadoresActivos(context, club.Id, zonaId, "Equipo Dos", ["20000013"]).equipoId;
        }

        var reporte = await ObtenerReporte(2026, mostrarEquipos: true);
        var fila = Assert.Single(reporte.SelectMany(r => r.Torneos), t => t.TorneoId == torneoId);

        Assert.Equal(3, fila.CantidadJugadoresActivos);
        Assert.Equal(2, fila.Equipos.Count);

        var equipoUno = Assert.Single(fila.Equipos, e => e.EquipoId == equipo1Id);
        Assert.Equal("Equipo Uno", equipoUno.NombreEquipo);
        Assert.Equal(2, equipoUno.CantidadJugadoresActivos);

        var equipoDos = Assert.Single(fila.Equipos, e => e.EquipoId == equipo2Id);
        Assert.Equal(1, equipoDos.CantidadJugadoresActivos);
    }

    [Fact]
    public async Task ObtenerReporte_EquipoEnDosZonasDelMismoTorneo_CuentaUnaSolaVez()
    {
        int torneoId;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var club = new Club { Id = 0, Nombre = "Club Multi Zona" };
            context.Clubs.Add(club);
            context.SaveChanges();

            torneoId = CrearTorneoConDosZonas(context, "Torneo Multi Zona Activos", 2026, out var zona1Id, out var zona2Id);

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

            CrearJugadorActivoEnEquipo(context, equipo.Id, "20000021");
        }

        var reporte = await ObtenerReporte(2026, mostrarEquipos: true);
        var fila = Assert.Single(reporte.SelectMany(r => r.Torneos), t => t.TorneoId == torneoId);

        Assert.Equal(1, fila.CantidadJugadoresActivos);
        Assert.Equal(1, Assert.Single(fila.Equipos).CantidadJugadoresActivos);
    }

    [Fact]
    public async Task ObtenerReporte_SoloCuentaEstadoActivo_NoOtrosEstados()
    {
        int torneoId;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var club = new Club { Id = 0, Nombre = "Club Estados" };
            context.Clubs.Add(club);
            context.SaveChanges();

            torneoId = CrearTorneoConZona(context, "Torneo Estados", 2026, out var zonaId);
            var (equipoId, _) = CrearEquipoConJugadoresActivos(context, club.Id, zonaId, "Equipo Estados", ["20000031"]);

            CrearJugadorEnEquipo(context, equipoId, "20000032", EstadoJugadorEnum.Suspendido);
            CrearJugadorEnEquipo(context, equipoId, "20000033", EstadoJugadorEnum.Inhabilitado);
            CrearJugadorEnEquipo(context, equipoId, "20000034", EstadoJugadorEnum.AprobadoPendienteDePago);
        }

        var reporte = await ObtenerReporte(2026, mostrarEquipos: false);
        var fila = Assert.Single(reporte.SelectMany(r => r.Torneos), t => t.TorneoId == torneoId);
        Assert.Equal(1, fila.CantidadJugadoresActivos);
    }

    [Fact]
    public async Task ObtenerReporte_SoloTorneosDelAnioFiltrado()
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var club = new Club { Id = 0, Nombre = "Club Anios" };
            context.Clubs.Add(club);
            context.SaveChanges();

            var zona2025 = 0;
            var zona2026 = 0;
            CrearTorneoConZona(context, "Torneo 2025", 2025, out zona2025);
            CrearTorneoConZona(context, "Torneo 2026", 2026, out zona2026);

            CrearEquipoConJugadoresActivos(context, club.Id, zona2025, "Equipo 2025", ["20000041"]);
            CrearEquipoConJugadoresActivos(context, club.Id, zona2026, "Equipo 2026", ["20000042", "20000043"]);
        }

        var reporte = await ObtenerReporte(2026, mostrarEquipos: false);
        var fila = Assert.Single(reporte.SelectMany(r => r.Torneos));
        Assert.Equal("Torneo 2026", fila.NombreTorneo);
        Assert.Equal(2, fila.CantidadJugadoresActivos);
    }

    private async Task<int> SeedTorneoConJugadorActivo(string nombreTorneo, int anio, string dni)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var club = new Club { Id = 0, Nombre = "Club Seed" };
        context.Clubs.Add(club);
        context.SaveChanges();

        var torneoId = CrearTorneoConZona(context, nombreTorneo, anio, out var zonaId);
        CrearEquipoConJugadoresActivos(context, club.Id, zonaId, $"Equipo {dni}", [dni]);
        return torneoId;
    }

    private static int CrearTorneoConZona(AppDbContext context, string nombreTorneo, int anio, out int zonaId)
    {
        return CrearTorneoConDosZonas(context, nombreTorneo, anio, out zonaId, out _);
    }

    private static int CrearTorneoConDosZonas(
        AppDbContext context,
        string nombreTorneo,
        int anio,
        out int zona1Id,
        out int zona2Id)
    {
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

        var zona1 = new ZonaTodosContraTodos { Id = 0, FaseId = fase.Id, Nombre = "Zona A", Orden = 1 };
        var zona2 = new ZonaTodosContraTodos { Id = 0, FaseId = fase.Id, Nombre = "Zona B", Orden = 2 };
        context.Zonas.Add(zona1);
        context.Zonas.Add(zona2);
        context.SaveChanges();

        zona1Id = zona1.Id;
        zona2Id = zona2.Id;
        return torneo.Id;
    }

    private static (int equipoId, List<int> jugadorEquipoIds) CrearEquipoConJugadoresActivos(
        AppDbContext context,
        int clubId,
        int zonaId,
        string nombreEquipo,
        IEnumerable<string> dnis)
    {
        var equipo = new Equipo
        {
            Id = 0,
            Nombre = nombreEquipo,
            ClubId = clubId,
            Jugadores = [],
            Zonas = []
        };
        context.Equipos.Add(equipo);
        context.SaveChanges();

        context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = equipo.Id, ZonaId = zonaId });
        context.SaveChanges();

        var ids = dnis.Select(dni => CrearJugadorActivoEnEquipo(context, equipo.Id, dni)).ToList();
        return (equipo.Id, ids);
    }

    private static int CrearJugadorActivoEnEquipo(AppDbContext context, int equipoId, string dni)
    {
        return CrearJugadorEnEquipo(context, equipoId, dni, EstadoJugadorEnum.Activo);
    }

    private static int CrearJugadorEnEquipo(AppDbContext context, int equipoId, string dni, EstadoJugadorEnum estado)
    {
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
            EquipoId = equipoId,
            FechaFichaje = DateTime.Now,
            EstadoJugadorId = (int)estado
        };
        context.JugadorEquipo.Add(je);
        context.SaveChanges();
        return je.Id;
    }

    private async Task<List<ReporteJugadoresActivosPorAgrupadorDeTorneoDTO>> ObtenerReporte(int anio, bool mostrarEquipos)
    {
        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync(
            $"/api/reporte/obtener-reporte-jugadores-activos-por-torneo?anio={anio}&mostrarEquipos={mostrarEquipos}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var reporte = JsonConvert.DeserializeObject<List<ReporteJugadoresActivosPorAgrupadorDeTorneoDTO>>(json);
        Assert.NotNull(reporte);
        return reporte;
    }
}
