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
        builder.HasDefaultSchema("dbo");
        base.OnModelCreating(builder);

        builder.Entity<EstadoDelegado>().ToTable("_EstadoDelegado");
        builder.Entity<EstadoJugador>().ToTable("_EstadoJugador");
        builder.Entity<Rol>().ToTable("_Rol");
        builder.Entity<FaseFormato>().ToTable("_FaseFormato");
        builder.Entity<InstanciaEliminacionDirecta>().ToTable("_InstanciaEliminacionDirecta");
        builder.Entity<EstadoFase>().ToTable("_EstadoFase");
        builder.Entity<LocalVisitante>().ToTable("_LocalVisitante");

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

        builder.Entity<LocalVisitante>().HasData(
            new LocalVisitante { Id = 1, Estado = "Local" },
            new LocalVisitante { Id = 2, Estado = "Visitante" }
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

        builder.Entity<Fase>()
            .ToTable("Fases")
            .HasDiscriminator<string>("TipoFase")
            .HasValue<FaseTodosContraTodos>("TodosContraTodos")
            .HasValue<FaseEliminacionDirecta>("EliminacionDirecta");

        builder.Entity<Fase>()
            .HasOne(tf => tf.Torneo)
            .WithMany(t => t.Fases)
            .HasForeignKey(tf => tf.TorneoId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Fase>()
            .HasIndex(tf => new { tf.TorneoId, tf.Numero })
            .IsUnique();

        builder.Entity<Zona>()
            .ToTable("Zonas")
            .HasDiscriminator<string>("TipoZona")
            .HasValue<ZonaTodosContraTodos>("TodosContraTodos")
            .HasValue<ZonaEliminacionDirecta>("EliminacionDirecta");

        builder.Entity<ZonaTodosContraTodos>()
            .HasOne(z => z.Fase)
            .WithMany(f => f.Zonas)
            .HasForeignKey(z => z.FaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ZonaEliminacionDirecta>()
            .HasOne(z => z.Fase)
            .WithMany(f => f.Zonas)
            .HasForeignKey(z => z.FaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Fecha>()
            .ToTable("Fechas")
            .HasDiscriminator<string>("TipoFecha")
            .HasValue<FechaTodosContraTodos>("TodosContraTodos")
            .HasValue<FechaEliminacionDirecta>("EliminacionDirecta");

        builder.Entity<ZonaTodosContraTodos>()
            .HasMany(z => z.Fechas)
            .WithOne(f => f.Zona)
            .HasForeignKey(f => f.ZonaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ZonaEliminacionDirecta>()
            .HasMany(z => z.Fechas)
            .WithOne(f => f.Zona)
            .HasForeignKey(f => f.ZonaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FechaEliminacionDirecta>()
            .HasOne(f => f.InstanciaEliminacionDirecta)
            .WithMany()
            .HasForeignKey(f => f.InstanciaEliminacionDirectaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FechaTodosContraTodos>()
            .HasIndex(f => new { f.ZonaId, f.Numero })
            .IsUnique();

        builder.Entity<FechaEliminacionDirecta>()
            .HasIndex(f => new { f.ZonaId, f.InstanciaEliminacionDirectaId })
            .IsUnique();

        builder.Entity<Jornada>()
            .ToTable("Jornadas", t =>
            {
                if (!Database.IsSqlite())
                {
                    t.HasCheckConstraint(
                        "CK_Jornada_Tipo_Valido",
                        @"([Tipo] = N'Normal' AND [LocalEquipoId] IS NOT NULL AND [VisitanteEquipoId] IS NOT NULL AND [LocalEquipoId] <> [VisitanteEquipoId] AND [EquipoId] IS NULL AND [JornadaLibre_EquipoId] IS NULL AND [LocalOVisitanteId] IS NULL)
    OR
    ([Tipo] = N'Libre' AND [JornadaLibre_EquipoId] IS NOT NULL AND [LocalEquipoId] IS NULL AND [VisitanteEquipoId] IS NULL AND [EquipoId] IS NULL AND [LocalOVisitanteId] IS NULL)
    OR
    ([Tipo] = N'Interzonal' AND [EquipoId] IS NOT NULL AND [LocalOVisitanteId] IS NOT NULL AND [LocalEquipoId] IS NULL AND [VisitanteEquipoId] IS NULL AND [JornadaLibre_EquipoId] IS NULL)");
                }
            });
        builder.Entity<Jornada>()
            .HasDiscriminator<string>("Tipo")
            .HasValue<JornadaNormal>("Normal")
            .HasValue<JornadaLibre>("Libre")
            .HasValue<JornadaInterzonal>("Interzonal");
        builder.Entity<Jornada>()
            .HasOne(j => j.Fecha)
            .WithMany(f => f.Jornadas)
            .HasForeignKey(j => j.FechaId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Jornada>()
            .HasIndex(j => j.FechaId);
        builder.Entity<JornadaNormal>()
            .HasOne(j => j.LocalEquipo)
            .WithMany()
            .HasForeignKey(j => j.LocalEquipoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<JornadaNormal>()
            .HasOne(j => j.VisitanteEquipo)
            .WithMany()
            .HasForeignKey(j => j.VisitanteEquipoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<JornadaLibre>()
            .HasOne(j => j.Equipo)
            .WithMany()
            .HasForeignKey(j => j.EquipoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<JornadaInterzonal>()
            .HasOne(j => j.Equipo)
            .WithMany()
            .HasForeignKey(j => j.EquipoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<JornadaInterzonal>()
            .HasOne(j => j.LocalVisitante)
            .WithMany()
            .HasForeignKey(j => j.LocalOVisitanteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EquipoZona>()
            .HasOne(ez => ez.Equipo)
            .WithMany(e => e.Zonas)
            .HasForeignKey(ez => ez.EquipoId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<EquipoZona>()
            .HasOne(ez => ez.Zona)
            .WithMany(z => z.EquiposZona)
            .HasForeignKey(ez => ez.ZonaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FixtureAlgoritmoFecha>()
            .ToTable("FixtureAlgoritmoFecha")
            .HasOne(f => f.FixtureAlgoritmo)
            .WithMany(fa => fa.Fechas)
            .HasForeignKey(f => f.FixtureAlgoritmoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FixtureAlgoritmo>().HasData(
            new FixtureAlgoritmo { Id = 1, CantidadDeEquipos = 4, Nombre = "Clásico" },
            new FixtureAlgoritmo { Id = 2, CantidadDeEquipos = 8, Nombre = "Apertura" },
            new FixtureAlgoritmo { Id = 3, CantidadDeEquipos = 10, Nombre = "Apertura" },
            new FixtureAlgoritmo { Id = 4, CantidadDeEquipos = 12, Nombre = "Apertura" },
            new FixtureAlgoritmo { Id = 5, CantidadDeEquipos = 14, Nombre = "Apertura" },
            new FixtureAlgoritmo { Id = 6, CantidadDeEquipos = 16, Nombre = "Apertura" },
            new FixtureAlgoritmo { Id = 7, CantidadDeEquipos = 4, Nombre = "Champions" },
            new FixtureAlgoritmo { Id = 8, CantidadDeEquipos = 8, Nombre = "Clausura" },
            new FixtureAlgoritmo { Id = 9, CantidadDeEquipos = 10, Nombre = "Clausura" },
            new FixtureAlgoritmo { Id = 10, CantidadDeEquipos = 12, Nombre = "Clausura" },
            new FixtureAlgoritmo { Id = 11, CantidadDeEquipos = 14, Nombre = "Clausura" },
            new FixtureAlgoritmo { Id = 12, CantidadDeEquipos = 16, Nombre = "Clausura" },
            new FixtureAlgoritmo { Id = 13, CantidadDeEquipos = 6, Nombre = "Apertura" },
            new FixtureAlgoritmo { Id = 14, CantidadDeEquipos = 6, Nombre = "Clausura" }
        );

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
                RolId = 0
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
            },
            new Usuario
            {
                Id = 1000,
                NombreUsuario = "eze",
                Password = AuthCore.HashPassword("edefiliga"),
                RolId = 1
            },
            new Usuario
            {
                Id = 1001,
                NombreUsuario = "lucas",
                Password = AuthCore.HashPassword("edefiliga"),
                RolId = 1
            },
            new Usuario
            {
                Id = 1002,
                NombreUsuario = "elias",
                Password = AuthCore.HashPassword("edefiliga"),
                RolId = 1
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
    public DbSet<LocalVisitante> LocalVisitante { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Rol> Roles { get; set; } = null!;
    public DbSet<Torneo> Torneos { get; set; } = null!;
    public DbSet<TorneoAgrupador> TorneoAgrupadores { get; set; } = null!;
    public DbSet<TorneoCategoria> TorneoCategorias { get; set; } = null!;
    public DbSet<Fase> Fases { get; set; } = null!;
    public DbSet<Zona> Zonas { get; set; } = null!;
    public DbSet<Fecha> Fechas { get; set; } = null!;
    public DbSet<EquipoZona> EquipoZona { get; set; } = null!;
    public DbSet<FixtureAlgoritmo> FixtureAlgoritmos { get; set; } = null!;
    public DbSet<FixtureAlgoritmoFecha> FixtureAlgoritmoFecha { get; set; } = null!;
    public DbSet<HistorialDePagos> HistorialDePagos { get; set; } = null!;
    public DbSet<Jornada> Jornadas { get; set; } = null!;
}