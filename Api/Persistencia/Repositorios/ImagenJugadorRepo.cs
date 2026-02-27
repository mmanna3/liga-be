using Api.Core.DTOs;
using Api.Core.Logica;
using Api.Core.Otros;
using Api.Core.Repositorios;

namespace Api.Persistencia.Repositorios
{
	public class ImagenJugadorRepo : ImagenPersonaFichadaRepoBase, IImagenJugadorRepo
	{
		public ImagenJugadorRepo(AppPaths paths)
			: base(paths, paths.ImagenesJugadoresAbsolute, paths.ImagenesJugadoresRelative)
		{
		}

		public void FicharJugadorTemporal(string dni) => FicharPersonaTemporal(dni);

		public void CopiarFotosDeDelegadoATemporales(string dni)
		{
			var carnetDelegado = $"{Paths.ImagenesDelegadosAbsolute}/{dni}.jpg";
			CopiarCarnetATemporales(carnetDelegado, dni);
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

		public void GuardarFotosTemporalesDeJugadorAutofichado(JugadorDTO vm) =>
			GuardarFotosTemporalesDePersonaFichada(vm.DNI, vm);

		public void GuardarFotosTemporalesDeJugadorAutofichadoSiendoEditado(JugadorDTO vm) =>
			GuardarFotosTemporalesDePersonaFichadaSiendoEditada(vm.DNI, vm);

		public void EliminarFotosDelJugador(string dni) => EliminarTodasLasFotos(dni);
	}
}
