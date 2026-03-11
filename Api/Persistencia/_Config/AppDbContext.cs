using Api.Core.Entidades;
using Api.Core.Entidades.EntidadesConValoresPredefinidos;
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

        builder.Entity<EstadoDelegado>().ToTable("_EstadoDelegado");
        builder.Entity<EstadoJugador>().ToTable("_EstadoJugador");
        builder.Entity<Rol>().ToTable("_Rol");
        builder.Entity<FaseFormato>().ToTable("_FaseFormato");
        builder.Entity<InstanciaEliminacionDirecta>().ToTable("_InstanciaEliminacionDirecta");
        builder.Entity<EstadoFase>().ToTable("_EstadoFase");
        
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

        builder.Entity<FaseFormato>().HasData(
            new FaseFormato { Id = 1, Nombre = "Todos contra todos" },
            new FaseFormato { Id = 2, Nombre = "Eliminación directa" }
        );

        builder.Entity<InstanciaEliminacionDirecta>().HasData(
            new InstanciaEliminacionDirecta { Id = 16, Nombre = "Octavos de final" },
            new InstanciaEliminacionDirecta { Id = 8, Nombre = "Cuartos de final" },
            new InstanciaEliminacionDirecta { Id = 4, Nombre = "Semifinal" },
            new InstanciaEliminacionDirecta { Id = 2, Nombre = "Final" }
        );

        builder.Entity<EstadoFase>().HasData(
            new EstadoFase { Id = 100, Estado = "Inicio pendiente" },
            new EstadoFase { Id = 200, Estado = "En curso" },
            new EstadoFase { Id = 300, Estado = "Finalizada" }
        );

        builder.Entity<TorneoAgrupador>()
            .ToTable("TorneoAgrupadores")
            .HasData(
                new TorneoAgrupador { Id = 1, Nombre = "General", VisibleEnApp = false }
            );

        builder.Entity<Torneo>()
            .HasIndex(t => new { t.Nombre, t.Anio, t.TorneoAgrupadorId })
            .IsUnique();

        builder.Entity<TorneoCategoria>()
            .ToTable("TorneoCategorias")
            .HasOne(tc => tc.Torneo)
            .WithMany(t => t.Categorias)
            .HasForeignKey(tc => tc.TorneoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TorneoFase>()
            .HasOne(tf => tf.Torneo)
            .WithMany(t => t.Fases)
            .HasForeignKey(tf => tf.TorneoId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<TorneoFase>()
            .HasIndex(tf => new { tf.TorneoId, tf.Numero })
            .IsUnique();

        builder.Entity<TorneoZona>()
            .HasOne(tz => tz.TorneoFase)
            .WithMany(tf => tf.Zonas)
            .HasForeignKey(tz => tz.TorneoFaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TorneoFecha>()
            .HasOne(tf => tf.Zona)
            .WithMany(z => z.Fechas)
            .HasForeignKey(tf => tf.ZonaId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<TorneoFecha>()
            .HasIndex(tf => new { tf.ZonaId, tf.Numero })
            .IsUnique();

        builder.Entity<Equipo>()
            .HasOne(e => e.ZonaActual)
            .WithMany(z => z.Equipos)
            .HasForeignKey(e => e.ZonaActualId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.Entity<Equipo>()
            .HasIndex(e => new { e.Nombre, e.ZonaActualId })
            .IsUnique()
            .HasFilter("[ZonaActualId] IS NOT NULL");

        builder.Entity<Delegado>()
            .HasIndex(d => d.DNI)
            .IsUnique();

        builder.Entity<Usuario>()
            .HasOne(u => u.Delegado)
            .WithOne(d => d.Usuario)
            .HasForeignKey<Usuario>(u => u.DelegadoId)
            .OnDelete(DeleteBehavior.Cascade);
        
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

        builder.Entity<DelegadoClub>()
            .HasOne(dc => dc.Club)
            .WithMany(c => c.DelegadoClubs)
            .HasForeignKey(dc => dc.ClubId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<DelegadoClub>()
            .HasOne(dc => dc.EstadoDelegado)
            .WithMany()
            .HasForeignKey(dc => dc.EstadoDelegadoId)
            .OnDelete(DeleteBehavior.Restrict);
        
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
    public DbSet<DelegadoClub> DelegadoClub { get; set; } = null!;
    public DbSet<Jugador> Jugadores { get; set; } = null!;
    public DbSet<JugadorEquipo> JugadorEquipo { get; set; } = null!;
    public DbSet<EstadoJugador> EstadoJugador { get; set; } = null!;
    public DbSet<EstadoDelegado> EstadoDelegado { get; set; } = null!;
    public DbSet<FaseFormato> FaseFormato { get; set; } = null!;
    public DbSet<InstanciaEliminacionDirecta> InstanciaEliminacionDirecta { get; set; } = null!;
    public DbSet<EstadoFase> EstadoFase { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Rol> Roles { get; set; } = null!;
    public DbSet<Torneo> Torneos { get; set; } = null!;
    public DbSet<TorneoAgrupador> TorneoAgrupadores { get; set; } = null!;
    public DbSet<TorneoCategoria> TorneoCategorias { get; set; } = null!;
    public DbSet<TorneoFase> TorneoFases { get; set; } = null!;
    public DbSet<TorneoZona> TorneoZonas { get; set; } = null!;
    public DbSet<TorneoFecha> TorneoFechas { get; set; } = null!;
    public DbSet<HistorialDePagos> HistorialDePagos { get; set; } = null!;
}