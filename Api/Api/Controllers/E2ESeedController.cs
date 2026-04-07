using Api.Core.Entidades;
using Api.Core.Entidades.EntidadesConValoresPredefinidos;
using Api.Core.Enums;
using Api.Core.Logica;
using Api.Core.Servicios;
using Api.Persistencia._Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

namespace Api.Api.Controllers;

/// <summary>
/// Endpoints de utilidad para tests E2E contra backend real.
/// Solo disponibles cuando la variable de entorno E2E_SEED_ENABLED=true.
/// </summary>
[Route("api/e2e")]
[ApiController]
[AllowAnonymous]
public class E2ESeedController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly AppPaths _paths;

    public E2ESeedController(AppDbContext context, AppPaths paths)
    {
        _context = context;
        _paths = paths;
    }

    /// <summary>
    /// Destruye y recrea la base de datos con datos de test para e2e.
    /// </summary>
    [HttpPost("seed")]
    public IActionResult Seed()
    {
        if (Environment.GetEnvironmentVariable("E2E_SEED_ENABLED") != "true")
            return NotFound();

        try
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            SeedDatos();
            CrearFotos();

            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            var mensajes = new List<string>();
            for (var e = ex; e != null; e = e.InnerException)
                mensajes.Add($"[{e.GetType().Name}] {e.Message}");
            return StatusCode(500, new { error = string.Join(" → ", mensajes) });
        }
    }

    /// <summary>
    /// Destruye la base de datos de test.
    /// </summary>
    [HttpPost("cleanup")]
    public IActionResult Cleanup()
    {
        if (Environment.GetEnvironmentVariable("E2E_SEED_ENABLED") != "true")
            return NotFound();

        _context.Database.EnsureDeleted();

        return Ok(new { ok = true });
    }

    private void SeedDatos()
    {
        // Id = 0 le indica a EF Core que el ID lo genera la DB (identity).
        // La DB está vacía tras EnsureCreated, así que club→1, torneo→1, equipo1→1, equipo2→2.

        var club = new Club { Id = 0, Nombre = "Club de Prueba" };
        _context.Clubs.Add(club);

        var torneo = new Torneo { Id = 0, Nombre = "Torneo E2E", Anio = 2026, TorneoAgrupadorId = 1, EsVisibleEnApp = true, SeVenLosGolesEnTablaDePosiciones = true };
        _context.Torneos.Add(torneo);
        _context.SaveChanges();

        var fase = new FaseTodosContraTodos
        {
            Id = 0,
            TorneoId = torneo.Id,
            Nombre = string.Empty,
            Numero = 1,
            EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
            EsVisibleEnApp = true
        };
        _context.Fases.Add(fase);
        _context.SaveChanges();

        var zona = new ZonaTodosContraTodos { Id = 0, FaseId = fase.Id, Nombre = "Zona única" };
        _context.Zonas.Add(zona);
        _context.SaveChanges();

        // Equipo 1 → Id=1 → código MTD0001 (hardcodeado en los tests YAML)
        // Equipo 2 → Id=2 → item-equipo-2 en test 15 (cambiar equipo)
        var equipo1 = new Equipo { Id = 0, Nombre = "Equipo de Prueba", ClubId = club.Id, Jugadores = new List<JugadorEquipo>(), Zonas = new List<EquipoZona>() };
        _context.Equipos.Add(equipo1);

        // Equipo 2: Pedro González (87654321) queda Activo acá, no en equipo 1,
        // para que los tests puedan ficharlo al equipo 1 sin "ya está fichado".
        var equipo2 = new Equipo { Id = 0, Nombre = "Equipo B", ClubId = club.Id, Jugadores = new List<JugadorEquipo>(), Zonas = new List<EquipoZona>() };
        _context.Equipos.Add(equipo2);
        _context.SaveChanges();

        _context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = equipo1.Id, ZonaId = zona.Id });
        _context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = equipo2.Id, ZonaId = zona.Id });
        _context.SaveChanges();

        // ── Jugadores ──────────────────────────────────────────────────────────

        // 87654321: Activo en equipo 2 — JugadorExiste=true, JugadorEstaPendiente=false.
        // Tests 03 (anon fichaje ya fichado) y 05 (delegado ya fichado) pueden ficharlo al equipo 1 sin error.
        var jugadorPedro = new Jugador { Id = 0, Nombre = "Pedro", Apellido = "González", DNI = "87654321", FechaNacimiento = new DateTime(1995, 1, 1) };
        _context.Jugadores.Add(jugadorPedro);
        _context.SaveChanges();
        _context.JugadorEquipo.Add(new JugadorEquipo { Id = 0, JugadorId = jugadorPedro.Id, EquipoId = equipo2.Id, EstadoJugadorId = (int)EstadoJugadorEnum.Activo, FechaFichaje = DateTime.Now });

        // 33333333: Pendiente en equipo 1 — para tab Pendientes en test 13
        var jugadorPendiente = new Jugador { Id = 0, Nombre = "Jugador", Apellido = "Pendiente", DNI = "33333333", FechaNacimiento = new DateTime(2006, 1, 1) };
        _context.Jugadores.Add(jugadorPendiente);
        _context.SaveChanges();
        _context.JugadorEquipo.Add(new JugadorEquipo { Id = 0, JugadorId = jugadorPendiente.Id, EquipoId = equipo1.Id, EstadoJugadorId = (int)EstadoJugadorEnum.FichajePendienteDeAprobacion, FechaFichaje = DateTime.Now });

        // 99999999: Activo en equipo 1, con foto — para ver carnets en tests 08 (mis jugadores) y 11 (buscar)
        var jugadorCarnet = new Jugador { Id = 0, Nombre = "Jugador", Apellido = "Carnet", DNI = "99999999", FechaNacimiento = new DateTime(2005, 1, 1) };
        _context.Jugadores.Add(jugadorCarnet);
        _context.SaveChanges();
        _context.JugadorEquipo.Add(new JugadorEquipo { Id = 0, JugadorId = jugadorCarnet.Id, EquipoId = equipo1.Id, EstadoJugadorId = (int)EstadoJugadorEnum.Activo, FechaFichaje = DateTime.Now });
        _context.SaveChanges();

        // 44444444: Activo en equipo 2 — para test 03 (anon fichaje jugador ya fichado)
        // DNI puro jugador (sin rol delegado) para evitar conflictos con checks delegado en FicharEnOtroEquipo.
        var jugadorCarlos = new Jugador { Id = 0, Nombre = "Carlos", Apellido = "Ruiz", DNI = "44444444", FechaNacimiento = new DateTime(1997, 3, 20) };
        _context.Jugadores.Add(jugadorCarlos);
        _context.SaveChanges();
        _context.JugadorEquipo.Add(new JugadorEquipo { Id = 0, JugadorId = jugadorCarlos.Id, EquipoId = equipo2.Id, EstadoJugadorId = (int)EstadoJugadorEnum.Activo, FechaFichaje = DateTime.Now });
        _context.SaveChanges();

        // 55555555: Activo en equipo 2 — para test 10 (auth fichaje ya fichado)
        // DNI puro jugador (sin rol delegado) para evitar conflictos con checks delegado en FicharEnOtroEquipo.
        var jugadorMaria = new Jugador { Id = 0, Nombre = "María", Apellido = "López", DNI = "55555555", FechaNacimiento = new DateTime(1998, 5, 15) };
        _context.Jugadores.Add(jugadorMaria);
        _context.SaveChanges();
        _context.JugadorEquipo.Add(new JugadorEquipo { Id = 0, JugadorId = jugadorMaria.Id, EquipoId = equipo2.Id, EstadoJugadorId = (int)EstadoJugadorEnum.Activo, FechaFichaje = DateTime.Now });
        _context.SaveChanges();

        // ── Delegados ──────────────────────────────────────────────────────────

        var estadoActivo = _context.EstadoDelegado.First(e => e.Id == (int)EstadoDelegadoEnum.Activo);
        var rolDelegado = _context.Roles.First(r => r.Nombre == "Delegado");

        // 87654321 (Pedro): Activo en club 1 — DelegadoExiste=true para test 05 (delegado ya fichado)
        var delegadoPedro = new Delegado { Id = 0, DNI = "87654321", Nombre = "Pedro", Apellido = "González", FechaNacimiento = new DateTime(1995, 1, 1), DelegadoClubs = new List<DelegadoClub>() };
        _context.Delegados.Add(delegadoPedro);
        _context.SaveChanges();
        _context.DelegadoClub.Add(new DelegadoClub { Id = 0, DelegadoId = delegadoPedro.Id, ClubId = club.Id, EstadoDelegadoId = estadoActivo.Id });
        _context.Usuarios.Add(new Usuario { Id = 0, NombreUsuario = "pedro.gonzalez", Password = AuthCore.HashPassword("password123"), RolId = rolDelegado.Id, DelegadoId = delegadoPedro.Id });

        // 11111111 (delegado-e2e): Activo en club 1 — para tests 07-15 (flujos autenticados)
        var delegadoE2E = new Delegado { Id = 0, DNI = "11111111", Nombre = "Delegado", Apellido = "E2E", FechaNacimiento = new DateTime(1985, 1, 1), DelegadoClubs = new List<DelegadoClub>() };
        _context.Delegados.Add(delegadoE2E);
        _context.SaveChanges();
        _context.DelegadoClub.Add(new DelegadoClub { Id = 0, DelegadoId = delegadoE2E.Id, ClubId = club.Id, EstadoDelegadoId = estadoActivo.Id });
        _context.Usuarios.Add(new Usuario { Id = 0, NombreUsuario = "delegado-e2e", Password = AuthCore.HashPassword("password123"), RolId = rolDelegado.Id, DelegadoId = delegadoE2E.Id });

        _context.SaveChanges();
    }

    private void CrearFotos()
    {
        Directory.CreateDirectory(_paths.ImagenesJugadoresAbsolute);
        Directory.CreateDirectory(_paths.ImagenesDelegadosAbsolute);
        Directory.CreateDirectory(_paths.ImagenesTemporalesCarnetAbsolute);
        Directory.CreateDirectory(_paths.ImagenesTemporalesDNIFrenteAbsolute);
        Directory.CreateDirectory(_paths.ImagenesTemporalesDNIDorsoAbsolute);

        CrearFotoJpg($"{_paths.ImagenesJugadoresAbsolute}/87654321.jpg");
        CrearFotoJpg($"{_paths.ImagenesJugadoresAbsolute}/99999999.jpg");
        CrearFotoJpg($"{_paths.ImagenesDelegadosAbsolute}/87654321.jpg");
        CrearFotoJpg($"{_paths.ImagenesDelegadosAbsolute}/11111111.jpg");
    }

    private static void CrearFotoJpg(string path)
    {
        using var bitmap = new SKBitmap(240, 240);
        using var canvas = new SKCanvas(bitmap);
        canvas.DrawColor(SKColors.CornflowerBlue);
        using var stream = new FileStream(path, FileMode.Create);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 75);
        data.SaveTo(stream);
    }
}
