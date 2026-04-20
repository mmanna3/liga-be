using System.Net.Http.Json;
using Api.Core.DTOs.AppCarnetDigital;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Logica;
using Api.Core.Servicios;
using Api.Persistencia._Config;
using Api.TestsDeIntegracion._Config;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;

namespace Api.TestsDeIntegracion;

public class AppCarnetDigitalIT : TestBase
{
    private int _zonaIdDePrueba;

    public AppCarnetDigitalIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var paths = scope.ServiceProvider.GetRequiredService<AppPaths>();
        
        SeedData(context, paths);
    }

    private void SeedData(AppDbContext context, AppPaths paths)
    {
        // HasData crea "General" con EsVisibleEnApp=false; lo activamos para probar info-inicial-de-torneos.
        var agrupadorGeneral = context.TorneoAgrupadores.Find(1);
        if (agrupadorGeneral != null)
        {
            agrupadorGeneral.EsVisibleEnApp = true;
            context.SaveChanges();
        }

        // Crear un club
        var club = new Club
        {
            Id = 1,
            Nombre = "Club de Prueba",
            Localidad = "Rosario",
            Direccion = "Calle Falsa 123",
            EsTechado = true
        };
        context.Clubs.Add(club);

        // Crear un torneo (año = calendario actual: info-inicial-de-torneos lista todos los visibles en app)
        var anioActual = DateTime.Today.Year;
        var torneo = new Torneo
        {
            Id = 1,
            Nombre = "Torneo 2024",
            Anio = anioActual,
            EsVisibleEnApp = true,
            SeVenLosGolesEnTablaDePosiciones = true,
            TorneoAgrupadorId = 1
        };
        context.Torneos.Add(torneo);
        context.SaveChanges();
        torneo = context.Torneos.First(t => t.Nombre == "Torneo 2024");

        var fase = new FaseTodosContraTodos { Id = 0, Nombre = "", TorneoId = torneo.Id, Numero = 1, EstadoFaseId = 100, EsVisibleEnApp = true };
        context.Fases.Add(fase);
        context.SaveChanges();
        var zona = new ZonaTodosContraTodos { Id = 0, FaseId = fase.Id, Nombre = "Zona única" };
        context.Zonas.Add(zona);
        context.SaveChanges();
        _zonaIdDePrueba = zona.Id;

        // Crear un equipo
        var equipo = new Equipo
        {
            Id = 1,
            Nombre = "Equipo de Prueba",
            ClubId = 1,
            Jugadores = new List<JugadorEquipo>(),
            Zonas = new List<EquipoZona>()
        };
        context.Equipos.Add(equipo);
        context.SaveChanges();
        context.EquipoZona.Add(new EquipoZona { Id = 0, EquipoId = equipo.Id, ZonaId = zona.Id });

        // Crear jugadores
        var jugador1 = new Jugador
        {
            Id = 1,
            Nombre = "Juan",
            Apellido = "Pérez",
            DNI = "12345678",
            FechaNacimiento = new DateTime(2000, 1, 1)
        };

        var jugador2 = new Jugador
        {
            Id = 2,
            Nombre = "Pedro",
            Apellido = "González",
            DNI = "87654321",
            FechaNacimiento = new DateTime(2001, 2, 2)
        };

        context.Jugadores.AddRange(jugador1, jugador2);
        
        // Guardar para obtener los IDs
        context.SaveChanges();

        // Asociar jugadores al equipo con diferentes estados
        var jugadorEquipo1 = new JugadorEquipo
        {
            Id = 1,
            JugadorId = 1,
            EquipoId = 1,
            Equipo = equipo,
            Jugador = jugador1,
            EstadoJugadorId = 3, // Activo
            FechaFichaje = DateTime.Now
        };

        var jugadorEquipo2 = new JugadorEquipo
        {
            Id = 2,
            JugadorId = 2,
            EquipoId = 1,
            Equipo = equipo,
            Jugador = jugador2,
            EstadoJugadorId = 1, // Fichaje pendiente de aprobación
            FechaFichaje = DateTime.Now
        };

        equipo.Jugadores.Add(jugadorEquipo1);
        equipo.Jugadores.Add(jugadorEquipo2);
        context.JugadorEquipo.AddRange(jugadorEquipo1, jugadorEquipo2);
        
        // Guardar los cambios
        context.SaveChanges();

        // Crear delegado y usuario para el endpoint equipos-del-delegado
        var estadoActivo = context.EstadoDelegado.First(e => e.Estado == "Activo");
        var delegado = new Delegado
        {
            Id = 1,
            DNI = "11111111",
            Nombre = "Delegado",
            Apellido = "Test",
            FechaNacimiento = new DateTime(1990, 1, 1),
            DelegadoClubs = new List<DelegadoClub>()
        };
        context.Delegados.Add(delegado);
        context.DelegadoClub.Add(new DelegadoClub { Id = 1, DelegadoId = delegado.Id, ClubId = club.Id, EstadoDelegadoId = estadoActivo.Id });

        // Delegado con mismo DNI que Juan (jugador) para probar que aparece en ambas listas
        var delegadoMismoDniQueJuan = new Delegado
        {
            Id = 2,
            DNI = "12345678",
            Nombre = "Juan",
            Apellido = "Pérez",
            FechaNacimiento = new DateTime(2000, 1, 1),
            DelegadoClubs = new List<DelegadoClub>()
        };
        context.Delegados.Add(delegadoMismoDniQueJuan);
        context.DelegadoClub.Add(new DelegadoClub { Id = 2, DelegadoId = delegadoMismoDniQueJuan.Id, ClubId = club.Id, EstadoDelegadoId = estadoActivo.Id });

        context.SaveChanges();

        var rolDelegado = context.Roles.First(r => r.Nombre == "Delegado");
        var usuarioDelegado = new Usuario
        {
            Id = 100,
            NombreUsuario = "delegadoTest",
            Password = AuthCore.HashPassword("delegado123"),
            RolId = rolDelegado.Id,
            DelegadoId = delegado.Id
        };
        context.Usuarios.Add(usuarioDelegado);
        context.SaveChanges();

        // Crear directorio de imágenes y agregar una imagen de prueba
        Directory.CreateDirectory(paths.ImagenesJugadoresAbsolute);
        Directory.CreateDirectory(paths.ImagenesDelegadosAbsolute);
        Directory.CreateDirectory(paths.ImagenesEscudosAbsolute);

        // Crear una imagen de prueba de 240x240 píxeles
        using var bitmap = new SKBitmap(240, 240);
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint();
        paint.Color = SKColors.Blue;
        canvas.DrawRect(0, 0, 240, 240, paint);

        // Guardar la imagen para el primer jugador
        using (var stream1 = new FileStream($"{paths.ImagenesJugadoresAbsolute}/12345678.jpg", FileMode.Create))
        {
            using var image1 = SKImage.FromBitmap(bitmap);
            using var data1 = image1.Encode(SKEncodedImageFormat.Jpeg, 75);
            data1.SaveTo(stream1);
        }

        // Guardar la imagen para el segundo jugador
        using (var stream2 = new FileStream($"{paths.ImagenesJugadoresAbsolute}/87654321.jpg", FileMode.Create))
        {
            using var image2 = SKImage.FromBitmap(bitmap);
            using var data2 = image2.Encode(SKEncodedImageFormat.Jpeg, 75);
            data2.SaveTo(stream2);
        }

        // Guardar la imagen para el delegado (mismo club que el equipo)
        using (var streamDelegado = new FileStream($"{paths.ImagenesDelegadosAbsolute}/11111111.jpg", FileMode.Create))
        {
            using var imageDelegado = SKImage.FromBitmap(bitmap);
            using var dataDelegado = imageDelegado.Encode(SKEncodedImageFormat.Jpeg, 75);
            dataDelegado.SaveTo(streamDelegado);
        }

        // Escudo del club 1 (endpoint clubes: ruta con archivo propio)
        using (var streamEscudo = new FileStream($"{paths.ImagenesEscudosAbsolute}/1.jpg", FileMode.Create))
        {
            using var imageEscudo = SKImage.FromBitmap(bitmap);
            using var dataEscudo = imageEscudo.Encode(SKEncodedImageFormat.Jpeg, 75);
            dataEscudo.SaveTo(streamEscudo);
        }
    }
    
    [Fact]
    public async Task Carnets_EquipoExistente_DevuelveJugadoresYDelegadosDelClub()
    {
        var client = await GetAuthenticatedClient();

        var response = await client.GetAsync("/api/carnet-digital/carnets?equipoId=1");

        response.EnsureSuccessStatusCode();

        var carnets = await response.Content.ReadFromJsonAsync<List<CarnetDigitalDTO>>();

        Assert.NotNull(carnets);
        // 2 delegados + 1 jugador activo (primero delegados, después jugadores)
        Assert.Equal(3, carnets.Count);

        var delegados = carnets.Where(c => c.EsDelegado).ToList();
        var jugadores = carnets.Where(c => !c.EsDelegado).ToList();
        Assert.Equal(2, delegados.Count);
        Assert.Single(jugadores);
        Assert.True(carnets.Take(2).All(c => c.EsDelegado)); // Delegados primero
        Assert.False(carnets.Last().EsDelegado); // Jugadores después

        var carnetJugador = jugadores.Single();
        Assert.Equal("Juan", carnetJugador.Nombre);
        Assert.Equal("Pérez", carnetJugador.Apellido);
        Assert.Equal("12345678", carnetJugador.DNI);
        Assert.Equal((int)EstadoJugadorEnum.Activo, carnetJugador.Estado);
        Assert.Equal("Equipo de Prueba", carnetJugador.Equipo);

        var carnetDelegadoTest = delegados.Single(c => c.DNI == "11111111");
        Assert.Equal("Delegado", carnetDelegadoTest.Nombre);
        Assert.Equal("Test", carnetDelegadoTest.Apellido);
        Assert.Equal((int)EstadoDelegadoEnum.Activo, carnetDelegadoTest.Estado);
        Assert.Equal("Club de Prueba", carnetDelegadoTest.Equipo);
    }

    [Fact]
    public async Task Carnets_JugadorYDelegadoMismoDNI_ApareceEnAmbasListas()
    {
        var client = await GetAuthenticatedClient();

        var response = await client.GetAsync("/api/carnet-digital/carnets?equipoId=1");

        response.EnsureSuccessStatusCode();

        var carnets = await response.Content.ReadFromJsonAsync<List<CarnetDigitalDTO>>();

        Assert.NotNull(carnets);
        // Primero delegados (2), después jugadores (1). Juan es delegado Y jugador: aparece en ambas listas.
        Assert.Equal(3, carnets.Count);
        Assert.Equal(2, carnets.Count(c => c.DNI == "12345678")); // Juan como delegado y como jugador
        Assert.True(carnets.First(c => c.DNI == "12345678").EsDelegado); // Primera aparición como delegado
        Assert.False(carnets.Last(c => c.DNI == "12345678").EsDelegado); // Segunda aparición como jugador
    }
    
    [Fact]
    public async Task JugadoresPendientes_EquipoExistente_DevuelveCarnetsCorrectos()
    {
        var client = await GetAuthenticatedClient();
        
        var response = await client.GetAsync("/api/carnet-digital/jugadores-pendientes?equipoId=1");
        
        response.EnsureSuccessStatusCode();
        
        var carnets = await response.Content.ReadFromJsonAsync<List<CarnetDigitalPendienteDTO>>();
        
        Assert.NotNull(carnets);
        Assert.Single(carnets);
        
        var segundoCarnet = carnets.First();
        Assert.Equal("Pedro", segundoCarnet.Nombre);
        Assert.Equal("González", segundoCarnet.Apellido);
        Assert.Equal("87654321", segundoCarnet.DNI);
        Assert.Equal(1, segundoCarnet.Estado); // EstadoJugador.Pendiente
    }

    [Fact]
    public async Task Carnets_EquipoInexistente_Devuelve404()
    {
        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync("/api/carnet-digital/carnets?equipoId=999");
        
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task EquiposDelDelegado_DelegadoConClubYEquipos_DevuelveEquipos()
    {
        var client = Factory.CreateClient();
        client = await AuthTestHelper.GetAuthenticatedClient(client, "delegadoTest", "delegado123");

        var response = await client.GetAsync("/api/carnet-digital/equipos-del-delegado");

        response.EnsureSuccessStatusCode();

        var resultado = await response.Content.ReadFromJsonAsync<EquiposDelDelegadoDTO>();

        Assert.NotNull(resultado);
        Assert.NotNull(resultado.ClubsConEquipos);
        Assert.NotEmpty(resultado.ClubsConEquipos);

        var clubConEquipos = resultado.ClubsConEquipos.First();
        Assert.Equal("Club de Prueba", clubConEquipos.Nombre);
        Assert.NotNull(clubConEquipos.Equipos);
        Assert.NotEmpty(clubConEquipos.Equipos);

        var equipo = clubConEquipos.Equipos.First();
        Assert.Equal(1, equipo.Id);
        Assert.Equal("Equipo de Prueba", equipo.Nombre);
        Assert.Equal("Torneo 2024", equipo.Torneo);
        Assert.NotNull(equipo.CodigoAlfanumerico);
    }

    [Fact]
    public async Task InformacionInicialDeTorneos_DevuelveAgrupadorTorneoFaseZona_SoloIdYNombre()
    {
        var client = await GetAuthenticatedClient();

        var response = await client.GetAsync("/api/carnet-digital/info-inicial-de-torneos");

        response.EnsureSuccessStatusCode();

        var lista = await response.Content.ReadFromJsonAsync<List<InformacionInicialAgrupadorDTO>>();
        Assert.NotNull(lista);

        var agrupador = Assert.Single(lista, a => a.Id == 1 && a.Nombre == "General");
        Assert.Equal("Negro", agrupador.Color);
        var torneo = Assert.Single(agrupador.Torneos, t => t.Id == 1 && t.Nombre == "Torneo 2024");

        var fase = Assert.Single(torneo.Fases);
        Assert.Equal(1, fase.Id);
        Assert.Equal(string.Empty, fase.Nombre);
        Assert.Equal("TodosContraTodos", fase.TipoDeFase);

        var zona = Assert.Single(fase.Zonas);
        Assert.True(zona.Id > 0);
        Assert.Equal("Zona única", zona.Nombre);
    }

    [Fact]
    public async Task InformacionInicialDeTorneos_TorneoDeOtroAnio_ConcatenaAnioAlNombre()
    {
        var anioActual = DateTime.Today.Year;
        int torneoId;
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var torneo = new Torneo
            {
                Id = 0,
                Nombre = "Futsal Mayores",
                Anio = anioActual - 1,
                EsVisibleEnApp = true,
                SeVenLosGolesEnTablaDePosiciones = true,
                TorneoAgrupadorId = 1
            };
            ctx.Torneos.Add(torneo);
            await ctx.SaveChangesAsync();
            ctx.Fases.Add(new FaseTodosContraTodos
            {
                Id = 0,
                Nombre = "",
                TorneoId = torneo.Id,
                Numero = 1,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            });
            await ctx.SaveChangesAsync();
            torneoId = torneo.Id;
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync("/api/carnet-digital/info-inicial-de-torneos");
        response.EnsureSuccessStatusCode();
        var lista = await response.Content.ReadFromJsonAsync<List<InformacionInicialAgrupadorDTO>>();
        Assert.NotNull(lista);
        var agrupador = Assert.Single(lista, a => a.Id == 1);
        var torneoDto = Assert.Single(agrupador.Torneos, t => t.Id == torneoId);
        Assert.Equal($"Futsal Mayores {anioActual - 1}", torneoDto.Nombre);
    }

    [Fact]
    public async Task InformacionInicialDeTorneos_ConAperturaYClausura_AgregaFaseAnualYZonasSinDuplicarNombre()
    {
        var anioActual = DateTime.Today.Year;
        int torneoId;
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var torneo = new Torneo
            {
                Id = 0,
                Nombre = "Torneo tabla anual app",
                Anio = anioActual,
                EsVisibleEnApp = true,
                SeVenLosGolesEnTablaDePosiciones = true,
                TorneoAgrupadorId = 1
            };
            ctx.Torneos.Add(torneo);
            await ctx.SaveChangesAsync();

            var fApertura = new FaseTodosContraTodos
            {
                Id = 0,
                Nombre = "Apertura",
                TorneoId = torneo.Id,
                Numero = 1,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            var fClausura = new FaseTodosContraTodos
            {
                Id = 0,
                Nombre = "Clausura",
                TorneoId = torneo.Id,
                Numero = 2,
                EstadoFaseId = 100,
                EsVisibleEnApp = true
            };
            ctx.Fases.AddRange(fApertura, fClausura);
            await ctx.SaveChangesAsync();

            ctx.Zonas.Add(new ZonaTodosContraTodos { Id = 0, FaseId = fApertura.Id, Nombre = "Norte" });
            ctx.Zonas.Add(new ZonaTodosContraTodos { Id = 0, FaseId = fClausura.Id, Nombre = "  Norte  " });
            ctx.Zonas.Add(new ZonaTodosContraTodos { Id = 0, FaseId = fClausura.Id, Nombre = "Sur" });
            torneo.FaseAperturaId = fApertura.Id;
            torneo.FaseClausuraId = fClausura.Id;
            await ctx.SaveChangesAsync();
            torneoId = torneo.Id;
        }

        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync("/api/carnet-digital/info-inicial-de-torneos");
        response.EnsureSuccessStatusCode();
        var lista = await response.Content.ReadFromJsonAsync<List<InformacionInicialAgrupadorDTO>>();
        Assert.NotNull(lista);
        var agr = lista.First(a => a.Id == 1);
        var torneoDto = agr.Torneos.First(t => t.Id == torneoId);

        var anual = Assert.Single(torneoDto.Fases, f => f.TipoDeFase == "Anual" && f.Nombre == "Anual");
        Assert.Equal(0, anual.Id);
        Assert.Equal(2, anual.Zonas.Count);
        Assert.Contains(anual.Zonas, z => z.Nombre == "Norte");
        Assert.Contains(anual.Zonas, z => z.Nombre == "Sur");
    }

    [Fact]
    public async Task Clubes_ZonaConEquipos_DevuelveUnaFilaPorEquipo_ConDatosDelClub()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/carnet-digital/clubes?zonaId={_zonaIdDePrueba}");

        response.EnsureSuccessStatusCode();

        var lista = await response.Content.ReadFromJsonAsync<List<ClubesDTO>>();
        Assert.NotNull(lista);
        Assert.Single(lista);

        var dto = lista[0];
        Assert.Equal("Equipo de Prueba", dto.Equipo);
        Assert.Equal("/Imagenes/Escudos/1.jpg", dto.Escudo);
        Assert.Equal("Rosario", dto.Localidad);
        Assert.Equal("Calle Falsa 123", dto.Direccion);
        Assert.Equal("Sí", dto.EsTechado);
        Assert.Equal(nameof(CanchaTipoEnum.Consultar), dto.TipoCancha);
    }

    [Fact]
    public async Task Clubes_ZonaSinEquipos_DevuelveListaVacia()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/carnet-digital/clubes?zonaId=999999");

        response.EnsureSuccessStatusCode();

        var lista = await response.Content.ReadFromJsonAsync<List<ClubesDTO>>();
        Assert.NotNull(lista);
        Assert.Empty(lista);
    }

    [Fact]
    public async Task FixtureTodosContraTodos_ZonaSinFechas_DevuelveFechasVacia()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/carnet-digital/fixture-todos-contra-todos?zonaId={_zonaIdDePrueba}");

        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<FixtureDTO>();
        Assert.NotNull(dto);
        Assert.NotNull(dto.Fechas);
        Assert.Empty(dto.Fechas);
    }

    [Fact]
    public async Task FixtureYJornadasTodosContraTodos_JornadasInterzonales_Numero1SinSufijo_NumeroMayorConcatenado()
    {
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var fecha = new FechaTodosContraTodos
            {
                Id = 0,
                ZonaId = _zonaIdDePrueba,
                Numero = 1,
                Dia = new DateOnly(2026, 6, 1),
                EsVisibleEnApp = true
            };
            ctx.Fechas.Add(fecha);
            await ctx.SaveChangesAsync();

            ctx.Jornadas.Add(new JornadaInterzonal
            {
                Id = 0,
                FechaId = fecha.Id,
                ResultadosVerificados = false,
                Numero = 1,
                EquipoId = 1,
                LocalOVisitanteId = (int)LocalVisitanteEnum.Local
            });
            ctx.Jornadas.Add(new JornadaInterzonal
            {
                Id = 0,
                FechaId = fecha.Id,
                ResultadosVerificados = false,
                Numero = 3,
                EquipoId = 1,
                LocalOVisitanteId = (int)LocalVisitanteEnum.Local
            });
            await ctx.SaveChangesAsync();
        }

        var client = Factory.CreateClient();

        var resFixture = await client.GetAsync(
            $"/api/carnet-digital/fixture-todos-contra-todos?zonaId={_zonaIdDePrueba}");
        resFixture.EnsureSuccessStatusCode();
        var fixture = await resFixture.Content.ReadFromJsonAsync<FixtureDTO>();
        Assert.NotNull(fixture);
        var partidos = Assert.Single(fixture.Fechas).Partidos.ToList();
        Assert.Equal(2, partidos.Count);
        Assert.Contains(partidos, p => p.Visitante == "INTERZONAL");
        Assert.Contains(partidos, p => p.Visitante == "INTERZONAL 3");

        var resJornadas = await client.GetAsync(
            $"/api/carnet-digital/jornadas-todos-contra-todos?zonaId={_zonaIdDePrueba}");
        resJornadas.EnsureSuccessStatusCode();
        var jornadasDto = await resJornadas.Content.ReadFromJsonAsync<JornadasDTO>();
        Assert.NotNull(jornadasDto);
        var filas = Assert.Single(jornadasDto.Fechas).Jornadas.ToList();
        Assert.Equal(2, filas.Count);
        Assert.Contains(filas, j => j.Visitante.Equipo == "INTERZONAL");
        Assert.Contains(filas, j => j.Visitante.Equipo == "INTERZONAL 3");
    }

    // [Fact]
    // public async Task CarnetsPorCodigoAlfanumerico_EquipoExistente_DevuelveCarnets()
    // {
    //     var client = await GetAuthenticatedClient();
    //     
    //     var response = await client.GetAsync("/api/carnet-digital/carnets?carnets-por-codigo-alfanumerico=999");
    //     
    // }
} 