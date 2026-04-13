using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Logica;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace Api.TestsDeIntegracion;
public class ClubIT : TestBase
{
    private Club? _club;

    public ClubIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        SeedData(context);
    }

    private void SeedData(AppDbContext context)
    {
        _club = new Club
        {
            Id = 0,
            Nombre = "Club de Prueba"
        };
        
        context.Clubs.Add(_club);
        context.SaveChanges();
    }

    [Fact]
    public async Task ListarClubes_Funciona()
    {
        var client = await GetAuthenticatedClient();
        
        var response = await client.GetAsync("/api/club");
        
        response.EnsureSuccessStatusCode();
        
        var stringResponse = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<List<ClubDTO>>(stringResponse);
        
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task ObtenerClub_PorId_DevuelveCanchaTipo()
    {
        var client = await GetAuthenticatedClient();

        var response = await client.GetAsync($"/api/club/{_club!.Id}");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<ClubDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal((int)CanchaTipoEnum.Consultar, content.CanchaTipoId);
        Assert.Equal(nameof(CanchaTipoEnum.Consultar), content.CanchaTipo);
    }

    [Fact]
    public async Task ModificarClub_ActualizaCanchaTipo()
    {
        var client = await GetAuthenticatedClient();

        var getResponse = await client.GetAsync($"/api/club/{_club!.Id}");
        getResponse.EnsureSuccessStatusCode();
        var dto = JsonConvert.DeserializeObject<ClubDTO>(await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(dto);

        dto.CanchaTipoId = (int)CanchaTipoEnum.Cubierta;
        var putResponse = await client.PutAsJsonAsync($"/api/club/{_club.Id}", dto);
        putResponse.EnsureSuccessStatusCode();

        var verifyResponse = await client.GetAsync($"/api/club/{_club.Id}");
        verifyResponse.EnsureSuccessStatusCode();
        var actualizado = JsonConvert.DeserializeObject<ClubDTO>(await verifyResponse.Content.ReadAsStringAsync());
        Assert.NotNull(actualizado);
        Assert.Equal((int)CanchaTipoEnum.Cubierta, actualizado.CanchaTipoId);
        Assert.Equal(nameof(CanchaTipoEnum.Cubierta), actualizado.CanchaTipo);
    }

    [Fact]
    public async Task ObtenerClub_PorIds_DevuelveClubesSolicitados()
    {
        var client = await GetAuthenticatedClient();

        int club2Id;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var club2 = new Club { Id = 0, Nombre = "Club Segundo" };
            context.Clubs.Add(club2);
            context.SaveChanges();
            club2Id = club2.Id;
        }

        var response = await client.GetAsync($"/api/club/por-ids?ids={_club!.Id}&ids={club2Id}");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<ClubDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(2, content.Count);
        Assert.Contains(content, c => c.Id == _club.Id && c.Nombre == "Club de Prueba");
        Assert.Contains(content, c => c.Id == club2Id && c.Nombre == "Club Segundo");
    }

    [Fact]
    public async Task ObtenerClub_PorIds_ListaVacia_DevuelveListaVacia()
    {
        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync("/api/club/por-ids");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<List<ClubDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Empty(content);
    }

    [Fact]
    public async Task ObtenerClub_PorId_DevuelveEscudoEnBase64()
    {
        const string fotoBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";
        var client = await GetAuthenticatedClient();
        await client.PostAsJsonAsync($"/api/club/{_club!.Id}/cambiar-escudo", new CambiarEscudoDTO { ImagenBase64 = fotoBase64 });

        var response = await client.GetAsync($"/api/club/{_club.Id}");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<ClubDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.NotNull(content.Escudo);
        Assert.StartsWith("data:image/", content.Escudo);
        Assert.Contains("base64,", content.Escudo);
    }

    [Fact]
    public async Task CambiarEscudo_ConImagenValida_GuardaEscudo()
    {
        const string fotoBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";
        var client = await GetAuthenticatedClient();

        var response = await client.PostAsJsonAsync($"/api/club/{_club!.Id}/cambiar-escudo", new CambiarEscudoDTO { ImagenBase64 = fotoBase64 });
        response.EnsureSuccessStatusCode();

        var getResponse = await client.GetAsync($"/api/club/{_club.Id}");
        getResponse.EnsureSuccessStatusCode();
        var club = JsonConvert.DeserializeObject<ClubDTO>(await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(club);
        Assert.StartsWith("data:image/", club.Escudo);
        Assert.Contains("base64,", club.Escudo);
    }

    [Fact]
    public async Task EliminarClub_EliminaClubYEscudo()
    {
        const string fotoBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";
        var client = await GetAuthenticatedClient();

        var clubParaEliminar = new Club { Id = 0, Nombre = "Club a Eliminar" };
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Clubs.Add(clubParaEliminar);
            context.SaveChanges();
        }

        await client.PostAsJsonAsync($"/api/club/{clubParaEliminar.Id}/cambiar-escudo", new CambiarEscudoDTO { ImagenBase64 = fotoBase64 });

        var deleteResponse = await client.DeleteAsync($"/api/club/{clubParaEliminar.Id}");
        deleteResponse.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var clubEliminado = context.Clubs.Find(clubParaEliminar.Id);
            Assert.Null(clubEliminado);

            var paths = scope.ServiceProvider.GetRequiredService<AppPaths>();
            var pathEscudo = Path.Combine(paths.ImagenesEscudosAbsolute, $"{clubParaEliminar.Id}.jpg");
            Assert.False(File.Exists(pathEscudo));
        }
    }

    [Fact]
    public async Task EliminarClub_EliminaEquiposYDelegadosSoloDeEseClub()
    {
        var client = await GetAuthenticatedClient();

        var clubParaEliminar = new Club { Id = 0, Nombre = "Club a Eliminar" };
        var torneo = new Torneo { Id = 0, Nombre = "Torneo Test", Anio = 2026, TorneoAgrupadorId = 1, EsVisibleEnApp = true, SeVenLosGolesEnTablaDePosiciones = true };
        var equipo1 = new Equipo { Id = 0, Nombre = "Equipo 1", ClubId = 0, Jugadores = [] };
        var delegadoSoloEnEsteClub = new Delegado { Id = 0, DNI = "11223344", Nombre = "Solo", Apellido = "Club", FechaNacimiento = new DateTime(1990, 1, 1) };
        var delegadoEnVariosClubs = new Delegado { Id = 0, DNI = "55667788", Nombre = "Varios", Apellido = "Clubs", FechaNacimiento = new DateTime(1990, 1, 1) };

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Clubs.Add(clubParaEliminar);
            context.Torneos.Add(torneo);
            context.SaveChanges();

            clubParaEliminar = context.Clubs.First(c => c.Nombre == "Club a Eliminar");
            torneo = context.Torneos.First();

            var fase = new FaseTodosContraTodos { Id = 0, Nombre = "", TorneoId = torneo.Id, Numero = 1, EstadoFaseId = 100, EsVisibleEnApp = true };
            context.Fases.Add(fase);
            context.SaveChanges();
            var zona = new ZonaTodosContraTodos { Id = 0, FaseId = fase.Id, Nombre = "Zona única" };
            context.Zonas.Add(zona);
            context.SaveChanges();

            equipo1.ClubId = clubParaEliminar.Id;
            context.Equipos.Add(equipo1);
            context.SaveChanges();
            context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = equipo1.Id, ZonaId = zona.Id });

            context.Delegados.Add(delegadoSoloEnEsteClub);
            context.Delegados.Add(delegadoEnVariosClubs);
            context.SaveChanges();

            delegadoSoloEnEsteClub = context.Delegados.First(d => d.DNI == "11223344");
            delegadoEnVariosClubs = context.Delegados.First(d => d.DNI == "55667788");

            context.DelegadoClub.Add(new DelegadoClub { Id = 0, DelegadoId = delegadoSoloEnEsteClub.Id, ClubId = clubParaEliminar.Id, EstadoDelegadoId = (int)EstadoDelegadoEnum.Activo });
            context.DelegadoClub.Add(new DelegadoClub { Id = 0, DelegadoId = delegadoEnVariosClubs.Id, ClubId = clubParaEliminar.Id, EstadoDelegadoId = (int)EstadoDelegadoEnum.Activo });
            context.DelegadoClub.Add(new DelegadoClub { Id = 0, DelegadoId = delegadoEnVariosClubs.Id, ClubId = _club!.Id, EstadoDelegadoId = (int)EstadoDelegadoEnum.Activo });
            context.SaveChanges();
        }

        var deleteResponse = await client.DeleteAsync($"/api/club/{clubParaEliminar.Id}");
        deleteResponse.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Null(context.Clubs.Find(clubParaEliminar.Id));
            Assert.Empty(context.Equipos.Where(e => e.ClubId == clubParaEliminar.Id));
            Assert.Null(context.Delegados.Find(delegadoSoloEnEsteClub.Id));
            Assert.NotNull(context.Delegados.Find(delegadoEnVariosClubs.Id));
            Assert.Empty(context.DelegadoClub.Where(dc => dc.ClubId == clubParaEliminar.Id));
        }
    }
}