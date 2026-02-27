using Api.Core.DTOs;

namespace Api.Core.Repositorios
{
	public interface IImagenJugadorRepo : IImagenPersonaFichadaRepo
	{
		void FicharJugadorTemporal(string dni);
		/// <summary>
		/// Copia las fotos de un delegado existente a temporales de jugador, para poder fichar como jugador a una persona que ya es delegado.
		/// </summary>
		void CopiarFotosDeDelegadoATemporales(string dni);
		void GuardarFotosTemporalesDeJugadorAutofichado(JugadorDTO vm);
		void GuardarFotosTemporalesDeJugadorAutofichadoSiendoEditado(JugadorDTO vm);
		void EliminarFotosDelJugador(string dni);
	}
}