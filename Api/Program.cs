using System.Net;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using Api._Config;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;
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

    var app = builder.Build();

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

    // CORS habilitado en desarrollo y en cualquier entorno E2E (tests de frontend
    // necesitan hacer requests cross-origin desde localhost:5173 al backend).
    if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("E2E_SEED_ENABLED") == "true")
    {
        app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
    }

    app.UseOpenApi();
    app.UseExceptionHandler();

    // app.UseHttpsRedirection();

    app.UseDefaultFiles();
    app.UseStaticFiles();

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