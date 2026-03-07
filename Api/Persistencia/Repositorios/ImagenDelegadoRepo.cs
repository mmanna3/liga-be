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
			CopiarCarnetATemporales($"{ImagenesDefinitivasAbsolute}/{dni}.jpg", dni, origenEnTemporalesCarnet: false);
		}

		public void CopiarFotosDeJugadorATemporales(string dni)
		{
			var (pathOrigen, origenEnTemporalesCarnet) = ObtenerPathFotoJugador(dni);
			CopiarCarnetATemporales(pathOrigen, dni, origenEnTemporalesCarnet);
		}

		private (string Path, bool OrigenEnTemporalesCarnet) ObtenerPathFotoJugador(string dni)
		{
			var pathJpg = System.IO.Path.Combine(Paths.ImagenesJugadoresAbsolute, $"{dni}.jpg");
			if (File.Exists(pathJpg))
				return (pathJpg, false);

			if (Directory.Exists(Paths.ImagenesJugadoresAbsolute))
			{
				var archivosJugador = Directory.GetFiles(Paths.ImagenesJugadoresAbsolute, $"{dni}.*");
				if (archivosJugador.Length > 0)
					return (archivosJugador[0], false);
			}

			if (Directory.Exists(Paths.ImagenesTemporalesCarnetAbsolute))
			{
				var archivosTemporales = Directory.GetFiles(Paths.ImagenesTemporalesCarnetAbsolute, $"{dni}.*");
				if (archivosTemporales.Length > 0)
					return (archivosTemporales[0], true);
			}

			throw new ExcepcionControlada("No se encontraron fotos para el DNI indicado");
		}

		private void CopiarCarnetATemporales(string pathOrigen, string dni, bool origenEnTemporalesCarnet)
		{
			if (!File.Exists(pathOrigen))
				throw new ExcepcionControlada("No se encontraron fotos para el DNI indicado");

			Directory.CreateDirectory(Paths.ImagenesTemporalesCarnetAbsolute);
			Directory.CreateDirectory(Paths.ImagenesTemporalesDNIFrenteAbsolute);
			Directory.CreateDirectory(Paths.ImagenesTemporalesDNIDorsoAbsolute);

			var pathCarnet = $"{Paths.ImagenesTemporalesCarnetAbsolute}/{dni}.jpg";
			var pathFrente = $"{Paths.ImagenesTemporalesDNIFrenteAbsolute}/{dni}.jpg";
			var pathDorso = $"{Paths.ImagenesTemporalesDNIDorsoAbsolute}/{dni}.jpg";

			if (!origenEnTemporalesCarnet)
				File.Copy(pathOrigen, pathCarnet, overwrite: true);
			File.Copy(pathOrigen, pathFrente, overwrite: true);
			File.Copy(pathOrigen, pathDorso, overwrite: true);
		}
	}
}
