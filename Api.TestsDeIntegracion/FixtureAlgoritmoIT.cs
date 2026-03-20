using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.TestsDeIntegracion._Config;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion;

public class FixtureAlgoritmoIT : TestBase
{
    private static FixtureAlgoritmoDTO CrearDtoValidoN4() => new()
    {
        FixtureAlgoritmoId = 0,
        CantidadDeEquipos = 4,
        Nombre = "Test",
        Fechas =
        [
            new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 1, EquipoVisitante = 2 },
            new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 3, EquipoVisitante = 4 },
            new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 2, EquipoVisitante = 3 },
            new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 4, EquipoVisitante = 1 },
            new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 3, EquipoVisitante = 1 },
            new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 2, EquipoVisitante = 4 }
        ]
    };

    public FixtureAlgoritmoIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Crear_Listar_ObtenerPorId_YModificar_FlujoCompletoConN4()
    {
        var client = await GetAuthenticatedClient();

        // 1. Crear
        var dtoCrear = CrearDtoValidoN4();
        var responseCrear = await client.PostAsJsonAsync("/api/FixtureAlgoritmo", dtoCrear);
        responseCrear.EnsureSuccessStatusCode();
        var creado = await responseCrear.Content.ReadFromJsonAsync<FixtureAlgoritmoDTO>();
        Assert.NotNull(creado);
        Assert.True(creado.Id > 0);
        Assert.Equal(4, creado.CantidadDeEquipos);
        Assert.Equal(6, creado.Fechas.Count);

        // 2. Listar
        var responseListar = await client.GetAsync("/api/FixtureAlgoritmo");
        responseListar.EnsureSuccessStatusCode();
        var listados = JsonConvert.DeserializeObject<List<FixtureAlgoritmoDTO>>(await responseListar.Content.ReadAsStringAsync());
        Assert.NotNull(listados);
        var encontrado = listados.FirstOrDefault(f => f.Id == creado.Id);
        Assert.NotNull(encontrado);
        Assert.Equal(6, encontrado.Fechas.Count);
        Assert.Contains(encontrado.Fechas, f => f.Fecha == 1 && f.EquipoLocal == 1 && f.EquipoVisitante == 2);

        // 3. Obtener por id
        var responseGet = await client.GetAsync($"/api/FixtureAlgoritmo/{creado.Id}");
        responseGet.EnsureSuccessStatusCode();
        var obtenido = await responseGet.Content.ReadFromJsonAsync<FixtureAlgoritmoDTO>();
        Assert.NotNull(obtenido);
        Assert.Equal(creado.Id, obtenido.Id);
        Assert.Equal(4, obtenido.CantidadDeEquipos);
        Assert.Equal(6, obtenido.Fechas.Count);
        var fecha1 = obtenido.Fechas.First(f => f.Fecha == 2 && f.EquipoLocal == 4);
        Assert.Equal(1, fecha1.EquipoVisitante);

        // 4. Modificar (nuevas fechas, mismo esquema round-robin)
        var dtoModificar = new FixtureAlgoritmoDTO
        {
            Id = creado.Id,
            FixtureAlgoritmoId = creado.Id,
            CantidadDeEquipos = 4,
            Nombre = creado.Nombre,
            Fechas =
            [
                new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 1, EquipoVisitante = 3 },
                new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 2, EquipoVisitante = 4 },
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 1, EquipoVisitante = 4 },
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 2, EquipoVisitante = 3 },
                new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 1, EquipoVisitante = 2 },
                new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 3, EquipoVisitante = 4 }
            ]
        };
        var responsePut = await client.PutAsJsonAsync($"/api/FixtureAlgoritmo/{creado.Id}", dtoModificar);
        responsePut.EnsureSuccessStatusCode();

        // 5. Verificar modificación
        var responseGet2 = await client.GetAsync($"/api/FixtureAlgoritmo/{creado.Id}");
        responseGet2.EnsureSuccessStatusCode();
        var modificado = await responseGet2.Content.ReadFromJsonAsync<FixtureAlgoritmoDTO>();
        Assert.NotNull(modificado);
        Assert.Equal(6, modificado.Fechas.Count);
        var primeraFecha = modificado.Fechas.First(f => f.Fecha == 1);
        Assert.Equal(1, primeraFecha.EquipoLocal);
        Assert.Equal(3, primeraFecha.EquipoVisitante);
    }

    [Fact]
    public async Task Crear_ConN4YCantidadCorrectaDeFechas_GuardaCorrectamente()
    {
        var client = await GetAuthenticatedClient();
        var dto = CrearDtoValidoN4();

        var response = await client.PostAsJsonAsync("/api/FixtureAlgoritmo", dto);

        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<FixtureAlgoritmoDTO>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal(4, created.CantidadDeEquipos);
        Assert.Equal(6, created.Fechas.Count);
    }

    [Fact]
    public async Task Crear_ConN4YCantidadIncorrectaDeFechas_DevuelveBadRequest()
    {
        var client = await GetAuthenticatedClient();

        var dto = new FixtureAlgoritmoDTO
        {
            FixtureAlgoritmoId = 0,
            CantidadDeEquipos = 4,
            Nombre = "Test",
            Fechas =
            [
                new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 1, EquipoVisitante = 2 },
                new FixtureAlgoritmoFechaDTO { Fecha = 1, EquipoLocal = 3, EquipoVisitante = 4 },
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 2, EquipoVisitante = 3 },
                new FixtureAlgoritmoFechaDTO { Fecha = 2, EquipoLocal = 4, EquipoVisitante = 1 },
                new FixtureAlgoritmoFechaDTO { Fecha = 3, EquipoLocal = 3, EquipoVisitante = 1 }
                // Solo 5 fechas, deberían ser 6
            ]
        };

        var response = await client.PostAsJsonAsync("/api/FixtureAlgoritmo", dto);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("6", content);
        Assert.Contains("5", content);
    }
}
