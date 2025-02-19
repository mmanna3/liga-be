using Api.Core.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia._Config;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        var connectionString = Database.GetDbConnection().ConnectionString;
        Console.WriteLine($"Using Connection String: {connectionString}");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<EstadoJugador>().HasData(
            new EstadoJugador { Id = 1, Estado = "Fichaje pendiente de aprobación" },
            new EstadoJugador { Id = 2, Estado = "Fichaje rechazado" },
            new EstadoJugador { Id = 3, Estado = "Activo" },
            new EstadoJugador { Id = 4, Estado = "Suspendido" },
            new EstadoJugador { Id = 5, Estado = "Inhabilitado" }
        );
    }
    
    public DbSet<Club> Clubs { get; set; } = null!;
    public DbSet<Equipo> Equipos { get; set; } = null!;
    public DbSet<Delegado> Delegados { get; set; } = null!;
    public DbSet<Jugador> Jugadores { get; set; } = null!;
    public DbSet<JugadorEquipo> JugadorEquipo { get; set; } = null!;
    public DbSet<EstadoJugador> EstadoJugador { get; set; } = null!;
}