using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Servicios;
using Api.Persistencia._Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

/// <summary>
/// Endpoints de utilidad para tests E2E de frontend web contra backend real.
/// Solo disponibles cuando la variable de entorno E2E_SEED_ENABLED=true.
/// Base de datos independiente: liga_e2e_fe_test (ASPNETCORE_ENVIRONMENT=E2EFETest).
/// </summary>
[Route("api/e2e-fe")]
[ApiController]
[AllowAnonymous]
public class E2ESeedFeController : ControllerBase
{
    private readonly AppDbContext _context;

    public E2ESeedFeController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Destruye y recrea la base de datos con datos de test para e2e de frontend web.
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
    /// Destruye la base de datos de test de frontend.
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
        var rolAdmin = _context.Roles.First(r => r.Nombre == "Administrador");
        var rolDelegado = _context.Roles.First(r => r.Nombre == "Delegado");
        var estadoDelegadoActivo = _context.EstadoDelegado.First(e => e.Id == (int)EstadoDelegadoEnum.Activo);
        var estadoDelegadoPendiente = _context.EstadoDelegado.First(e => e.Id == (int)EstadoDelegadoEnum.PendienteDeAprobacion);

        // ── Usuario admin ──────────────────────────────────────────────────────
        _context.Usuarios.Add(new Usuario
        {
            Id = 0,
            NombreUsuario = "admin",
            Password = AuthCore.HashPassword("admin123"),
            RolId = rolAdmin.Id
        });
        _context.SaveChanges();

        // ── Clubs ──────────────────────────────────────────────────────────────
        var club1 = new Club { Id = 0, Nombre = "Club Defensores del Norte", Localidad = "San Martín", Direccion = "Av. San Martín 123" };
        var club2 = new Club { Id = 0, Nombre = "Atlético San Lorenzo", Localidad = "Palermo", Direccion = "Calle Corrientes 456" };
        _context.Clubs.Add(club1);
        _context.Clubs.Add(club2);
        _context.SaveChanges();

        // ── Equipos ────────────────────────────────────────────────────────────
        var equipo1 = new Equipo { Id = 0, Nombre = "Primera División", ClubId = club1.Id, Jugadores = new List<JugadorEquipo>(), Zonas = new List<EquipoZona>() };
        var equipo2 = new Equipo { Id = 0, Nombre = "Reserva", ClubId = club1.Id, Jugadores = new List<JugadorEquipo>(), Zonas = new List<EquipoZona>() };
        _context.Equipos.Add(equipo1);
        _context.Equipos.Add(equipo2);
        _context.SaveChanges();

        // ── Jugadores ──────────────────────────────────────────────────────────
        var jugador1 = new Jugador { Id = 0, Nombre = "Carlos", Apellido = "Rodríguez", DNI = "12345678", FechaNacimiento = new DateTime(1995, 3, 10) };
        var jugador2 = new Jugador { Id = 0, Nombre = "Marcos", Apellido = "García", DNI = "23456789", FechaNacimiento = new DateTime(1998, 7, 22) };
        var jugador3 = new Jugador { Id = 0, Nombre = "Lucía", Apellido = "Fernández", DNI = "34567890", FechaNacimiento = new DateTime(2001, 11, 5) };
        var jugador4 = new Jugador { Id = 0, Nombre = "Jugador", Apellido = "Pendiente", DNI = "45678901", FechaNacimiento = new DateTime(2003, 4, 15) };
        _context.Jugadores.Add(jugador1);
        _context.Jugadores.Add(jugador2);
        _context.Jugadores.Add(jugador3);
        _context.Jugadores.Add(jugador4);
        _context.SaveChanges();

        _context.JugadorEquipo.Add(new JugadorEquipo { Id = 0, JugadorId = jugador1.Id, EquipoId = equipo1.Id, EstadoJugadorId = (int)EstadoJugadorEnum.Activo, FechaFichaje = DateTime.Now });
        _context.JugadorEquipo.Add(new JugadorEquipo { Id = 0, JugadorId = jugador2.Id, EquipoId = equipo1.Id, EstadoJugadorId = (int)EstadoJugadorEnum.Activo, FechaFichaje = DateTime.Now });
        _context.JugadorEquipo.Add(new JugadorEquipo { Id = 0, JugadorId = jugador3.Id, EquipoId = equipo1.Id, EstadoJugadorId = (int)EstadoJugadorEnum.Activo, FechaFichaje = DateTime.Now });
        _context.JugadorEquipo.Add(new JugadorEquipo { Id = 0, JugadorId = jugador4.Id, EquipoId = equipo1.Id, EstadoJugadorId = (int)EstadoJugadorEnum.FichajePendienteDeAprobacion, FechaFichaje = DateTime.Now });
        _context.SaveChanges();

        // ── Delegados ──────────────────────────────────────────────────────────
        var delegado1 = new Delegado { Id = 0, DNI = "56789012", Nombre = "Roberto", Apellido = "Méndez", FechaNacimiento = new DateTime(1980, 6, 20), DelegadoClubs = new List<DelegadoClub>() };
        var delegado2 = new Delegado { Id = 0, DNI = "67890123", Nombre = "Ana", Apellido = "Suárez", FechaNacimiento = new DateTime(1985, 9, 14), DelegadoClubs = new List<DelegadoClub>() };
        var delegado3 = new Delegado { Id = 0, DNI = "78901234", Nombre = "Delegado", Apellido = "Pendiente", FechaNacimiento = new DateTime(1990, 1, 1), DelegadoClubs = new List<DelegadoClub>() };
        _context.Delegados.Add(delegado1);
        _context.Delegados.Add(delegado2);
        _context.Delegados.Add(delegado3);
        _context.SaveChanges();

        _context.DelegadoClub.Add(new DelegadoClub { Id = 0, DelegadoId = delegado1.Id, ClubId = club1.Id, EstadoDelegadoId = estadoDelegadoActivo.Id });
        _context.DelegadoClub.Add(new DelegadoClub { Id = 0, DelegadoId = delegado2.Id, ClubId = club1.Id, EstadoDelegadoId = estadoDelegadoActivo.Id });
        _context.DelegadoClub.Add(new DelegadoClub { Id = 0, DelegadoId = delegado3.Id, ClubId = club2.Id, EstadoDelegadoId = estadoDelegadoPendiente.Id });

        _context.Usuarios.Add(new Usuario { Id = 0, NombreUsuario = "roberto.mendez", Password = AuthCore.HashPassword("password123"), RolId = rolDelegado.Id, DelegadoId = delegado1.Id });
        _context.Usuarios.Add(new Usuario { Id = 0, NombreUsuario = "ana.suarez", Password = AuthCore.HashPassword("password123"), RolId = rolDelegado.Id, DelegadoId = delegado2.Id });

        _context.SaveChanges();
    }
}
