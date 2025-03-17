﻿// <auto-generated />
using System;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Api.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Api.Core.Entidades.Club", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Clubs");
                });

            modelBuilder.Entity("Api.Core.Entidades.Delegado", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Apellido")
                        .IsRequired()
                        .HasMaxLength(14)
                        .HasColumnType("nvarchar(14)");

                    b.Property<int>("ClubId")
                        .HasColumnType("int");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasMaxLength(14)
                        .HasColumnType("nvarchar(14)");

                    b.Property<int>("UsuarioId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ClubId");

                    b.HasIndex("UsuarioId");

                    b.ToTable("Delegados");
                });

            modelBuilder.Entity("Api.Core.Entidades.Equipo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ClubId")
                        .HasColumnType("int");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int?>("TorneoId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ClubId");

                    b.HasIndex("TorneoId");

                    b.HasIndex(new[] { "Nombre", "TorneoId" }, "IX_Equipo_Nombre_TorneoId")
                        .IsUnique()
                        .HasFilter("[TorneoId] IS NOT NULL");

                    b.ToTable("Equipos");
                });

            modelBuilder.Entity("Api.Core.Entidades.EstadoJugador", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Estado")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("EstadoJugador");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Estado = "Fichaje pendiente de aprobación"
                        },
                        new
                        {
                            Id = 2,
                            Estado = "Fichaje rechazado"
                        },
                        new
                        {
                            Id = 3,
                            Estado = "Activo"
                        },
                        new
                        {
                            Id = 4,
                            Estado = "Suspendido"
                        },
                        new
                        {
                            Id = 5,
                            Estado = "Inhabilitado"
                        },
                        new
                        {
                            Id = 6,
                            Estado = "Aprobado pendiente de pago"
                        });
                });

            modelBuilder.Entity("Api.Core.Entidades.HistorialDePagos", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Fecha")
                        .HasColumnType("datetime2");

                    b.Property<int>("JugadorEquipoId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("JugadorEquipoId")
                        .IsUnique();

                    b.ToTable("HistorialDePagos");
                });

            modelBuilder.Entity("Api.Core.Entidades.Jugador", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Apellido")
                        .IsRequired()
                        .HasMaxLength(14)
                        .HasColumnType("nvarchar(14)");

                    b.Property<string>("DNI")
                        .IsRequired()
                        .HasMaxLength(9)
                        .HasColumnType("nvarchar(9)");

                    b.Property<DateTime>("FechaNacimiento")
                        .HasColumnType("datetime2");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasMaxLength(14)
                        .HasColumnType("nvarchar(14)");

                    b.HasKey("Id");

                    b.HasIndex("DNI")
                        .IsUnique();

                    b.ToTable("Jugadores");
                });

            modelBuilder.Entity("Api.Core.Entidades.JugadorEquipo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("EquipoId")
                        .HasColumnType("int");

                    b.Property<int>("EstadoJugadorId")
                        .HasColumnType("int");

                    b.Property<DateTime>("FechaFichaje")
                        .HasColumnType("datetime2");

                    b.Property<int>("JugadorId")
                        .HasColumnType("int");

                    b.Property<string>("Motivo")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.HasKey("Id");

                    b.HasIndex("EquipoId");

                    b.HasIndex("EstadoJugadorId");

                    b.HasIndex("JugadorId");

                    b.ToTable("JugadorEquipo");
                });

            modelBuilder.Entity("Api.Core.Entidades.Torneo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Torneos");
                });

            modelBuilder.Entity("Api.Core.Entidades.Usuario", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("NombreUsuario")
                        .IsRequired()
                        .HasMaxLength(14)
                        .HasColumnType("nvarchar(14)");

                    b.Property<string>("Password")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("NombreUsuario")
                        .IsUnique();

                    b.ToTable("Usuarios");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            NombreUsuario = "mati",
                            Password = "$2a$12$d4jBzxzvym4tDVy2IZciOuJw3jkQCVbMzKc745WSlsLRiwJPtlPwe"
                        },
                        new
                        {
                            Id = 2,
                            NombreUsuario = "pipa",
                            Password = "$2a$12$9UEoU8fizncf8D/wTB7fmu4YBwnUkC4d4Bw8r4Dd5nGIQLQyIxe2y"
                        });
                });

            modelBuilder.Entity("Api.Core.Entidades.Delegado", b =>
                {
                    b.HasOne("Api.Core.Entidades.Club", "Club")
                        .WithMany("Delegados")
                        .HasForeignKey("ClubId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Api.Core.Entidades.Usuario", "Usuario")
                        .WithMany()
                        .HasForeignKey("UsuarioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Club");

                    b.Navigation("Usuario");
                });

            modelBuilder.Entity("Api.Core.Entidades.Equipo", b =>
                {
                    b.HasOne("Api.Core.Entidades.Club", "Club")
                        .WithMany("Equipos")
                        .HasForeignKey("ClubId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Api.Core.Entidades.Torneo", "Torneo")
                        .WithMany("Equipos")
                        .HasForeignKey("TorneoId");

                    b.Navigation("Club");

                    b.Navigation("Torneo");
                });

            modelBuilder.Entity("Api.Core.Entidades.HistorialDePagos", b =>
                {
                    b.HasOne("Api.Core.Entidades.JugadorEquipo", "JugadorEquipo")
                        .WithOne("HistorialDePagos")
                        .HasForeignKey("Api.Core.Entidades.HistorialDePagos", "JugadorEquipoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("JugadorEquipo");
                });

            modelBuilder.Entity("Api.Core.Entidades.JugadorEquipo", b =>
                {
                    b.HasOne("Api.Core.Entidades.Equipo", "Equipo")
                        .WithMany("Jugadores")
                        .HasForeignKey("EquipoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Api.Core.Entidades.EstadoJugador", "EstadoJugador")
                        .WithMany("JugadorEquipos")
                        .HasForeignKey("EstadoJugadorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Api.Core.Entidades.Jugador", "Jugador")
                        .WithMany("JugadorEquipos")
                        .HasForeignKey("JugadorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Equipo");

                    b.Navigation("EstadoJugador");

                    b.Navigation("Jugador");
                });

            modelBuilder.Entity("Api.Core.Entidades.Club", b =>
                {
                    b.Navigation("Delegados");

                    b.Navigation("Equipos");
                });

            modelBuilder.Entity("Api.Core.Entidades.Equipo", b =>
                {
                    b.Navigation("Jugadores");
                });

            modelBuilder.Entity("Api.Core.Entidades.EstadoJugador", b =>
                {
                    b.Navigation("JugadorEquipos");
                });

            modelBuilder.Entity("Api.Core.Entidades.Jugador", b =>
                {
                    b.Navigation("JugadorEquipos");
                });

            modelBuilder.Entity("Api.Core.Entidades.JugadorEquipo", b =>
                {
                    b.Navigation("HistorialDePagos");
                });

            modelBuilder.Entity("Api.Core.Entidades.Torneo", b =>
                {
                    b.Navigation("Equipos");
                });
#pragma warning restore 612, 618
        }
    }
}
