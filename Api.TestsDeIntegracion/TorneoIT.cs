using System.Net;
using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace Api.TestsDeIntegracion;

public class TorneoIT : TestBase
{
    public TorneoIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedData(context);
    }

    private static void SeedData(AppDbContext context)
    {
        // TorneoAgrupador "General" (Id=1) ya existe por HasData en AppDbContext
    }

    [Fact]
    public async Task CrearTorneo_ConFaseYCategorias_200()
    {
        var client = await GetAuthenticatedClient();

        var dto = new CrearTorneoDTO
        {
            Nombre = "Torneo con Fase y Categorías",
            Anio = 2026,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true,
            PrimeraFase = new FaseDTO
            {
                Nombre = "Fase de grupos",
                Numero = 1,
                TipoDeFase = TipoDeFaseEnum.TodosContraTodos,
                EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
                EsVisibleEnApp = true
            },
            Categorias =
            [
                new TorneoCategoriaDTO { Nombre = "Sub-15", AnioDesde = 2010, AnioHasta = 2015 },
                new TorneoCategoriaDTO { Nombre = "Sub-18", AnioDesde = 2007, AnioHasta = 2009 }
            ]
        };

        var response = await client.PostAsJsonAsync("/api/torneo", dto);

        response.EnsureSuccessStatusCode();

        var torneoCreado = JsonConvert.DeserializeObject<CrearTorneoDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(torneoCreado);
        Assert.True(torneoCreado.Id > 0);
        Assert.Equal("Torneo con Fase y Categorías", torneoCreado.Nombre);
        Assert.Equal(2026, torneoCreado.Anio);
        Assert.True(torneoCreado.SeVenLosGolesEnTablaDePosiciones);

        var torneoId = torneoCreado.Id;

        var fasesResponse = await client.GetAsync($"/api/Torneo/{torneoId}/fases");
        fasesResponse.EnsureSuccessStatusCode();
        var fases = JsonConvert.DeserializeObject<List<FaseDTO>>(await fasesResponse.Content.ReadAsStringAsync());
        Assert.NotNull(fases);
        Assert.Single(fases);
        Assert.Equal("Fase de grupos", fases[0].Nombre);
        Assert.Equal(1, fases[0].Numero);
        Assert.Equal("Todos contra todos", fases[0].TipoDeFaseNombre);

        var categoriasResponse = await client.GetAsync($"/api/Torneo/{torneoId}/categorias");
        categoriasResponse.EnsureSuccessStatusCode();
        var categorias = JsonConvert.DeserializeObject<List<TorneoCategoriaDTO>>(await categoriasResponse.Content.ReadAsStringAsync());
        Assert.NotNull(categorias);
        Assert.Equal(2, categorias.Count);
        Assert.Contains(categorias, c => c.Nombre == "Sub-15" && c.AnioDesde == 2010 && c.AnioHasta == 2015);
        Assert.Contains(categorias, c => c.Nombre == "Sub-18" && c.AnioDesde == 2007 && c.AnioHasta == 2009);
    }

    [Fact]
    public async Task CrearTorneo_SinFaseNiCategorias_CreaFasePorDefecto()
    {
        var client = await GetAuthenticatedClient();

        var dto = new CrearTorneoDTO
        {
            Nombre = "Torneo básico",
            Anio = 2026,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };

        var response = await client.PostAsJsonAsync("/api/torneo", dto);

        response.EnsureSuccessStatusCode();

        var torneoCreado = JsonConvert.DeserializeObject<CrearTorneoDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(torneoCreado);
        Assert.True(torneoCreado.Id > 0);

        var torneoId = torneoCreado.Id;

        var fasesResponse = await client.GetAsync($"/api/Torneo/{torneoId}/fases");
        fasesResponse.EnsureSuccessStatusCode();
        var fases = JsonConvert.DeserializeObject<List<FaseDTO>>(await fasesResponse.Content.ReadAsStringAsync());
        Assert.NotNull(fases);
        Assert.Single(fases);
        Assert.Equal(1, fases[0].Numero);
        Assert.Equal("Todos contra todos", fases[0].TipoDeFaseNombre);

        var categoriasResponse = await client.GetAsync($"/api/Torneo/{torneoId}/categorias");
        categoriasResponse.EnsureSuccessStatusCode();
        var categorias = JsonConvert.DeserializeObject<List<TorneoCategoriaDTO>>(await categoriasResponse.Content.ReadAsStringAsync());
        Assert.NotNull(categorias);
        Assert.Empty(categorias);
    }

    [Fact]
    public async Task CrearTorneo_SoloCategorias_CreaFasePorDefectoYCategorias()
    {
        var client = await GetAuthenticatedClient();

        var dto = new CrearTorneoDTO
        {
            Nombre = "Torneo con categorías",
            Anio = 2026,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true,
            Categorias =
            [
                new TorneoCategoriaDTO { Nombre = "Primera", AnioDesde = 2005, AnioHasta = 2010 }
            ]
        };

        var response = await client.PostAsJsonAsync("/api/torneo", dto);

        response.EnsureSuccessStatusCode();

        var torneoCreado = JsonConvert.DeserializeObject<CrearTorneoDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(torneoCreado);
        var torneoId = torneoCreado.Id;

        var fasesResponse = await client.GetAsync($"/api/Torneo/{torneoId}/fases");
        fasesResponse.EnsureSuccessStatusCode();
        var fases = JsonConvert.DeserializeObject<List<FaseDTO>>(await fasesResponse.Content.ReadAsStringAsync());
        Assert.NotNull(fases);
        Assert.Single(fases);

        var categoriasResponse = await client.GetAsync($"/api/Torneo/{torneoId}/categorias");
        categoriasResponse.EnsureSuccessStatusCode();
        var categorias = JsonConvert.DeserializeObject<List<TorneoCategoriaDTO>>(await categoriasResponse.Content.ReadAsStringAsync());
        Assert.NotNull(categorias);
        Assert.Single(categorias);
        Assert.Equal("Primera", categorias[0].Nombre);
    }

    [Fact]
    public async Task CrearTorneo_CategoriaAnioDesdeMayorQueAnioHasta_400()
    {
        var client = await GetAuthenticatedClient();

        var dto = new CrearTorneoDTO
        {
            Nombre = "Torneo inválido",
            Anio = 2026,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true,
            Categorias =
            [
                new TorneoCategoriaDTO { Nombre = "Sub-15", AnioDesde = 2015, AnioHasta = 2010 }
            ]
        };

        var response = await client.PostAsJsonAsync("/api/torneo", dto);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var mensaje = await response.Content.ReadAsStringAsync();
        Assert.Contains("año", mensaje.ToLowerInvariant());
    }

    [Fact]
    public async Task CrearTorneo_NombreYAnioDuplicado_400()
    {
        var client = await GetAuthenticatedClient();

        var dto = new CrearTorneoDTO
        {
            Nombre = "Torneo Único",
            Anio = 2026,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };

        var response1 = await client.PostAsJsonAsync("/api/torneo", dto);
        response1.EnsureSuccessStatusCode();

        var response2 = await client.PostAsJsonAsync("/api/torneo", dto);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response2.StatusCode);
        var mensaje = await response2.Content.ReadAsStringAsync();
        Assert.Contains("nombre", mensaje.ToLowerInvariant());
        Assert.Contains("año", mensaje.ToLowerInvariant());
        Assert.Contains("agrupador", mensaje.ToLowerInvariant());
    }

    [Fact]
    public async Task CrearTorneo_MismoNombreYAnioEnDistintoAgrupador_200()
    {
        var client = await GetAuthenticatedClient();

        int agrupador2Id;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var agrupador2 = new TorneoAgrupador
            {
                Id = 0,
                Nombre = "Otro Agrupador",
                EsVisibleEnApp = false,
                ColorId = (int)ColorEnum.Negro
            };
            context.TorneoAgrupadores.Add(agrupador2);
            await context.SaveChangesAsync();
            agrupador2Id = agrupador2.Id;
        }

        var dto = new CrearTorneoDTO
        {
            Nombre = "Torneo Compartido",
            Anio = 2026,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };

        var response1 = await client.PostAsJsonAsync("/api/torneo", dto);
        response1.EnsureSuccessStatusCode();

        dto.TorneoAgrupadorId = agrupador2Id;
        var response2 = await client.PostAsJsonAsync("/api/torneo", dto);
        response2.EnsureSuccessStatusCode();

        var torneo1 = JsonConvert.DeserializeObject<TorneoDTO>(await response1.Content.ReadAsStringAsync());
        var torneo2 = JsonConvert.DeserializeObject<TorneoDTO>(await response2.Content.ReadAsStringAsync());
        Assert.NotNull(torneo1);
        Assert.NotNull(torneo2);
        Assert.NotEqual(torneo1.Id, torneo2.Id);
        Assert.Equal("Torneo Compartido", torneo1.Nombre);
        Assert.Equal("Torneo Compartido", torneo2.Nombre);
    }

    [Fact]
    public async Task ObtenerTorneo_FasesSinZonas_SePuedeEditarTrue()
    {
        int torneoId;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var torneo = new Torneo { Id = 0, Nombre = "Torneo Editable", Anio = 2026, TorneoAgrupadorId = 1, EsVisibleEnApp = true, SeVenLosGolesEnTablaDePosiciones = true };
            context.Torneos.Add(torneo);
            await context.SaveChangesAsync();
            torneoId = torneo.Id;

            var fase = new FaseTodosContraTodos
            {
                Id = 0, Nombre = "", Numero = 1, TorneoId = torneoId,
                EstadoFaseId = 100, EsVisibleEnApp = true
            };
            context.Fases.Add(fase);
            await context.SaveChangesAsync();

            // context.Zonas.Add(new ZonaTodosContraTodos { Id = 0, Nombre = "Zona A", FaseId = fase.Id });
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/torneo/{torneoId}");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<TorneoDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.True(content.SePuedeEditar);
    }

    [Fact]
    public async Task ObtenerTorneo_ConFasesConFechas_SePuedeEditarFalse()
    {
        int torneoId;
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var torneo = new Torneo { Id = 0, Nombre = "Torneo No Editable", Anio = 2026, TorneoAgrupadorId = 1, EsVisibleEnApp = true, SeVenLosGolesEnTablaDePosiciones = true };
            context.Torneos.Add(torneo);
            await context.SaveChangesAsync();
            torneoId = torneo.Id;

            var fase = new FaseTodosContraTodos
            {
                Id = 0, Nombre = "", Numero = 1, TorneoId = torneoId,
                EstadoFaseId = 100, EsVisibleEnApp = true
            };
            context.Fases.Add(fase);
            await context.SaveChangesAsync();

            var zona = new ZonaTodosContraTodos { Id = 0, Nombre = "Zona A", FaseId = fase.Id };
            context.Zonas.Add(zona);
            await context.SaveChangesAsync();

            context.Fechas.Add(new FechaTodosContraTodos
            {
                Id = 0, Dia = new DateOnly(2026, 5, 10), Numero = 1,
                ZonaId = zona.Id, EsVisibleEnApp = true
            });
            await context.SaveChangesAsync();
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"/api/torneo/{torneoId}");
        response.EnsureSuccessStatusCode();

        var content = JsonConvert.DeserializeObject<TorneoDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.False(content.SePuedeEditar);
    }

    [Fact]
    public async Task ModificarTorneo_NombreYAnioDuplicado_400()
    {
        var client = await GetAuthenticatedClient();

        var dto1 = new CrearTorneoDTO { Nombre = "Torneo A", Anio = 2026, TorneoAgrupadorId = 1, EsVisibleEnApp = true, SeVenLosGolesEnTablaDePosiciones = true };
        var dto2 = new CrearTorneoDTO { Nombre = "Torneo B", Anio = 2026, TorneoAgrupadorId = 1, EsVisibleEnApp = true, SeVenLosGolesEnTablaDePosiciones = true };

        var response1 = await client.PostAsJsonAsync("/api/torneo", dto1);
        response1.EnsureSuccessStatusCode();
        var torneoA = JsonConvert.DeserializeObject<TorneoDTO>(await response1.Content.ReadAsStringAsync());

        var response2 = await client.PostAsJsonAsync("/api/torneo", dto2);
        response2.EnsureSuccessStatusCode();
        var torneoB = JsonConvert.DeserializeObject<TorneoDTO>(await response2.Content.ReadAsStringAsync());

        Assert.NotNull(torneoA);
        Assert.NotNull(torneoB);

        var dtoModificar = new TorneoDTO
        {
            Id = torneoB.Id,
            Nombre = "Torneo A",
            Anio = 2026,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };

        var responseModificar = await client.PutAsJsonAsync($"/api/torneo/{torneoB.Id}", dtoModificar);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, responseModificar.StatusCode);
        var mensaje = await responseModificar.Content.ReadAsStringAsync();
        Assert.Contains("nombre", mensaje.ToLowerInvariant());
        Assert.Contains("año", mensaje.ToLowerInvariant());
        Assert.Contains("agrupador", mensaje.ToLowerInvariant());
    }

    [Fact]
    public async Task Filtrar_PorAnioYAgrupador_DevuelveTorneosConFases()
    {
        var client = await GetAuthenticatedClient();

        var dto = new CrearTorneoDTO
        {
            Nombre = "Torneo Filtrado",
            Anio = 2025,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true,
            PrimeraFase = new FaseDTO
            {
                Nombre = "Fase grupos",
                Numero = 1,
                TipoDeFase = TipoDeFaseEnum.TodosContraTodos,
                EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
                EsVisibleEnApp = true
            }
        };
        await client.PostAsJsonAsync("/api/torneo", dto);

        var response = await client.GetAsync("/api/torneo/filtrar?anio=2025&agrupador=1");
        response.EnsureSuccessStatusCode();

        var torneos = JsonConvert.DeserializeObject<List<TorneoDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(torneos);
        Assert.NotEmpty(torneos);
        var torneo = torneos.First(t => t.Nombre == "Torneo Filtrado");
        Assert.Equal(2025, torneo.Anio);
        Assert.Equal(1, torneo.TorneoAgrupadorId);
        Assert.NotEmpty(torneo.Fases!);
        var fase = torneo.Fases![0];
        Assert.Equal("Fase grupos", fase.Nombre);
        Assert.Equal(1, fase.Numero);
        Assert.Equal("Todos contra todos", fase.TipoDeFaseNombre);
        Assert.True(fase.EsVisibleEnApp);
    }

    [Fact]
    public async Task ObtenerPorId_DevuelveTorneoConFasesYCategorias()
    {
        var client = await GetAuthenticatedClient();

        var dto = new CrearTorneoDTO
        {
            Nombre = "Torneo Completo",
            Anio = 2024,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true,
            PrimeraFase = new FaseDTO
            {
                Nombre = "Fase inicial",
                Numero = 1,
                TipoDeFase = TipoDeFaseEnum.TodosContraTodos,
                EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
                EsVisibleEnApp = true
            },
            Categorias =
            [
                new TorneoCategoriaDTO { Nombre = "Sub-15", AnioDesde = 2010, AnioHasta = 2015 }
            ]
        };
        var crearResponse = await client.PostAsJsonAsync("/api/torneo", dto);
        crearResponse.EnsureSuccessStatusCode();
        var torneoCreado = JsonConvert.DeserializeObject<CrearTorneoDTO>(await crearResponse.Content.ReadAsStringAsync());
        Assert.NotNull(torneoCreado);

        var response = await client.GetAsync($"/api/torneo/{torneoCreado.Id}");
        response.EnsureSuccessStatusCode();

        var torneo = JsonConvert.DeserializeObject<TorneoDTO>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(torneo);
        Assert.NotEmpty(torneo.Fases!);
        Assert.Equal("Fase inicial", torneo.Fases![0].Nombre);
        Assert.NotEmpty(torneo.Categorias!);
        Assert.Equal("Sub-15", torneo.Categorias![0].Nombre);
    }

    [Fact]
    public async Task ModificarTorneo_ListaVaciaDeFasesYCategorias_BorraTodas()
    {
        var client = await GetAuthenticatedClient();

        var crearDto = new CrearTorneoDTO
        {
            Nombre = "Torneo a Vaciar",
            Anio = 2023,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true,
            PrimeraFase = new FaseDTO
            {
                Nombre = "Fase",
                Numero = 1,
                TipoDeFase = TipoDeFaseEnum.TodosContraTodos,
                EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
                EsVisibleEnApp = true
            },
            Categorias = [new TorneoCategoriaDTO { Nombre = "Cat", AnioDesde = 2010, AnioHasta = 2015 }]
        };
        var crearResponse = await client.PostAsJsonAsync("/api/torneo", crearDto);
        crearResponse.EnsureSuccessStatusCode();
        var torneoCreado = JsonConvert.DeserializeObject<TorneoDTO>(await crearResponse.Content.ReadAsStringAsync());
        Assert.NotNull(torneoCreado);

        var modificarDto = new TorneoDTO
        {
            Id = torneoCreado.Id,
            Nombre = "Torneo a Vaciar",
            Anio = 2023,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true,
            Fases = [],
            Categorias = []
        };

        var putResponse = await client.PutAsJsonAsync($"/api/torneo/{torneoCreado.Id}", modificarDto);
        putResponse.EnsureSuccessStatusCode();

        var getResponse = await client.GetAsync($"/api/torneo/{torneoCreado.Id}");
        getResponse.EnsureSuccessStatusCode();
        var torneo = JsonConvert.DeserializeObject<TorneoDTO>(await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(torneo);
        Assert.Empty(torneo.Fases!);
        Assert.Empty(torneo.Categorias!);
    }

    [Fact]
    public async Task ModificarTorneo_ConNuevasFasesYCategorias_Reemplaza()
    {
        var client = await GetAuthenticatedClient();

        var crearDto = new CrearTorneoDTO
        {
            Nombre = "Torneo a Reemplazar",
            Anio = 2022,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true,
            PrimeraFase = new FaseDTO
            {
                Nombre = "Fase vieja",
                Numero = 1,
                TipoDeFase = TipoDeFaseEnum.TodosContraTodos,
                EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
                EsVisibleEnApp = true
            },
            Categorias = [new TorneoCategoriaDTO { Nombre = "Cat vieja", AnioDesde = 2010, AnioHasta = 2015 }]
        };
        var crearResponse = await client.PostAsJsonAsync("/api/torneo", crearDto);
        crearResponse.EnsureSuccessStatusCode();
        var torneoCreado = JsonConvert.DeserializeObject<TorneoDTO>(await crearResponse.Content.ReadAsStringAsync());
        Assert.NotNull(torneoCreado);

        var modificarDto = new TorneoDTO
        {
            Id = torneoCreado.Id,
            Nombre = "Torneo a Reemplazar",
            Anio = 2022,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true,
            Fases =
            [
                new FaseDTO
                {
                    Nombre = "Fase nueva",
                    Numero = 1,
                    TipoDeFase = TipoDeFaseEnum.TodosContraTodos,
                    EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
                    EsVisibleEnApp = false,
                }
            ],
            Categorias =
            [
                new TorneoCategoriaDTO { Nombre = "Sub-18", AnioDesde = 2007, AnioHasta = 2009 }
            ]
        };

        var putResponse = await client.PutAsJsonAsync($"/api/torneo/{torneoCreado.Id}", modificarDto);
        putResponse.EnsureSuccessStatusCode();

        var getResponse = await client.GetAsync($"/api/torneo/{torneoCreado.Id}");
        getResponse.EnsureSuccessStatusCode();
        var torneo = JsonConvert.DeserializeObject<TorneoDTO>(await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(torneo);
        Assert.Single(torneo.Fases!);
        Assert.Equal("Fase nueva", torneo.Fases![0].Nombre);
        Assert.False(torneo.Fases[0].EsVisibleEnApp);
        Assert.Single(torneo.Categorias!);
        Assert.Equal("Sub-18", torneo.Categorias![0].Nombre);
    }

    [Fact]
    public async Task ModificarTorneo_SinFasesNiCategorias_NoLasModifica()
    {
        var client = await GetAuthenticatedClient();

        var crearDto = new CrearTorneoDTO
        {
            Nombre = "Torneo Sin Tocar",
            Anio = 2021,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true,
            PrimeraFase = new FaseDTO
            {
                Nombre = "Fase original",
                Numero = 1,
                TipoDeFase = TipoDeFaseEnum.TodosContraTodos,
                EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
                EsVisibleEnApp = true
            },
            Categorias = [new TorneoCategoriaDTO { Nombre = "Cat original", AnioDesde = 2010, AnioHasta = 2015 }]
        };
        var crearResponse = await client.PostAsJsonAsync("/api/torneo", crearDto);
        crearResponse.EnsureSuccessStatusCode();
        var torneoCreado = JsonConvert.DeserializeObject<TorneoDTO>(await crearResponse.Content.ReadAsStringAsync());
        Assert.NotNull(torneoCreado);

        var modificarDto = new TorneoDTO
        {
            Id = torneoCreado.Id,
            Nombre = "Torneo Sin Tocar Modificado",
            Anio = 2021,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };

        var putResponse = await client.PutAsJsonAsync($"/api/torneo/{torneoCreado.Id}", modificarDto);
        putResponse.EnsureSuccessStatusCode();

        var getResponse = await client.GetAsync($"/api/torneo/{torneoCreado.Id}");
        getResponse.EnsureSuccessStatusCode();
        var torneo = JsonConvert.DeserializeObject<TorneoDTO>(await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(torneo);
        Assert.Equal("Torneo Sin Tocar Modificado", torneo.Nombre);
        Assert.Single(torneo.Fases!);
        Assert.Equal("Fase original", torneo.Fases![0].Nombre);
        Assert.Single(torneo.Categorias!);
        Assert.Equal("Cat original", torneo.Categorias![0].Nombre);
    }

    [Fact]
    public async Task ModificarTorneo_SeVenLosGolesEnTablaDePosiciones_Persiste()
    {
        var client = await GetAuthenticatedClient();

        var crear = new CrearTorneoDTO
        {
            Nombre = "Torneo flag goles",
            Anio = 2020,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };
        var crearResp = await client.PostAsJsonAsync("/api/torneo", crear);
        crearResp.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<TorneoDTO>(await crearResp.Content.ReadAsStringAsync());
        Assert.NotNull(creado);
        Assert.True(creado.SeVenLosGolesEnTablaDePosiciones);

        var modificar = new TorneoDTO
        {
            Id = creado.Id,
            Nombre = creado.Nombre,
            Anio = creado.Anio,
            TorneoAgrupadorId = creado.TorneoAgrupadorId,
            EsVisibleEnApp = creado.EsVisibleEnApp,
            SeVenLosGolesEnTablaDePosiciones = false
        };
        var putResp = await client.PutAsJsonAsync($"/api/torneo/{creado.Id}", modificar);
        putResp.EnsureSuccessStatusCode();

        var getResp = await client.GetAsync($"/api/torneo/{creado.Id}");
        getResp.EnsureSuccessStatusCode();
        var final = JsonConvert.DeserializeObject<TorneoDTO>(await getResp.Content.ReadAsStringAsync());
        Assert.NotNull(final);
        Assert.False(final.SeVenLosGolesEnTablaDePosiciones);
    }

    /// <summary>
    /// PUT con la lista completa de categorías (mismos ids) no debe disparar la validación de “eliminar”
    /// cuando hay partidos asociados a esas categorías.
    /// </summary>
    [Fact]
    public async Task ModificarTorneo_ConTodasLasCategoriasDelTorneoInclusoConPartidosAsociados_204()
    {
        var client = await GetAuthenticatedClient();

        var crearDto = new CrearTorneoDTO
        {
            Nombre = "Torneo cat con partidos",
            Anio = 2018,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true,
            PrimeraFase = new FaseDTO
            {
                Nombre = "Fase",
                Numero = 1,
                TipoDeFase = TipoDeFaseEnum.TodosContraTodos,
                EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
                EsVisibleEnApp = true
            },
            Categorias =
            [
                new TorneoCategoriaDTO { Nombre = "Cat uno", AnioDesde = 2010, AnioHasta = 2015 },
                new TorneoCategoriaDTO { Nombre = "Cat dos", AnioDesde = 2010, AnioHasta = 2015 }
            ]
        };
        var crearResponse = await client.PostAsJsonAsync("/api/torneo", crearDto);
        crearResponse.EnsureSuccessStatusCode();
        var torneoCreado = JsonConvert.DeserializeObject<TorneoDTO>(await crearResponse.Content.ReadAsStringAsync());
        Assert.NotNull(torneoCreado);
        var torneoId = torneoCreado.Id;

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var zona = await context.Zonas.OfType<ZonaTodosContraTodos>()
                .FirstAsync(z => z.Fase.TorneoId == torneoId);
            var club = new Club
            {
                Id = 0,
                Nombre = "Club seed partido cat",
                Localidad = "X",
                Direccion = "Y",
                EsTechado = false
            };
            context.Clubs.Add(club);
            await context.SaveChangesAsync();

            var eq1 = new Equipo { Id = 0, Nombre = "Eq A", ClubId = club.Id, Jugadores = [] };
            var eq2 = new Equipo { Id = 0, Nombre = "Eq B", ClubId = club.Id, Jugadores = [] };
            context.Equipos.AddRange(eq1, eq2);
            await context.SaveChangesAsync();

            context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = eq1.Id, ZonaId = zona.Id });
            context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = eq2.Id, ZonaId = zona.Id });

            var fecha = new FechaTodosContraTodos
            {
                Id = 0,
                Dia = new DateOnly(2026, 3, 1),
                Numero = 1,
                ZonaId = zona.Id,
                EsVisibleEnApp = true
            };
            context.Fechas.Add(fecha);
            await context.SaveChangesAsync();

            var jornada = new JornadaNormal
            {
                Id = 0,
                FechaId = fecha.Id,
                ResultadosVerificados = false,
                LocalEquipoId = eq1.Id,
                VisitanteEquipoId = eq2.Id,
                Partidos = []
            };
            context.Jornadas.Add(jornada);
            await context.SaveChangesAsync();

            var primeraCategoriaId = await context.TorneoCategorias
                .Where(c => c.TorneoId == torneoId)
                .OrderBy(c => c.Id)
                .Select(c => c.Id)
                .FirstAsync();

            context.Partidos.Add(new Partido
            {
                Id = 0,
                CategoriaId = primeraCategoriaId,
                JornadaId = jornada.Id,
                ResultadoLocal = "1",
                ResultadoVisitante = "0"
            });
            await context.SaveChangesAsync();
        }

        var getResponse = await client.GetAsync($"/api/torneo/{torneoId}");
        getResponse.EnsureSuccessStatusCode();
        var torneoActual = JsonConvert.DeserializeObject<TorneoDTO>(await getResponse.Content.ReadAsStringAsync());
        Assert.NotNull(torneoActual?.Categorias);
        Assert.Equal(2, torneoActual.Categorias!.Count);

        var modificarDto = new TorneoDTO
        {
            Id = torneoActual.Id,
            Nombre = "Torneo cat con partidos renombrado",
            Anio = torneoActual.Anio,
            TorneoAgrupadorId = torneoActual.TorneoAgrupadorId,
            EsVisibleEnApp = torneoActual.EsVisibleEnApp,
            SeVenLosGolesEnTablaDePosiciones = torneoActual.SeVenLosGolesEnTablaDePosiciones,
            Categorias = torneoActual.Categorias!.Select(c => new TorneoCategoriaDTO
            {
                Id = c.Id,
                Nombre = c.Nombre,
                AnioDesde = c.AnioDesde,
                AnioHasta = c.AnioHasta,
                TorneoId = torneoId
            }).ToList()
        };

        var putResponse = await client.PutAsJsonAsync($"/api/torneo/{torneoId}", modificarDto);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);
    }

    [Fact]
    public async Task CrearYListar_ObtenerPorId_FaseAperturaYClausura_Null()
    {
        var client = await GetAuthenticatedClient();
        var crear = new CrearTorneoDTO
        {
            Nombre = "Torneo sin fases tabla anual",
            Anio = 2031,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };
        var crearResp = await client.PostAsJsonAsync("/api/torneo", crear);
        crearResp.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<TorneoDTO>(await crearResp.Content.ReadAsStringAsync());
        Assert.NotNull(creado);

        var getResp = await client.GetAsync($"/api/torneo/{creado.Id}");
        getResp.EnsureSuccessStatusCode();
        var torneo = JsonConvert.DeserializeObject<TorneoDTO>(await getResp.Content.ReadAsStringAsync());
        Assert.NotNull(torneo);
        Assert.Null(torneo.FaseAperturaId);
        Assert.Null(torneo.FaseClausuraId);
        Assert.Null(torneo.FaseAperturaNombre);
        Assert.Null(torneo.FaseClausuraNombre);
    }

    [Fact]
    public async Task EditarFasesParaTablaAnual_YObtenerPorId_DevuelveIds()
    {
        var client = await GetAuthenticatedClient();
        var crear = new CrearTorneoDTO
        {
            Nombre = "Torneo tabla anual",
            Anio = 2032,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };
        var crearResp = await client.PostAsJsonAsync("/api/torneo", crear);
        crearResp.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<TorneoDTO>(await crearResp.Content.ReadAsStringAsync());
        Assert.NotNull(creado);
        var torneoId = creado.Id;

        var fase2Dto = new FaseDTO
        {
            Nombre = "Fase 2",
            Numero = 2,
            TipoDeFase = TipoDeFaseEnum.TodosContraTodos,
            EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
            EsVisibleEnApp = true
        };
        var postFase2 = await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/fases", fase2Dto);
        postFase2.EnsureSuccessStatusCode();

        var fasesResp = await client.GetAsync($"/api/Torneo/{torneoId}/fases");
        fasesResp.EnsureSuccessStatusCode();
        var fases = JsonConvert.DeserializeObject<List<FaseDTO>>(await fasesResp.Content.ReadAsStringAsync());
        Assert.NotNull(fases);
        Assert.Equal(2, fases.Count);
        var fase1Id = fases.Single(f => f.Numero == 1).Id;
        var fase2Id = fases.Single(f => f.Numero == 2).Id;

        var editar = new EditarFasesParaTablaAnualDTO
        {
            FaseAperturaId = fase1Id,
            FaseClausuraId = fase2Id
        };
        var putFases = await client.PutAsJsonAsync($"/api/torneo/{torneoId}/fases-para-tabla-anual", editar);
        putFases.EnsureSuccessStatusCode();

        var getResp = await client.GetAsync($"/api/torneo/{torneoId}");
        getResp.EnsureSuccessStatusCode();
        var torneo = JsonConvert.DeserializeObject<TorneoDTO>(await getResp.Content.ReadAsStringAsync());
        Assert.NotNull(torneo);
        Assert.Equal(fase1Id, torneo.FaseAperturaId);
        Assert.Equal(fase2Id, torneo.FaseClausuraId);
        Assert.Equal(fases.Single(f => f.Numero == 1).Nombre, torneo.FaseAperturaNombre);
        Assert.Equal(fases.Single(f => f.Numero == 2).Nombre, torneo.FaseClausuraNombre);
    }

    [Fact]
    public async Task PutTorneoGeneral_NoModificaFaseAperturaNiClausura()
    {
        var client = await GetAuthenticatedClient();
        var crear = new CrearTorneoDTO
        {
            Nombre = "Torneo put no toca fases anuales",
            Anio = 2033,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };
        var crearResp = await client.PostAsJsonAsync("/api/torneo", crear);
        crearResp.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<TorneoDTO>(await crearResp.Content.ReadAsStringAsync());
        Assert.NotNull(creado);
        var torneoId = creado.Id;

        var fasesResp = await client.GetAsync($"/api/Torneo/{torneoId}/fases");
        fasesResp.EnsureSuccessStatusCode();
        var fases = JsonConvert.DeserializeObject<List<FaseDTO>>(await fasesResp.Content.ReadAsStringAsync());
        Assert.NotNull(fases);
        var fase1Id = fases[0].Id;

        var fase2Dto = new FaseDTO
        {
            Nombre = "Fase clausura put test",
            Numero = 2,
            TipoDeFase = TipoDeFaseEnum.TodosContraTodos,
            EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
            EsVisibleEnApp = true
        };
        var postFase2 = await client.PostAsJsonAsync($"/api/Torneo/{torneoId}/fases", fase2Dto);
        postFase2.EnsureSuccessStatusCode();
        var fasesTras2 = await client.GetAsync($"/api/Torneo/{torneoId}/fases");
        fasesTras2.EnsureSuccessStatusCode();
        var lista2 = JsonConvert.DeserializeObject<List<FaseDTO>>(await fasesTras2.Content.ReadAsStringAsync());
        Assert.NotNull(lista2);
        var fase2Id = lista2.Single(f => f.Numero == 2).Id;

        await client.PutAsJsonAsync($"/api/torneo/{torneoId}/fases-para-tabla-anual",
            new EditarFasesParaTablaAnualDTO { FaseAperturaId = fase1Id, FaseClausuraId = fase2Id });

        var modificarDto = new TorneoDTO
        {
            Id = torneoId,
            Nombre = "Torneo renombrado put",
            Anio = 2033,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = false
        };
        var putResp = await client.PutAsJsonAsync($"/api/torneo/{torneoId}", modificarDto);
        putResp.EnsureSuccessStatusCode();

        var getResp = await client.GetAsync($"/api/torneo/{torneoId}");
        getResp.EnsureSuccessStatusCode();
        var final = JsonConvert.DeserializeObject<TorneoDTO>(await getResp.Content.ReadAsStringAsync());
        Assert.NotNull(final);
        Assert.Equal("Torneo renombrado put", final.Nombre);
        Assert.False(final.SeVenLosGolesEnTablaDePosiciones);
        Assert.Equal(fase1Id, final.FaseAperturaId);
        Assert.Equal(fase2Id, final.FaseClausuraId);
    }

    [Fact]
    public async Task EditarFasesParaTablaAnual_AperturaYClausuraIguales_400()
    {
        var client = await GetAuthenticatedClient();
        var crear = new CrearTorneoDTO
        {
            Nombre = "Torneo validación misma fase",
            Anio = 2036,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };
        var crearResp = await client.PostAsJsonAsync("/api/torneo", crear);
        crearResp.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<TorneoDTO>(await crearResp.Content.ReadAsStringAsync());
        Assert.NotNull(creado);
        var fasesResp = await client.GetAsync($"/api/Torneo/{creado.Id}/fases");
        fasesResp.EnsureSuccessStatusCode();
        var fases = JsonConvert.DeserializeObject<List<FaseDTO>>(await fasesResp.Content.ReadAsStringAsync());
        Assert.NotNull(fases);
        var faseId = fases[0].Id;

        var put = await client.PutAsJsonAsync($"/api/torneo/{creado.Id}/fases-para-tabla-anual",
            new EditarFasesParaTablaAnualDTO { FaseAperturaId = faseId, FaseClausuraId = faseId });
        Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
    }

    [Fact]
    public async Task EditarFasesParaTablaAnual_FaseEliminacionDirecta_400()
    {
        var client = await GetAuthenticatedClient();
        var crear = new CrearTorneoDTO
        {
            Nombre = "Torneo validación ED",
            Anio = 2037,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };
        var crearResp = await client.PostAsJsonAsync("/api/torneo", crear);
        crearResp.EnsureSuccessStatusCode();
        var creado = JsonConvert.DeserializeObject<TorneoDTO>(await crearResp.Content.ReadAsStringAsync());
        Assert.NotNull(creado);
        var edDto = new FaseDTO
        {
            Nombre = "Playoffs",
            Numero = 2,
            TipoDeFase = TipoDeFaseEnum.EliminacionDirecta,
            EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
            EsVisibleEnApp = false
        };
        var postEd = await client.PostAsJsonAsync($"/api/Torneo/{creado.Id}/fases", edDto);
        postEd.EnsureSuccessStatusCode();
        var faseEd = JsonConvert.DeserializeObject<FaseDTO>(await postEd.Content.ReadAsStringAsync());
        Assert.NotNull(faseEd);

        var put = await client.PutAsJsonAsync($"/api/torneo/{creado.Id}/fases-para-tabla-anual",
            new EditarFasesParaTablaAnualDTO { FaseAperturaId = faseEd.Id, FaseClausuraId = null });
        Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
    }

    [Fact]
    public async Task EditarFasesParaTablaAnual_FaseDeOtroTorneo_400()
    {
        var client = await GetAuthenticatedClient();

        var crear1 = new CrearTorneoDTO
        {
            Nombre = "Torneo A tabla anual",
            Anio = 2034,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };
        var crear2 = new CrearTorneoDTO
        {
            Nombre = "Torneo B tabla anual",
            Anio = 2035,
            TorneoAgrupadorId = 1,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true
        };
        var r1 = await client.PostAsJsonAsync("/api/torneo", crear1);
        var r2 = await client.PostAsJsonAsync("/api/torneo", crear2);
        r1.EnsureSuccessStatusCode();
        r2.EnsureSuccessStatusCode();
        var t1 = JsonConvert.DeserializeObject<TorneoDTO>(await r1.Content.ReadAsStringAsync());
        var t2 = JsonConvert.DeserializeObject<TorneoDTO>(await r2.Content.ReadAsStringAsync());
        Assert.NotNull(t1);
        Assert.NotNull(t2);

        var fasesB = await client.GetAsync($"/api/Torneo/{t2.Id}/fases");
        fasesB.EnsureSuccessStatusCode();
        var listaB = JsonConvert.DeserializeObject<List<FaseDTO>>(await fasesB.Content.ReadAsStringAsync());
        Assert.NotNull(listaB);
        var faseOtroTorneo = listaB[0].Id;

        var put = await client.PutAsJsonAsync($"/api/torneo/{t1.Id}/fases-para-tabla-anual",
            new EditarFasesParaTablaAnualDTO { FaseAperturaId = faseOtroTorneo, FaseClausuraId = null });
        Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
    }
}
