using Api.Core.Logica;
using Api.Core.Repositorios;

namespace Api.Persistencia.Repositorios
{
	public class ImagenDelegadoRepo : ImagenPersonaFichadaRepoBase, IImagenDelegadoRepo
	{
		public ImagenDelegadoRepo(AppPaths paths)
			: base(paths, paths.ImagenesDelegadosAbsolute, paths.ImagenesDelegadosRelative)
		{
		}
	}
}
