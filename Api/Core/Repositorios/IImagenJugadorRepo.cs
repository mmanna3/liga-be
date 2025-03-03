using Api.Core.DTOs;

namespace Api.Core.Repositorios
{
	public interface IImagenJugadorRepo
	{
		// void GuardarFotoDeJugadorDesdeArchivo(JugadorPendienteDeAprobacionDTO vm);
		string GetFotoEnBase64(string dni);
		// void GuardarImagenJugadorImportado(string dni, byte[] fotoByteArray);
		void Eliminar(string dni);
		void EliminarLista(IList<string> dni);
		string Path(string dni);
		void CambiarDNI(string dniAnterior, string dniNuevo);
		string PathFotoTemporalCarnet(string dni);
		string PathFotoTemporalDNIFrente(string dni);
		string PathFotoTemporalDNIDorso(string dni);
		void FicharJugadorTemporal(string dniJugadorTemporal);
		void GuardarFotosTemporalesDeJugadorAutofichado(JugadorPendienteDeAprobacionDTO vm);
		void GuardarFotosTemporalesDeJugadorAutofichadoSiendoEditado(JugadorPendienteDeAprobacionDTO vm);
		void RenombrarFotosTemporalesPorCambioDeDNI(string dniAnterior, string dni);
	}
}