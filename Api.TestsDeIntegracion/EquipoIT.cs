using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Api.TestsUtilidades;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace Api.TestsDeIntegracion;
public class EquipoIT : TestBase
{
    private Utilidades? _utilidades;
    private Club? _club;

    public EquipoIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        SeedData(context);
    }

    private void SeedData(AppDbContext context)
    {
        _utilidades = new Utilidades(context);
        _club = _utilidades.DadoQueExisteElClub();
        _utilidades.DadoQueExisteElEquipo(_club);
        context.SaveChanges();
    }

    [Fact]
    public async Task ListarEquipos_Funciona()
    {
        var client = await GetAuthenticatedClient();
        
        var response = await client.GetAsync("/api/equipo");
        
        response.EnsureSuccessStatusCode();
        
        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<List<EquipoDTO>>(stringResponse);
        
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }
    
    [Fact]
    public async Task CrearEquipo_DatosCorrectos_200()
    {
        var client = await GetAuthenticatedClient();
        
        var equipoDTO = new EquipoDTO
        {
            Nombre = "Nuevo Equipo",
            ClubId = 1
        };
        
        var response = await client.PostAsJsonAsync("/api/equipo", equipoDTO);
        
        response.EnsureSuccessStatusCode();
        
        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<EquipoDTO>(stringResponse);
        
        Assert.NotNull(content);
        Assert.Equal("Nuevo Equipo", content.Nombre);
    }

    [Fact]
    public async Task EliminarEquipo_EliminaJugadoresQueSoloJugabanEnEseEquipo()
    {
        var client = await GetAuthenticatedClient();

        int equipoId;
        int jugadorSoloEnEsteEquipoId;
        int jugadorEnVariosEquiposId;

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var torneo = new Torneo { Id = 0, Nombre = "Torneo Elim", Anio = 2026, TorneoAgrupadorId = 1 };
            context.Torneos.Add(torneo);
            context.SaveChanges();
            torneo = context.Torneos.First();

            var fase = new FaseTodosContraTodos { Id = 0, Nombre = "", TorneoId = torneo.Id, Numero = 1, EstadoFaseId = 100, EsVisibleEnApp = true };
            context.Fases.Add(fase);
            context.SaveChanges();
            var zona = new ZonaTodosContraTodos { Id = 0, FaseId = fase.Id, Nombre = "Zona única" };
            context.Zonas.Add(zona);
            context.SaveChanges();

            var equipoOtro = context.Equipos.First(e => e.ClubId == _club!.Id);

            var equipoParaEliminar = new Equipo { Id = 0, Nombre = "Equipo a Eliminar", ClubId = _club!.Id, Jugadores = [], Zonas = new List<EquipoZona>() };
            context.Equipos.Add(equipoParaEliminar);
            context.SaveChanges();
            context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = equipoParaEliminar.Id, ZonaId = zona.Id });
            context.SaveChanges();
            equipoId = equipoParaEliminar.Id;

            var jugadorSolo = new Jugador { Id = 0, DNI = "11112222", Nombre = "Solo", Apellido = "Equipo", FechaNacimiento = new DateTime(1995, 1, 1) };
            var jugadorVarios = new Jugador { Id = 0, DNI = "33334444", Nombre = "Varios", Apellido = "Equipos", FechaNacimiento = new DateTime(1995, 1, 1) };
            context.Jugadores.Add(jugadorSolo);
            context.Jugadores.Add(jugadorVarios);
            context.SaveChanges();
            jugadorSoloEnEsteEquipoId = jugadorSolo.Id;
            jugadorEnVariosEquiposId = jugadorVarios.Id;

            context.JugadorEquipo.Add(new JugadorEquipo { Id = 0, JugadorId = jugadorSolo.Id, EquipoId = equipoId, FechaFichaje = DateTime.Now, EstadoJugadorId = (int)EstadoJugadorEnum.Activo });
            context.JugadorEquipo.Add(new JugadorEquipo { Id = 0, JugadorId = jugadorVarios.Id, EquipoId = equipoId, FechaFichaje = DateTime.Now, EstadoJugadorId = (int)EstadoJugadorEnum.Activo });
            context.JugadorEquipo.Add(new JugadorEquipo { Id = 0, JugadorId = jugadorVarios.Id, EquipoId = equipoOtro.Id, FechaFichaje = DateTime.Now, EstadoJugadorId = (int)EstadoJugadorEnum.Activo });
            context.SaveChanges();
        }

        var deleteResponse = await client.DeleteAsync($"/api/equipo/{equipoId}");
        deleteResponse.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Null(context.Equipos.Find(equipoId));
            Assert.Null(context.Jugadores.Find(jugadorSoloEnEsteEquipoId));
            Assert.NotNull(context.Jugadores.Find(jugadorEnVariosEquiposId));
            Assert.Empty(context.JugadorEquipo.Where(je => je.EquipoId == equipoId));
        }
    }
}