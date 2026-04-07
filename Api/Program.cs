using System.Net;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using Api._Config;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using NLog;
using NLog.Web;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("Inició el servidor");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder = InyeccionDeDependenciasConfig.Configurar(builder);

    builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new Api._Config.DateOnlyJsonConverter());
        o.JsonSerializerOptions.Converters.Add(new Api._Config.FechaJsonConverter());
        o.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
    });

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var connectionString = builder.Configuration.GetConnectionString("Default");
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

    builder.Services.AddAutoMapper(typeof(MapperConfig));

    builder.Services.AddOpenApiDocument();


    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("EdefiWeb", policy =>
        {
            policy.WithOrigins(
                    "https://web.edefi.com.ar",
                    "https://web2.edefi.com.ar")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    var app = builder.Build();

    Console.WriteLine($"Entorno: {app.Environment.EnvironmentName}");
    Console.WriteLine($"Base de datos: {connectionString}");

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseDeveloperExceptionPage();

        var localIp = LocalIpAddress();
        app.Urls.Add($"http://0.0.0.0:5072");
        app.Urls.Add($"http://{localIp}:5072");
        // app.Urls.Add("https://" + localIp + ":7072");
    }

    // CORS: en desarrollo/E2E, abierto (localhost:5173, etc.). En el resto, solo orígenes Edefi.
    if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("E2E_SEED_ENABLED") == "true")
    {
        app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
    }
    else
    {
        app.UseCors("EdefiWeb");
    }

    app.UseOpenApi();
    app.UseExceptionHandler();

    // app.UseHttpsRedirection();

    app.UseDefaultFiles();
    app.UseStaticFiles();

    // Imágenes (escudos, jugadores, etc.) viven en ContentRoot/Imagenes/, no en wwwroot.
    var imagenesRoot = Path.Combine(app.Environment.ContentRootPath, "Imagenes");
    Directory.CreateDirectory(imagenesRoot);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(imagenesRoot),
        RequestPath = "/Imagenes"
    });

    // Habilitar la autenticación y autorización
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.MapFallbackToFile("index.html");

    // Esto es por si hay problema ejecutando las migraciones en una nueva instancia
    // using (var scope = app.Services.CreateScope())
    // {
    //     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //     db.Database.Migrate();
    // }

    app.Run();
}
catch (Exception exception)
{
    logger.Error(exception, "El servidor interrumpió su ejecución por una excepción");
    throw;
}
finally
{
    LogManager.Shutdown();
}

static string LocalIpAddress()
{
    try
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        socket.Connect("8.8.8.8", 65530);
        var endPoint = socket.LocalEndPoint as IPEndPoint;
        return endPoint != null ? endPoint.Address.ToString() : "127.0.0.1";
    }
    catch (SocketException)
    {
        Console.WriteLine("No hay conexión a internet");
        return "127.0.0.1";
    }
}

public partial class Program
{
}