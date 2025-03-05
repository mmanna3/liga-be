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

                    b.ToTable("Clubs", (string)null);
                });

            modelBuilder.Entity("Api.Core.Entidades.Delegado", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Apellido")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ClubId")
                        .HasColumnType("int");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Usuario")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ClubId");

                    b.ToTable("Delegados", (string)null);
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
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ClubId");

                    b.ToTable("Equipos", (string)null);
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

                    b.ToTable("EstadoJugador", (string)null);

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
                        });
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

                    b.ToTable("Jugadores", (string)null);
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

                    b.Property<string>("MotivoDeRechazoFichaje")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.HasKey("Id");

                    b.HasIndex("EquipoId");

                    b.HasIndex("EstadoJugadorId");

                    b.HasIndex("JugadorId");

                    b.ToTable("JugadorEquipo", (string)null);
                });

            modelBuilder.Entity("Api.Core.Entidades.Delegado", b =>
                {
                    b.HasOne("Api.Core.Entidades.Club", "Club")
                        .WithMany()
                        .HasForeignKey("ClubId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Club");
                });

            modelBuilder.Entity("Api.Core.Entidades.Equipo", b =>
                {
                    b.HasOne("Api.Core.Entidades.Club", "Club")
                        .WithMany("Equipos")
                        .HasForeignKey("ClubId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Club");
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
#pragma warning restore 612, 618
        }
    }
}
