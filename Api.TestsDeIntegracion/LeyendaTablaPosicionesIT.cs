using System.Net;
using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Api.TestsUtilidades;
using Microsoft.Extensions.DependencyInjection;

namespace Api.TestsDeIntegracion;

/// <summary>
/// Reglas de unicidad (ZonaId, CategoriaId, EquipoId) y validación de <see cref="LeyendaTablaPosiciones.QuitaDePuntos"/>.
/// </summary>
public class LeyendaTablaPosicionesIT : TestBase
{
    public LeyendaTablaPosicionesIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private static async Task<(int ZonaId, int CategoriaId, int EquipoEnZona1, int EquipoEnZona2, int EquipoFueraDeZona)>
        SeedZonaCategoriaYTresEquipos(CustomWebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var torneo = new Torneo
        {
            Id = 0,
            Nombre = "Torneo Leyendas IT",
            Anio = 2026,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };
        context.Torneos.Add(torneo);
        await context.SaveChangesAsync();

        var fase = new FaseTodosContraTodos
        {
            Id = 0,
            Nombre = "",
            Numero = 1,
            TorneoId = torneo.Id,
            EstadoFaseId = 100,
            EsVisibleEnApp = true
        };
        context.Fases.Add(fase);
        await context.SaveChangesAsync();

        var cat = await CategoriasDePrueba.AgregarFaseCategoria(context, fase.Id, "Cat Leyenda", 1);

        var zona = new ZonaTodosContraTodos { Id = 0, Nombre = "Zona leyendas", FaseId = fase.Id, Orden = 1 };
        context.Zonas.Add(zona);
        await context.SaveChangesAsync();

        var club = new Club { Id = 0, Nombre = "Club Leyendas IT" };
        context.Clubs.Add(club);
        await context.SaveChangesAsync();

        var e1 = new Equipo
        {
            Id = 0,
            Nombre = "Equipo Alpha",
            ClubId = club.Id,
            Club = club,
            Jugadores = new List<JugadorEquipo>()
        };
        var e2 = new Equipo
        {
            Id = 0,
            Nombre = "Equipo Beta",
            ClubId = club.Id,
            Club = club,
            Jugadores = new List<JugadorEquipo>()
        };
        var e3 = new Equipo
        {
            Id = 0,
            Nombre = "Equipo Gamma",
            ClubId = club.Id,
            Club = club,
            Jugadores = new List<JugadorEquipo>()
        };
        context.Equipos.AddRange(e1, e2, e3);
        await context.SaveChangesAsync();

        context.Set<EquipoZona>().AddRange(
            new EquipoZona { Id = 0, EquipoId = e1.Id, ZonaId = zona.Id },
            new EquipoZona { Id = 0, EquipoId = e2.Id, ZonaId = zona.Id });
        await context.SaveChangesAsync();

        return (zona.Id, cat.Id, e1.Id, e2.Id, e3.Id);
    }

    [Fact]
    public async Task PostLeyenda_SinEquipo_GeneralPorCategoria_OK_y_Duplicado_400()
    {
        var (zonaId, catId, _, _, _) = await SeedZonaCategoriaYTresEquipos(Factory);
        var client = await GetAuthenticatedClient();

        var dto1 = new LeyendaTablaPosicionesDTO
        {
            Id = 0,
            Leyenda = "Leyenda base",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = null,
            QuitaDePuntos = 0
        };
        var ok = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", dto1);
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);

