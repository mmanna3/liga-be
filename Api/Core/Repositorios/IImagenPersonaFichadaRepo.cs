using Api.Core.DTOs;

namespace Api.Core.Repositorios
{
	public interface IImagenPersonaFichadaRepo
	{
		string GetFotoCarnetEnBase64(string dni);
		string GetFotoEnBase64ConPathAbsoluto(string pathAbsoluto);
		void Eliminar(string dni);
		void EliminarLista(IList<string> dni);
		string Path(string dni);
		void CambiarDNI(string dniAnterior, string dniNuevo);
		string PathFotoTemporalCarnet(string dni);
		string PathFotoTemporalDNIFrente(string dni);
		string PathFotoTemporalDNIDorso(string dni);
		void FicharPersonaTemporal(string dni);
		void GuardarFotosTemporalesDePersonaFichada(string dni, IFotosDTO fotos);
		void GuardarFotosTemporalesDePersonaFichadaSiendoEditada(string dni, IFotosDTO fotos);
		void RenombrarFotosTemporalesPorCambioDeDNI(string dniAnterior, string dni);
		void EliminarTodasLasFotos(string dni);
	}
}
