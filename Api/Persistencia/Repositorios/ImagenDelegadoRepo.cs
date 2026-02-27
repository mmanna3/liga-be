using Api.Core.Logica;
using Api.Core.Otros;
using Api.Core.Repositorios;

namespace Api.Persistencia.Repositorios
{
	public class ImagenDelegadoRepo : ImagenPersonaFichadaRepoBase, IImagenDelegadoRepo
	{
		public ImagenDelegadoRepo(AppPaths paths)
			: base(paths, paths.ImagenesDelegadosAbsolute, paths.ImagenesDelegadosRelative)
		{
		}

		public void CopiarFotosDeDelegadoExistenteATemporales(string dni)
		{
			CopiarCarnetATemporales($"{ImagenesDefinitivasAbsolute}/{dni}.jpg", dni);
		}

		public void CopiarFotosDeJugadorATemporales(string dni)
		{
			var carnetJugador = $"{Paths.ImagenesJugadoresAbsolute}/{dni}.jpg";
			CopiarCarnetATemporales(carnetJugador, dni);
		}

		private void CopiarCarnetATemporales(string pathOrigen, string dni)
		{
			if (!File.Exists(pathOrigen))
				throw new ExcepcionControlada("No se encontraron fotos para el DNI indicado");

			Directory.CreateDirectory(Paths.ImagenesTemporalesCarnetAbsolute);
			Directory.CreateDirectory(Paths.ImagenesTemporalesDNIFrenteAbsolute);
			Directory.CreateDirectory(Paths.ImagenesTemporalesDNIDorsoAbsolute);

			File.Copy(pathOrigen, $"{Paths.ImagenesTemporalesCarnetAbsolute}/{dni}.jpg", overwrite: true);
			File.Copy(pathOrigen, $"{Paths.ImagenesTemporalesDNIFrenteAbsolute}/{dni}.jpg", overwrite: true);
			File.Copy(pathOrigen, $"{Paths.ImagenesTemporalesDNIDorsoAbsolute}/{dni}.jpg", overwrite: true);
		}
	}
}
