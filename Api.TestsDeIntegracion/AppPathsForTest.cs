using System.Reflection;
using Api.Core.Logica;
using Microsoft.AspNetCore.Hosting;

namespace Api.TestsDeIntegracion
{
	internal class AppPathsForTest : AppPaths
	{
		protected override string GetAbsolutePath(string relativePath)
		{
			// No concatenar con string: en Windows Path.Combine("D:\\...\\bin", "/Imagenes/...")
			// ignora el directorio base si el segundo segmento es "absoluto" y termina en una ruta inválida.
			var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
				?? throw new InvalidOperationException("No se pudo resolver el directorio del ensamblado de tests.");
			var segmentos = relativePath.TrimStart('/', '\\');
			return Path.Combine(assemblyDir, segmentos);
		}

		public override string BackupAbsoluteOf(string fileNameWithExtension)
		{
			return GetAbsolutePath($"/Backup/{fileNameWithExtension}");
		}

		public override string BackupAbsolute()
		{
			return GetAbsolutePath("/Backup");
		}

		public AppPathsForTest(IWebHostEnvironment env) : base(env)
		{
		}
	}
}