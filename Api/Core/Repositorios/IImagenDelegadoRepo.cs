namespace Api.Core.Repositorios
{
	public interface IImagenDelegadoRepo : IImagenPersonaFichadaRepo
	{
		/// <summary>
		/// Copia las fotos de un delegado existente (definitivas) a temporales, para poder aprobar un nuevo fichaje del mismo DNI en otro club.
		/// </summary>
		void CopiarFotosDeDelegadoExistenteATemporales(string dni);

		/// <summary>
		/// Copia las fotos de un jugador existente a temporales de delegado, para poder fichar como delegado a una persona que ya es jugador.
		/// </summary>
		void CopiarFotosDeJugadorATemporales(string dni);
	}
}
