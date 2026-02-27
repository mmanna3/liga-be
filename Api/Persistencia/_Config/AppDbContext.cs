using Api.Core.Entidades;
using Api.Core.Servicios;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia._Config;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        // var connectionString = Database.GetDbConnection().ConnectionString;
        // Console.WriteLine($"Using Connection String: {connectionString}");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<Rol>().HasData(
            new Rol { Id = 1, Nombre = "Administrador" },
            new Rol { Id = 2, Nombre = "Usuario" },
            new Rol { Id = 3, Nombre = "Consulta" },
            new Rol { Id = 4, Nombre = "Delegado" }
        );
        
        builder.Entity<Usuario>()
            .Property(u => u.RolId)
            .HasDefaultValue(1);
            
        builder.Entity<EstadoJugador>().HasData(
            new EstadoJugador { Id = 1, Estado = "Fichaje pendiente de aprobación" },
            new EstadoJugador { Id = 2, Estado = "Fichaje rechazado" },
            new EstadoJugador { Id = 3, Estado = "Activo" },
            new EstadoJugador { Id = 4, Estado = "Suspendido" },
            new EstadoJugador { Id = 5, Estado = "Inhabilitado" },
            new EstadoJugador { Id = 6, Estado = "Aprobado pendiente de pago" }
        );

        builder.Entity<EstadoDelegado>().HasData(
            new EstadoDelegado { Id = 1, Estado = "Pendiente de aprobación" },
            new EstadoDelegado { Id = 2, Estado = "Rechazado" },
            new EstadoDelegado { Id = 3, Estado = "Activo" }
        );

        builder.Entity<Delegado>()
            .Property(d => d.EstadoDelegadoId)
            .HasDefaultValue(3); // Activo para delegados existentes
        
        builder.Entity<Jugador>()
            .HasIndex(u => u.DNI)
            .IsUnique();
        
        builder.Entity<Usuario>()
            .HasIndex(u => u.NombreUsuario)
            .IsUnique();
        
        builder.Entity<JugadorEquipo>()
            .HasOne(je => je.HistorialDePagos)
            .WithOne(hp => hp.JugadorEquipo)
            .HasForeignKey<HistorialDePagos>(hp => hp.JugadorEquipoId);
        
        builder.Entity<Usuario>().HasData(
            new Usuario 
            { 
                Id = 1, 
                NombreUsuario = "mati", 
                Password = AuthCore.HashPassword("mandarina1"),
                RolId = 1
            },
            new Usuario 
            { 
                Id = 2, 
                NombreUsuario = "pipa", 
                Password = AuthCore.HashPassword("edefiliga"),
                RolId = 1
            },
            new Usuario 
            { 
                Id = 101, 
                NombreUsuario = "consulta", 
                Password = AuthCore.HashPassword("consulta"),
                RolId = 3
            }
        );
    }
    
    public DbSet<Club> Clubs { get; set; } = null!;
    public DbSet<Equipo> Equipos { get; set; } = null!;
    public DbSet<Delegado> Delegados { get; set; } = null!;
    public DbSet<Jugador> Jugadores { get; set; } = null!;
    public DbSet<JugadorEquipo> JugadorEquipo { get; set; } = null!;
    public DbSet<EstadoJugador> EstadoJugador { get; set; } = null!;
    public DbSet<EstadoDelegado> EstadoDelegado { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Rol> Roles { get; set; } = null!;
    public DbSet<Torneo> Torneos { get; set; } = null!;
    public DbSet<HistorialDePagos> HistorialDePagos { get; set; } = null!;
}