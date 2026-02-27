using Api.Core.DTOs;
using Api.Core.Logica;
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

		public void GuardarFotosTemporalesDeJugadorAutofichado(JugadorDTO vm) =>
			GuardarFotosTemporalesDePersonaFichada(vm.DNI, vm);

		public void GuardarFotosTemporalesDeJugadorAutofichadoSiendoEditado(JugadorDTO vm) =>
			GuardarFotosTemporalesDePersonaFichadaSiendoEditada(vm.DNI, vm);

		public void EliminarFotosDelJugador(string dni) => EliminarTodasLasFotos(dni);
	}
}
