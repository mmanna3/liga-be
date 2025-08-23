using Api.Core.Logica;
using Api.Persistencia.Repositorios;
using SkiaSharp;

namespace Api.TestsUnitarios;

public class ImagenJugadorRepoTest
{
    private class AppPathsTest : AppPaths
    {
        public AppPathsTest(string root) : base(new DummyEnv(root))
        {
            ImagenesTemporalesJugadorCarnetAbsolute = GetAbsolutePath(ImagenesTemporalesJugadorCarnetRelative);
            ImagenesTemporalesJugadorDNIFrenteAbsolute = GetAbsolutePath(ImagenesTemporalesJugadorDNIFrenteRelative);
            ImagenesTemporalesJugadorDNIDorsoAbsolute = GetAbsolutePath(ImagenesTemporalesJugadorDNIDorsoRelative);
        }

        protected override string GetAbsolutePath(string relativePath)
        {
            return Path.Combine(Env.ContentRootPath, relativePath.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar));
        }

        public override string BackupAbsoluteOf(string fileNameWithExtension) => Path.Combine(Env.ContentRootPath, fileNameWithExtension);
        public override string BackupAbsolute() => Env.ContentRootPath;

        private class DummyEnv : Microsoft.AspNetCore.Hosting.IWebHostEnvironment
        {
            public DummyEnv(string root) { ContentRootPath = root; }
            public string WebRootPath { get; set; } = string.Empty;
            public string EnvironmentName { get; set; } = "Development";
            public string ApplicationName { get; set; } = "Tests";
            public string ContentRootPath { get; set; }
            public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = null!;
            public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
        }
    }

    [Fact]
    public void FicharJugadorTemporal_MueveFotoYCorrigeExtensiones()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var paths = new AppPathsTest(tempDir);
        var repo = new ImagenJugadorRepo(paths);
        const string dni = "12345678";

        Directory.CreateDirectory(paths.ImagenesTemporalesJugadorCarnetAbsolute);
        Directory.CreateDirectory(paths.ImagenesTemporalesJugadorDNIFrenteAbsolute);
        Directory.CreateDirectory(paths.ImagenesTemporalesJugadorDNIDorsoAbsolute);

        // Creamos una imagen PNG válida
        using (var bitmap = new SKBitmap(1, 1))
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
        {
            File.WriteAllBytes(Path.Combine(paths.ImagenesTemporalesJugadorCarnetAbsolute, $"{dni}.png"), data.ToArray());
            File.WriteAllBytes(Path.Combine(paths.ImagenesTemporalesJugadorDNIFrenteAbsolute, $"{dni}.png"), data.ToArray());
            File.WriteAllBytes(Path.Combine(paths.ImagenesTemporalesJugadorDNIDorsoAbsolute, $"{dni}.png"), data.ToArray());
        }

        // Act
        repo.FicharJugadorTemporal(dni);

        // Assert
        var pathDefinitivo = Path.Combine(paths.ImagenesJugadoresAbsolute, $"{dni}.jpg");
        Assert.True(File.Exists(pathDefinitivo), "La imagen definitiva no existe");
        Assert.False(File.Exists(Path.Combine(paths.ImagenesTemporalesJugadorCarnetAbsolute, $"{dni}.png")), "La imagen temporal carnet no fue eliminada");
        Assert.False(File.Exists(Path.Combine(paths.ImagenesTemporalesJugadorDNIFrenteAbsolute, $"{dni}.png")), "La imagen temporal DNI frente no fue eliminada");
        Assert.False(File.Exists(Path.Combine(paths.ImagenesTemporalesJugadorDNIDorsoAbsolute, $"{dni}.png")), "La imagen temporal DNI dorso no fue eliminada");
    }

}
