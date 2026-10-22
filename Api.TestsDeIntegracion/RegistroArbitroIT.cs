using System.Net;
using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Api.TestsUtilidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Api.TestsDeIntegracion;

public class RegistroArbitroIT : TestBase
{
    private int _agrupadorFutsalId;
    private int _agrupadorBabyId;
    private int _equipoId;

    public RegistroArbitroIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedData(context);
    }

    private void SeedData(AppDbContext context)
    {
        // El seed "General" (Id=1, no visible en la app) ya existe por HasData en AppDbContext
        var futsal = new TorneoAgrupador { Id = 0, Nombre = "Futsal", EsVisibleEnApp = true, ColorId = (int)ColorEnum.Azul };
        var baby = new TorneoAgrupador { Id = 0, Nombre = "Baby", EsVisibleEnApp = true, ColorId = (int)ColorEnum.Rojo };
        var oculto = new TorneoAgrupador { Id = 0, Nombre = "Oculto", EsVisibleEnApp = false, ColorId = (int)ColorEnum.Verde };
        context.TorneoAgrupadores.AddRange(futsal, baby, oculto);
        context.SaveChanges();
        _agrupadorFutsalId = futsal.Id;
        _agrupadorBabyId = baby.Id;

        var util = new Utilidades(context);
        var equipo = util.DadoQueExisteElEquipo(util.DadoQueExisteElClub());
        context.SaveChanges();
        _equipoId = equipo!.Id;
    }

    private RegistroArbitroDTO RegistroValido(string dni = "30111222") => new()
    {
        DNI = dni,
        Nombre = "  Juan ",
        Apellido = " Pérez  ",
        TelefonoCelular = "+5491112345678",
        Direccion = "  Calle 12 ",
        Localidad = " Lanús ",
        FechaNacimiento = new DateTime(1995, 5, 20),
        TipoMovilidad = TipoMovilidadArbitroEnum.Moto,
        TorneoAgrupadorIds = [_agrupadorFutsalId, _agrupadorBabyId],
        FranjasHorarias = [FranjaHorariaArbitroEnum.Sabados8A14, FranjaHorariaArbitroEnum.Domingos15A22],
        TieneRelacionInstitucional = true,
        Institucion = " Club Atlético "
    };

    private async Task<int> RegistrarPendiente(string dni = "30111222")
    {
        var response = await Factory.CreateClient().PostAsJsonAsync("/api/publico/registro-arbitro", RegistroValido(dni));
        response.EnsureSuccessStatusCode();
        return JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
    }

    private static async Task<string?> TituloDelError(HttpResponseMessage response)
    {
        var problema = JsonConvert.DeserializeObject<ProblemDetails>(await response.Content.ReadAsStringAsync());
        return problema?.Title;
    }

    private Arbitro ObtenerArbitroDeLaBase(int id)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return context.Arbitros
            .Include(a => a.ArbitroTorneoAgrupadores)
            .Include(a => a.ArbitroEquiposProhibidos)
            .Include(a => a.ArbitroFranjasHorarias)
            .AsNoTracking()
            .Single(a => a.Id == id);
    }

    [Fact]
    public async Task TorneoAgrupadoresParaArbitros_SinLogin_DevuelveSoloVisiblesOrdenadosPorNombre()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/publico/torneo-agrupadores-para-arbitros");

        response.EnsureSuccessStatusCode();
        var content = JsonConvert.DeserializeObject<List<TorneoAgrupadorParaArbitroDTO>>(
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Equal(new[] { "Baby", "Futsal" }, content.Select(a => a.Nombre));
        Assert.All(content, a => Assert.True(a.Id > 0));
    }

    [Fact]
    public async Task Registro_SinLogin_CreaArbitroPendienteConTodosLosDatos()
    {
        var id = await RegistrarPendiente();

        var arbitro = ObtenerArbitroDeLaBase(id);
        Assert.Equal((int)EstadoArbitroEnum.PendienteDeAprobacion, arbitro.EstadoArbitroId);
        Assert.Equal("30111222", arbitro.DNI);
        Assert.Equal("Juan", arbitro.Nombre);
        Assert.Equal("Pérez", arbitro.Apellido);
        Assert.Equal("+5491112345678", arbitro.TelefonoCelular);
        Assert.Equal("Calle 12", arbitro.Direccion);
        Assert.Equal("Lanús", arbitro.Localidad);
        Assert.Equal(new DateTime(1995, 5, 20), arbitro.FechaNacimiento);
        Assert.Equal((int)TipoMovilidadArbitroEnum.Moto, arbitro.TipoMovilidadArbitroId);
        Assert.True(arbitro.TieneRelacionInstitucional);
        Assert.Equal("Club Atlético", arbitro.Institucion);
        Assert.Equal(
            new[] { _agrupadorFutsalId, _agrupadorBabyId }.OrderBy(x => x),
            arbitro.ArbitroTorneoAgrupadores.Select(x => x.TorneoAgrupadorId).OrderBy(x => x));
        Assert.Equal(
            new[] { (int)FranjaHorariaArbitroEnum.Sabados8A14, (int)FranjaHorariaArbitroEnum.Domingos15A22 },
            arbitro.ArbitroFranjasHorarias.Select(x => x.FranjaHorariaArbitroId).OrderBy(x => x));
        Assert.Empty(arbitro.ArbitroEquiposProhibidos);
    }

    [Fact]
    public async Task Registro_SinRelacionInstitucional_NoGuardaInstitucion()
    {
        var dto = RegistroValido();
        dto.TieneRelacionInstitucional = false;
        dto.Institucion = "Club que no corresponde";

        var response = await Factory.CreateClient().PostAsJsonAsync("/api/publico/registro-arbitro", dto);

        response.EnsureSuccessStatusCode();
        var id = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
        var arbitro = ObtenerArbitroDeLaBase(id);
        Assert.False(arbitro.TieneRelacionInstitucional);
        Assert.Null(arbitro.Institucion);
    }

    [Fact]
    public async Task Registro_DniDeUnArbitroActivo_400ConMensaje()
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Arbitros.Add(new Arbitro { Id = 0, DNI = "30111222", Nombre = "Otro", Apellido = "Árbitro" });
            context.SaveChanges();
        }

        var response = await Factory.CreateClient().PostAsJsonAsync("/api/publico/registro-arbitro", RegistroValido());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Ya hay un árbitro registrado con ese DNI", await TituloDelError(response));
    }

    [Fact]
    public async Task Registro_DniDeUnArbitroPendiente_400ConMensaje()
    {
        await RegistrarPendiente();

        var response = await Factory.CreateClient().PostAsJsonAsync("/api/publico/registro-arbitro", RegistroValido());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Ya hay un árbitro registrado con ese DNI", await TituloDelError(response));
    }

    [Fact]
    public async Task Registro_ConRelacionInstitucionalSinInstitucion_400()
    {
        var dto = RegistroValido();
        dto.Institucion = "   ";

        var response = await Factory.CreateClient().PostAsJsonAsync("/api/publico/registro-arbitro", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("1112345678")]
    [InlineData("+549111234567")]
    [InlineData("+54911123456789")]
    [InlineData("+5491212345678")]
    public async Task Registro_TelefonoConFormatoInvalido_400(string telefono)
    {
        var dto = RegistroValido();
        dto.TelefonoCelular = telefono;

        var response = await Factory.CreateClient().PostAsJsonAsync("/api/publico/registro-arbitro", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("1234567890")]
    [InlineData("30.111.222")]
    public async Task Registro_DniInvalido_400(string dni)
    {
        var response = await Factory.CreateClient().PostAsJsonAsync("/api/publico/registro-arbitro", RegistroValido(dni));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Registro_NombreDeMasDe50Caracteres_400()
    {
        var dto = RegistroValido();
        dto.Nombre = new string('N', 51);

        var response = await Factory.CreateClient().PostAsJsonAsync("/api/publico/registro-arbitro", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Registro_SinFranjasHorarias_400()
    {
        var dto = RegistroValido();
        dto.FranjasHorarias = [];

        var response = await Factory.CreateClient().PostAsJsonAsync("/api/publico/registro-arbitro", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Registro_SinTorneos_400()
    {
        var dto = RegistroValido();
        dto.TorneoAgrupadorIds = [];

        var response = await Factory.CreateClient().PostAsJsonAsync("/api/publico/registro-arbitro", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Registro_FranjaHorariaInexistente_400()
    {
        var dto = RegistroValido();
        dto.FranjasHorarias = [(FranjaHorariaArbitroEnum)99];

        var response = await Factory.CreateClient().PostAsJsonAsync("/api/publico/registro-arbitro", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Registro_TipoMovilidadInexistente_400()
    {
        var dto = RegistroValido();
        dto.TipoMovilidad = (TipoMovilidadArbitroEnum)99;

        var response = await Factory.CreateClient().PostAsJsonAsync("/api/publico/registro-arbitro", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Registro_SinTipoMovilidad_400()
    {
        var dto = RegistroValido();
        dto.TipoMovilidad = null;

        var response = await Factory.CreateClient().PostAsJsonAsync("/api/publico/registro-arbitro", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Registro_AgrupadorInexistente_400()
    {
        var dto = RegistroValido();
        dto.TorneoAgrupadorIds = [9999];

        var response = await Factory.CreateClient().PostAsJsonAsync("/api/publico/registro-arbitro", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Pendientes_DevuelveSoloPendientesConTodosSusDatos()
    {
        var pendienteId = await RegistrarPendiente();
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Arbitros.Add(new Arbitro { Id = 0, DNI = "28000111", Nombre = "Activo", Apellido = "Existente" });
            context.SaveChanges();
        }
        var client = await GetAuthenticatedClient();

        var response = await client.GetAsync("/api/arbitro/pendientes");

        response.EnsureSuccessStatusCode();
        var content = JsonConvert.DeserializeObject<List<ArbitroDTO>>(await response.Content.ReadAsStringAsync());
        var pendiente = Assert.Single(content!);
        Assert.Equal(pendienteId, pendiente.Id);
        Assert.Equal("Calle 12", pendiente.Direccion);
        Assert.Equal("Lanús", pendiente.Localidad);
        Assert.Equal(new DateTime(1995, 5, 20), pendiente.FechaNacimiento);
        Assert.Equal(TipoMovilidadArbitroEnum.Moto, pendiente.TipoMovilidad);
        Assert.Equal(
            new[] { FranjaHorariaArbitroEnum.Sabados8A14, FranjaHorariaArbitroEnum.Domingos15A22 },
            pendiente.FranjasHorarias);
        Assert.True(pendiente.TieneRelacionInstitucional);
        Assert.Equal("Club Atlético", pendiente.Institucion);
        Assert.Equal(2, pendiente.TorneoAgrupadorIds.Count);
    }

    [Fact]
    public async Task Pendientes_SinLogin_401()
    {
        var response = await Factory.CreateClient().GetAsync("/api/arbitro/pendientes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ListarArbitros_NoIncluyePendientes()
    {
        await RegistrarPendiente();
        var client = await GetAuthenticatedClient();

        var response = await client.GetAsync("/api/arbitro");

        response.EnsureSuccessStatusCode();
        var content = JsonConvert.DeserializeObject<List<ArbitroDTO>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(content);
        Assert.Empty(content);
    }

    [Fact]
    public async Task Aprobar_Pendiente_PasaAActivoYPisaLasRelaciones()
    {
        var id = await RegistrarPendiente();
        var client = await GetAuthenticatedClient();

        var response = await client.PostAsJsonAsync($"/api/arbitro/{id}/aprobar", new AprobarArbitroDTO
        {
            TorneoAgrupadorIds = [_agrupadorBabyId],
            EquipoProhibidoIds = [_equipoId]
        });

        response.EnsureSuccessStatusCode();
        var arbitro = ObtenerArbitroDeLaBase(id);
        Assert.Equal((int)EstadoArbitroEnum.Activo, arbitro.EstadoArbitroId);
        Assert.Equal(new[] { _agrupadorBabyId }, arbitro.ArbitroTorneoAgrupadores.Select(x => x.TorneoAgrupadorId));
        Assert.Equal(new[] { _equipoId }, arbitro.ArbitroEquiposProhibidos.Select(x => x.EquipoId));
        Assert.Equal(2, arbitro.ArbitroFranjasHorarias.Count);

        var listado = JsonConvert.DeserializeObject<List<ArbitroDTO>>(
            await (await client.GetAsync("/api/arbitro")).Content.ReadAsStringAsync());
        Assert.Contains(listado!, a => a.Id == id);
    }

    [Fact]
    public async Task Aprobar_ArbitroActivo_400()
    {
        var id = await RegistrarPendiente();
        var client = await GetAuthenticatedClient();
        await client.PostAsJsonAsync($"/api/arbitro/{id}/aprobar", new AprobarArbitroDTO());

        var response = await client.PostAsJsonAsync($"/api/arbitro/{id}/aprobar", new AprobarArbitroDTO());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("El árbitro no está pendiente de aprobación.", await TituloDelError(response));
    }

    [Fact]
    public async Task Aprobar_ArbitroInexistente_404()
    {
        var client = await GetAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/arbitro/9999/aprobar", new AprobarArbitroDTO());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Rechazar_Pendiente_LoElimina()
    {
        var id = await RegistrarPendiente();
        var client = await GetAuthenticatedClient();

        var response = await client.DeleteAsync($"/api/arbitro/{id}");

        response.EnsureSuccessStatusCode();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(context.Arbitros.Any(a => a.Id == id));
        Assert.False(context.ArbitroFranjaHoraria.Any(a => a.ArbitroId == id));
        Assert.False(context.ArbitroTorneoAgrupador.Any(a => a.ArbitroId == id));
    }

    [Fact]
    public async Task ModificarPendienteDesdeElAbm_SiguePendiente()
    {
        var id = await RegistrarPendiente();
        var client = await GetAuthenticatedClient();
        var dto = JsonConvert.DeserializeObject<ArbitroDTO>(
            await (await client.GetAsync($"/api/arbitro/{id}")).Content.ReadAsStringAsync())!;
        dto.Localidad = "Avellaneda";
        dto.FranjasHorarias = [FranjaHorariaArbitroEnum.MartesAViernes20A2330];

        var response = await client.PutAsJsonAsync($"/api/arbitro/{id}", dto);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var arbitro = ObtenerArbitroDeLaBase(id);
        Assert.Equal((int)EstadoArbitroEnum.PendienteDeAprobacion, arbitro.EstadoArbitroId);
        Assert.Equal("Avellaneda", arbitro.Localidad);
        Assert.Equal(
            new[] { (int)FranjaHorariaArbitroEnum.MartesAViernes20A2330 },
            arbitro.ArbitroFranjasHorarias.Select(x => x.FranjaHorariaArbitroId));
    }
}
