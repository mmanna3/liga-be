namespace Api.Core.Logica
{
	public abstract class AppPaths
	{
		protected readonly IWebHostEnvironment Env;
		
		public string ImagenesRelative { get; } = "/Imagenes";
		public string ImagenesJugadoresRelative { get; } = "/Imagenes/Jugadores";
		public string ImagenesEscudosRelative { get; } = "/Imagenes/Escudos";
		public string ImagenesPublicidadesRelative { get; } = "/Imagenes/Publicidades";
		public string EscudoDefaultRelative { get; } = "/Imagenes/Escudos/default.jpg";
		public string ImagenesTemporalesJugadorCarnetRelative { get; } = "/Imagenes/Temporales/Carnet";
		public string ImagenesTemporalesJugadorDNIFrenteRelative { get; } = "/Imagenes/Temporales/DNIFrente";
		public string ImagenesTemporalesJugadorDNIDorsoRelative { get; } = "/Imagenes/Temporales/DNIDorso";
		public string BackupGeneratorExeRelative { get; } = "/Utilidades/Backup/Recursos/SchemaZen.exe";

		public string ImagenesAbsolute { get; }
		public string ImagenesJugadoresAbsolute { get; }
		public string ImagenesPublicidadesAbsolute { get; }
		public string ImagenesEscudosAbsolute { get; }
		public string EscudoDefaultFileAbsolute { get; }
		public string ImagenesTemporalesJugadorCarnetAbsolute { get; set; }
		public string ImagenesTemporalesJugadorDNIFrenteAbsolute { get; set; }
		public string ImagenesTemporalesJugadorDNIDorsoAbsolute { get; set; }
		public string BackupGeneratorExeAbsolute { get; }
		public string CarpetaTemporalBackupBaseDeDatosAbsolute { get; }

		// ReSharper disable VirtualMemberCallInConstructor
		protected AppPaths(IWebHostEnvironment env)
		{			
			Env = env;
			ImagenesAbsolute = GetAbsolutePath(ImagenesRelative);
			ImagenesJugadoresAbsolute = GetAbsolutePath(ImagenesJugadoresRelative);
			ImagenesEscudosAbsolute = GetAbsolutePath(ImagenesEscudosRelative);
			ImagenesPublicidadesAbsolute = GetAbsolutePath(ImagenesPublicidadesRelative);
			EscudoDefaultFileAbsolute = GetAbsolutePath(EscudoDefaultRelative);
			ImagenesTemporalesJugadorCarnetAbsolute = GetAbsolutePath(ImagenesTemporalesJugadorCarnetRelative);
			ImagenesTemporalesJugadorDNIFrenteAbsolute = GetAbsolutePath(ImagenesTemporalesJugadorDNIFrenteRelative);
			ImagenesTemporalesJugadorDNIDorsoAbsolute = GetAbsolutePath(ImagenesTemporalesJugadorDNIDorsoRelative);
			BackupGeneratorExeAbsolute = GetAbsolutePath(BackupGeneratorExeRelative);
			CarpetaTemporalBackupBaseDeDatosAbsolute = BackupAbsoluteOf("ZenSchemaBackup");
		}

		protected abstract string GetAbsolutePath(string relativePath);
		public abstract string BackupAbsoluteOf(string fileNameWithExtension);
		public abstract string BackupAbsolute();
		
		public string BackupImagenes()
		{
			var fileName = $"Imagenes-{FechaUtils.AhoraEnArgentinaFormatoBackup}.zip";
			return BackupAbsoluteOf(fileName);
		}

		public string BackupBaseDeDatos()
		{
			var fileName = $"BaseDeDatos-{FechaUtils.AhoraEnArgentinaFormatoBackup}.zip";
			return BackupAbsoluteOf(fileName);
		}		
	}

	public class AppPathsWebApp : AppPaths
	{
		public AppPathsWebApp(IWebHostEnvironment env) : base(env)
		{
		}
		protected override string GetAbsolutePath(string relativePath)
		{
			return Path.Combine(Env.ContentRootPath, relativePath.TrimStart('~', '/'));
		}

		public override string BackupAbsoluteOf(string fileNameWithExtension)
		{
			return Path.Combine(Env.ContentRootPath, "App_Data", fileNameWithExtension);
		}
		
		public override string BackupAbsolute()
		{
			return Path.Combine(Env.ContentRootPath, "App_Data");
		}
	}
		
}