using Api.Core.DTOs;

namespace Api.Core.Repositorios
{
	public interface IImagenJugadorRepo
	{
		// void GuardarFotoDeJugadorDesdeArchivo(JugadorDTO vm);
		string GetFotoCarnetEnBase64(string dni);
		string GetFotoEnBase64ConPathAbsoluto(string pathAbsoluto);
		// void GuardarImagenJugadorImportado(string dni, byte[] fotoByteArray);
		void Eliminar(string dni);
		void EliminarLista(IList<string> dni);
		string Path(string dni);
		void CambiarDNI(string dniAnterior, string dniNuevo);
		string PathFotoTemporalCarnet(string dni);
		string PathFotoTemporalDNIFrente(string dni);
		string PathFotoTemporalDNIDorso(string dni);
		void FicharJugadorTemporal(string dniJugadorTemporal);
		void GuardarFotosTemporalesDeJugadorAutofichado(JugadorDTO vm);
		void GuardarFotosTemporalesDeJugadorAutofichadoSiendoEditado(JugadorDTO vm);
		void RenombrarFotosTemporalesPorCambioDeDNI(string dniAnterior, string dni);
		void EliminarFotosDelJugador(string dni);
	}
}