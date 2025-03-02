using System.Reflection;
using Api.Core.Logica;
using Microsoft.AspNetCore.Hosting;

namespace Api.TestsDeIntegracion
{
	internal class AppPathsForTest : AppPaths
	{
		protected override string GetAbsolutePath(string relativePath)
		{
			var asemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			return $"{asemblyPath}{relativePath}";
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