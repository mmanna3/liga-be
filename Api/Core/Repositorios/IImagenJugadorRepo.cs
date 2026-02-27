using Api.Core.DTOs;

namespace Api.Core.Repositorios
{
	public interface IImagenJugadorRepo : IImagenPersonaFichadaRepo
	{
		void FicharJugadorTemporal(string dni);
		void GuardarFotosTemporalesDeJugadorAutofichado(JugadorDTO vm);
		void GuardarFotosTemporalesDeJugadorAutofichadoSiendoEditado(JugadorDTO vm);
		void EliminarFotosDelJugador(string dni);
	}
}