using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;

namespace Api.TestsDeIntegracion;

public class EliminacionIT : TestBase
{
    private Club? _club;

    public EliminacionIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedData(context);
    }

    private void SeedData(AppDbContext context)
    {
        _club = new Club { Id = 0, Nombre = "Club Test" };
        context.Clubs.Add(_club);
        context.SaveChanges();
    }

    [Fact]
    public async Task EliminarTorneo_EquiposSiguenExistiendo()
    {
        var client = await GetAuthenticatedClient();

        int torneoId;
        int equipoId;

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var torneo = new Torneo { Id = 0, Nombre = "Torneo a Eliminar", Anio = 2026, TorneoAgrupadorId = 1 };
            context.Torneos.Add(torneo);
            context.SaveChanges();
            torneoId = context.Torneos.First(t => t.Nombre == "Torneo a Eliminar").Id;

            var fase = new TorneoFase { Id = 0, TorneoId = torneoId, Numero = 1, FaseFormatoId = 1, EstadoFaseId = 100, EsVisibleEnApp = true };
            context.TorneoFases.Add(fase);
            context.SaveChanges();
            var zona = new TorneoZona { Id = 0, TorneoFaseId = fase.Id, Nombre = "Zona única" };
            context.TorneoZonas.Add(zona);
            context.SaveChanges();

            var equipo = new Equipo { Id = 0, Nombre = "Equipo en Torneo", ClubId = _club!.Id, ZonaExcluyenteId = zona.Id, Jugadores = [] };
            context.Equipos.Add(equipo);
            context.SaveChanges();
            equipoId = equipo.Id;
        }

        var deleteResponse = await client.DeleteAsync($"/api/torneo/{torneoId}");
        deleteResponse.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Null(context.Torneos.Find(torneoId));
            var equipoRestante = context.Equipos.Find(equipoId);
            Assert.NotNull(equipoRestante);
            Assert.Null(equipoRestante.ZonaExcluyenteId);
        }
    }
}
