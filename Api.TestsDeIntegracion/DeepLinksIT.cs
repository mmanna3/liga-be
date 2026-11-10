using System.Net;
using System.Text.Json;
using Api.TestsDeIntegracion._Config;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Api.TestsDeIntegracion;

public class DeepLinksIT : TestBase
{
    private const string AppStore = "https://apps.apple.com/app/id123";
    private const string PlayStore = "https://play.google.com/store/apps/details?id=com.ykn.test";
    private const string UaIPhone = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 Mobile/15E148";
    private const string UaAndroid = "Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 Chrome/120 Mobile Safari/537.36";
    private const string UaEscritorio = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/120 Safari/537.36";

    public DeepLinksIT(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private WebApplicationFactory<Program> ConConfig(string? webRoot = null) =>
        Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DeepLinks:AppleTeamId"] = "TEAM123456",
                ["DeepLinks:IosBundleId"] = "com.ykn.test",
                ["DeepLinks:AndroidPackage"] = "com.ykn.test",
                ["DeepLinks:AndroidSha256Fingerprints:0"] = "AA:BB",
                ["DeepLinks:AndroidSha256Fingerprints:1"] = "CC:DD",
                ["DeepLinks:AppStoreUrl"] = AppStore,
                ["DeepLinks:PlayStoreUrl"] = PlayStore
            }));
            if (webRoot != null) builder.UseWebRoot(webRoot);
        });

    private static HttpClient ClienteSinRedirect(WebApplicationFactory<Program> factory, string userAgent)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        return client;
    }

    [Fact]
    public async Task AppleAppSiteAssociation_ConConfig_200JsonConAppId()
    {
        var client = ConConfig().CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync("/.well-known/apple-app-site-association");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("application/json", response.Content.Headers.ContentType!.MediaType);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var detalle = doc.RootElement.GetProperty("applinks").GetProperty("details")[0];
        Assert.Equal("TEAM123456.com.ykn.test", detalle.GetProperty("appIDs")[0].GetString());
        Assert.Equal("/registro-arbitro", detalle.GetProperty("components")[0].GetProperty("/").GetString());
    }

    [Fact]
    public async Task AssetLinks_ConConfig_200JsonConHuellas()
    {
        var client = ConConfig().CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync("/.well-known/assetlinks.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("application/json", response.Content.Headers.ContentType!.MediaType);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var item = doc.RootElement[0];
        Assert.Equal("delegate_permission/common.handle_all_urls", item.GetProperty("relation")[0].GetString());
        var target = item.GetProperty("target");
        Assert.Equal("android_app", target.GetProperty("namespace").GetString());
        Assert.Equal("com.ykn.test", target.GetProperty("package_name").GetString());
        var huellas = target.GetProperty("sha256_cert_fingerprints").EnumerateArray().Select(h => h.GetString()).ToArray();
        Assert.Equal(new[] { "AA:BB", "CC:DD" }, huellas);
    }

    [Fact]
    public async Task RegistroArbitro_IPhone_RedirigeAlAppStore()
    {
        var client = ClienteSinRedirect(ConConfig(), UaIPhone);
        var response = await client.GetAsync("/registro-arbitro");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal(AppStore, response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task RegistroArbitro_Android_RedirigeAlPlayStore()
    {
        var client = ClienteSinRedirect(ConConfig(), UaAndroid);
        var response = await client.GetAsync("/registro-arbitro");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal(PlayStore, response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task RegistroArbitro_Escritorio_HtmlConLosDosLinks()
    {
        var client = ClienteSinRedirect(ConConfig(), UaEscritorio);
        var response = await client.GetAsync("/registro-arbitro");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("text/html", response.Content.Headers.ContentType!.MediaType);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Descargá la app de EDeFI para registrarte como árbitro", html);
        Assert.Contains(AppStore, html);
        Assert.Contains("id=com.ykn.test", html);
    }

    [Theory]
    [InlineData("/.well-known/apple-app-site-association")]
    [InlineData("/.well-known/assetlinks.json")]
    [InlineData("/registro-arbitro")]
    public async Task SinConfig_404(string ruta)
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync(ruta);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FallbackDelSpa_SigueSirviendoIndexHtml()
    {
        var webRoot = Path.Combine(Path.GetTempPath(), "liga-webroot-it-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(webRoot);
        try
        {
            File.WriteAllText(Path.Combine(webRoot, "index.html"), "<html>spa-index</html>");
            var client = ConConfig(webRoot).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            var response = await client.GetAsync("/alguna-ruta-del-front");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("spa-index", await response.Content.ReadAsStringAsync());
        }
        finally
        {
            if (Directory.Exists(webRoot))
                Directory.Delete(webRoot, recursive: true);
        }
    }
}