        var dtoDup = new LeyendaTablaPosicionesDTO
        {
            Id = 0,
            Leyenda = "Otra",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = null,
            QuitaDePuntos = 0
        };
        var bad = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", dtoDup);
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
    }

    [Fact]
    public async Task PostLeyenda_ConDosEquiposDistintos_MismaCategoria_OK()
    {
        var (zonaId, catId, eq1, eq2, _) = await SeedZonaCategoriaYTresEquipos(Factory);
        var client = await GetAuthenticatedClient();

        var baseDto = new LeyendaTablaPosicionesDTO
        {
            Id = 0,
            Leyenda = "General cat",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = null,
            QuitaDePuntos = 0
        };
        (await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", baseDto)).EnsureSuccessStatusCode();

        var a = new LeyendaTablaPosicionesDTO
        {
            Id = 0,
            Leyenda = "Sanción A",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = eq1,
            QuitaDePuntos = 2
        };
        var r1 = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", a);
        Assert.Equal(HttpStatusCode.OK, r1.StatusCode);

        var b = new LeyendaTablaPosicionesDTO
        {
            Id = 0,
            Leyenda = "Sanción B",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = eq2,
            QuitaDePuntos = 5
        };
        var r2 = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", b);
        Assert.Equal(HttpStatusCode.OK, r2.StatusCode);
    }

    [Fact]
    public async Task PostLeyenda_MismoEquipoDuplicado_400()
    {
        var (zonaId, catId, eq1, _, _) = await SeedZonaCategoriaYTresEquipos(Factory);
        var client = await GetAuthenticatedClient();

        (await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", new LeyendaTablaPosicionesDTO
        {
            Id = 0,
            Leyenda = "G",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = null,
            QuitaDePuntos = 0
        })).EnsureSuccessStatusCode();

        var dto = new LeyendaTablaPosicionesDTO
        {
            Id = 0,
            Leyenda = "S1",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = eq1,
            QuitaDePuntos = 1
        };
        (await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", dto)).EnsureSuccessStatusCode();

        var dup = new LeyendaTablaPosicionesDTO
        {
            Id = 0,
            Leyenda = "S2",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = eq1,
            QuitaDePuntos = 1
        };
        var res = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", dup);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task PostLeyenda_ConEquipoYQuitaCero_400()
    {
        var (zonaId, catId, eq1, _, _) = await SeedZonaCategoriaYTresEquipos(Factory);
        var client = await GetAuthenticatedClient();

        (await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", new LeyendaTablaPosicionesDTO
        {
            Id = 0,
            Leyenda = "G",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = null,
            QuitaDePuntos = 0
        })).EnsureSuccessStatusCode();

        var res = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", new LeyendaTablaPosicionesDTO
        {
            Id = 0,
            Leyenda = "X",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = eq1,
            QuitaDePuntos = 0
        });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task PostLeyenda_SinEquipoYQuitaDistintaDeCero_400()
    {
        var (zonaId, catId, _, _, _) = await SeedZonaCategoriaYTresEquipos(Factory);
        var client = await GetAuthenticatedClient();

        var res = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", new LeyendaTablaPosicionesDTO
        {
            Id = 0,
            Leyenda = "X",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = null,
            QuitaDePuntos = 3
        });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task PostLeyenda_EquipoNoEnZona_400()
    {
        var (zonaId, catId, _, _, eqFuera) = await SeedZonaCategoriaYTresEquipos(Factory);
        var client = await GetAuthenticatedClient();

        (await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", new LeyendaTablaPosicionesDTO
        {
            Id = 0,
            Leyenda = "G",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = null,
            QuitaDePuntos = 0
        })).EnsureSuccessStatusCode();

        var res = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", new LeyendaTablaPosicionesDTO
        {
            Id = 0,
            Leyenda = "X",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = eqFuera,
            QuitaDePuntos = 2
        });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task GetLeyendas_ListaIncluyeEquipoIdYQuitaMapeados()
    {
        var (zonaId, catId, eq1, _, _) = await SeedZonaCategoriaYTresEquipos(Factory);
        var client = await GetAuthenticatedClient();

        (await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", new LeyendaTablaPosicionesDTO
        {
            Id = 0,
            Leyenda = "G",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = null,
            QuitaDePuntos = 0
        })).EnsureSuccessStatusCode();

        (await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", new LeyendaTablaPosicionesDTO
        {
            Id = 0,
            Leyenda = "Eq",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = eq1,
            QuitaDePuntos = 4
        })).EnsureSuccessStatusCode();

        var list = await client.GetFromJsonAsync<List<LeyendaTablaPosicionesDTO>>($"/api/Zona/{zonaId}/leyendas");
        Assert.NotNull(list);
        Assert.Equal(2, list.Count);
        var conEquipo = list.Single(x => x.EquipoId == eq1);
        Assert.Equal(4, conEquipo.QuitaDePuntos);
        Assert.Equal("Equipo Alpha", conEquipo.Equipo);
        var sinEquipo = list.Single(x => x.EquipoId == null);
        Assert.Null(sinEquipo.EquipoId);
        Assert.Null(sinEquipo.Equipo);
        Assert.Equal(0, sinEquipo.QuitaDePuntos);
    }

    [Fact]
    public async Task PutLeyenda_ModificaTextoCategoriaYAgregaQuitaDePuntos_OK()
    {
        var (zonaId, catId, eq1, _, _) = await SeedZonaCategoriaYTresEquipos(Factory);
        var client = await GetAuthenticatedClient();

        var creada = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", new LeyendaTablaPosicionesDTO
        {
            Id = 0,
            Leyenda = "Original",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = null,
            QuitaDePuntos = 0
        });
        creada.EnsureSuccessStatusCode();
        var dto = await creada.Content.ReadFromJsonAsync<LeyendaTablaPosicionesDTO>();
        Assert.NotNull(dto);

        var editada = new LeyendaTablaPosicionesDTO
        {
            Id = dto!.Id,
            Leyenda = "Modificada",
            CategoriaId = null, // pasa a general
            ZonaId = zonaId,
            EquipoId = eq1,
            QuitaDePuntos = 3
        };
        var put = await client.PutAsJsonAsync($"/api/Zona/{zonaId}/leyendas/{dto.Id}", editada);
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var list = await client.GetFromJsonAsync<List<LeyendaTablaPosicionesDTO>>($"/api/Zona/{zonaId}/leyendas");
        Assert.NotNull(list);
        var actualizada = Assert.Single(list);
        Assert.Equal("Modificada", actualizada.Leyenda);
        Assert.Null(actualizada.CategoriaId);
        Assert.Equal(eq1, actualizada.EquipoId);
        Assert.Equal(3, actualizada.QuitaDePuntos);
    }

    [Fact]
    public async Task PutLeyenda_MismaZonaCategoriaYEquipoQueEllaMisma_NoChocaConsigoMisma_OK()
    {
        var (zonaId, catId, eq1, _, _) = await SeedZonaCategoriaYTresEquipos(Factory);
        var client = await GetAuthenticatedClient();

        var creada = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", new LeyendaTablaPosicionesDTO
        {
            Id = 0,
            Leyenda = "Texto",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = eq1,
            QuitaDePuntos = 2
        });
        creada.EnsureSuccessStatusCode();
        var dto = await creada.Content.ReadFromJsonAsync<LeyendaTablaPosicionesDTO>();

        var editada = new LeyendaTablaPosicionesDTO
        {
            Id = dto!.Id,
            Leyenda = "Texto editado",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = eq1,
            QuitaDePuntos = 2
        };
        var put = await client.PutAsJsonAsync($"/api/Zona/{zonaId}/leyendas/{dto.Id}", editada);
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);
    }

    [Fact]
    public async Task PutLeyenda_ColisionaConOtraExistente_400()
    {
        var (zonaId, catId, eq1, eq2, _) = await SeedZonaCategoriaYTresEquipos(Factory);
        var client = await GetAuthenticatedClient();

        var r1 = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", new LeyendaTablaPosicionesDTO
        {
            Id = 0, Leyenda = "Uno", CategoriaId = catId, ZonaId = zonaId, EquipoId = eq1, QuitaDePuntos = 1
        });
        r1.EnsureSuccessStatusCode();

        var r2 = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", new LeyendaTablaPosicionesDTO
        {
            Id = 0, Leyenda = "Dos", CategoriaId = catId, ZonaId = zonaId, EquipoId = eq2, QuitaDePuntos = 2
        });
        r2.EnsureSuccessStatusCode();
        var dto2 = await r2.Content.ReadFromJsonAsync<LeyendaTablaPosicionesDTO>();

        // Editar "Dos" para que quede con la misma (zona, categoría, equipo) que "Uno".
        var editada = new LeyendaTablaPosicionesDTO
        {
            Id = dto2!.Id,
            Leyenda = "Dos modificada",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = eq1,
            QuitaDePuntos = 5
        };
        var put = await client.PutAsJsonAsync($"/api/Zona/{zonaId}/leyendas/{dto2.Id}", editada);
        Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
    }

    [Fact]
    public async Task PutLeyenda_EquipoFueraDeZona_400()
    {
        var (zonaId, catId, eq1, _, eqFuera) = await SeedZonaCategoriaYTresEquipos(Factory);
        var client = await GetAuthenticatedClient();

        var creada = await client.PostAsJsonAsync($"/api/Zona/{zonaId}/leyendas", new LeyendaTablaPosicionesDTO
        {
            Id = 0,
            Leyenda = "Texto",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = eq1,
            QuitaDePuntos = 2
        });
        creada.EnsureSuccessStatusCode();
        var dto = await creada.Content.ReadFromJsonAsync<LeyendaTablaPosicionesDTO>();

        var editada = new LeyendaTablaPosicionesDTO
        {
            Id = dto!.Id,
            Leyenda = "Texto",
            CategoriaId = catId,
            ZonaId = zonaId,
            EquipoId = eqFuera,
            QuitaDePuntos = 2
        };
        var put = await client.PutAsJsonAsync($"/api/Zona/{zonaId}/leyendas/{dto.Id}", editada);
        Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
    }
}
